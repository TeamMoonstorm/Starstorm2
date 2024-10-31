using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Grumpy;
using SS2.Items;
namespace SS2.Components
{
    public class StickyOverloaderController : NetworkBehaviour
    {
        public static void TrySpawnBomb(CharacterBody victimBody, CharacterBody attackerBody)
        {
            var goop = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("StickyOverloaderBomb", SS2Bundle.Items), victimBody.corePosition, Quaternion.identity);
            StickyOverloaderController bomba = goop.GetComponent<StickyOverloaderController>();
            bomba.TrySticking(victimBody, attackerBody);
            NetworkServer.Spawn(goop);  
        }

        public Transform indicator;
        public Animator animator;
        public GameObject explosionEffectPrefab;
        public GameObject impactEffectPrefab;
        public Rigidbody rigidbody;
        

        private int buffCount;

        private bool hasExploded;
        private float blastRadius;
        private float oldRadius;
        private AnimationCurve sizeCurve;
        private float scaleStopwatch;
        private float scaleDuration = 1f;
        private float detonationTimer = 1f;

        [SyncVar]
        private GameObject victim;
        [SyncVar]
        private Quaternion localRotation;
        [SyncVar]
        private Vector3 localPosition;
        [SyncVar]
        private sbyte stuckHurtboxIndex = -1;
        private Transform stuckTransform;
        [SyncVar]
        private int itemStacks;
        private CharacterBody stuckBody
        {
            get
            {
                if(!_stuckBody && victim)
                {
                    _stuckBody = victim.GetComponent<CharacterBody>();
                   
                }
                return _stuckBody;
            }
            set => _stuckBody = value;
        }
        private CharacterBody _stuckBody;
        private CharacterBody attackerBody;
        public void TrySticking(CharacterBody victimBody, CharacterBody attackerBody)
        {
            if (!NetworkServer.active) return;

            // origin of raycast is a hemisphere above body coreposition
            // raycast to body coreposition
            this.attackerBody = attackerBody;
            this.itemStacks = attackerBody.inventory ? attackerBody.inventory.GetItemCount(SS2Content.Items.StickyOverloader) : 1; 
            Vector3 origin = UnityEngine.Random.onUnitSphere * victimBody.radius * 2f;
            origin.y = Mathf.Abs(origin.y);
            origin += victimBody.corePosition;
            var hits = Physics.RaycastAll(origin, victimBody.corePosition - origin, Mathf.Infinity, LayerIndex.entityPrecise.mask);
            for(int i = 0; i < hits.Length; i++)
            {
                HurtBox hurtbox = hits[i].collider.GetComponent<HurtBox>();
                if(hurtbox && hurtbox.healthComponent.body == victimBody)
                {
                    stuckHurtboxIndex = (sbyte)hurtbox.indexInGroup;
                    localRotation = Quaternion.LookRotation(hurtbox.transform.position - hits[i].point);
                    localPosition = hurtbox.transform.InverseTransformPoint(hits[i].point);
                    victim = victimBody.gameObject;
                    stuckBody = victimBody;
                    return;
                }
            }
            SS2Log.Info("StickyOverloaderController.TrySticking: Raycast failed");
            stuckHurtboxIndex = 0;
            localRotation = Quaternion.LookRotation(origin - victimBody.corePosition);
            localPosition = Vector3.zero;
            victim = victimBody.gameObject;
            stuckBody = victimBody;
        }
        private void Start()
        {
            sizeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            buffCount = stuckBody.GetBuffCount(SS2Content.Buffs.BuffStickyOverloader);
            rigidbody = base.GetComponent<Rigidbody>();
            SetBuffCount(buffCount);
            //victimsToInstances.Add(stuckBody, this); // im so fuckig stupdi man
            indicator.transform.SetParent(null); // :/
        }
        private void FixedUpdate()
        {
            if(stuckBody)
            {
                int count = stuckBody.GetBuffCount(SS2Content.Buffs.BuffStickyOverloader);
                if (buffCount != count)
                {
                    SetBuffCount(count);
                }
            }     
            if (!NetworkServer.active) return;
            detonationTimer -= Time.fixedDeltaTime;
            if(detonationTimer <= 0f)
            {
                FireBlast();
            }
        }

        private void Update()
        {
            scaleStopwatch += Time.deltaTime;
            
            if(!stuckTransform && stuckBody)
            {
                stuckTransform = stuckBody.hurtBoxGroup.hurtBoxes[stuckHurtboxIndex] ? stuckBody.hurtBoxGroup.hurtBoxes[stuckHurtboxIndex].transform : null;
            }
            bool hasStuckTransform = stuckTransform;
            if(hasStuckTransform)
            {
                base.transform.SetPositionAndRotation(this.stuckTransform.TransformPoint(this.localPosition), this.stuckTransform.rotation * this.localRotation);               
            }
            if (this.rigidbody.isKinematic != hasStuckTransform)
            {
                if (hasStuckTransform)
                {
                    this.rigidbody.detectCollisions = false;
                    this.rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    this.rigidbody.isKinematic = true;
                }
                else
                {
                    this.rigidbody.detectCollisions = true;
                    this.rigidbody.isKinematic = false;
                    this.rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
            }

            if (indicator)
            {
                float t = Mathf.Clamp01(scaleStopwatch / scaleDuration);
                float scale = (sizeCurve.Evaluate(t) * (blastRadius - oldRadius)) + oldRadius;
                indicator.localScale = Vector3.one * scale;
                indicator.position = base.transform.position;
            }
        }
        private void SetBuffCount(int count)
        {
            if(count > buffCount)
            {
                detonationTimer = 1f;
            }
            buffCount = count;
            scaleStopwatch = 0;
            if(indicator)
            {
                oldRadius = blastRadius;
                blastRadius = count * StickyOverloader.blastRadius;// (StickyOverloader.blastRadius + StickyOverloader.blastRadiusPerStack * (itemStacks - 1));
            }
            if(animator)
            {
                animator.SetFloat("intensity", count / (float)(StickyOverloader.maxStacks + StickyOverloader.maxStacksPerStack * (itemStacks - 1)));
            }
        }

        private void FireBlast()
        {
            if(!hasExploded)
            {
                hasExploded = true;          
                EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
                {
                    origin = base.transform.position,
                    rotation = base.transform.rotation,
                    scale = blastRadius,
                }, true);
                new BlastAttack
                {
                    attacker = attackerBody.gameObject,
                    inflictor = attackerBody.gameObject,
                    attackerFiltering = AttackerFiltering.Default,
                    position = base.transform.position,
                    teamIndex = attackerBody.teamComponent.teamIndex,
                    radius = blastRadius,
                    baseDamage = attackerBody.damage * (StickyOverloader.damageCoefficient + StickyOverloader.damageCoefficientPerStack * (itemStacks - 1)) * buffCount,
                    damageType = DamageType.Generic,
                    crit = attackerBody.RollCrit(),
                    procCoefficient = 1f,
                    procChainMask = default(ProcChainMask),
                    baseForce = 600f,
                    damageColorIndex = DamageColorIndex.Item,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    losType = BlastAttack.LoSType.NearestHit,
                    impactEffect = EffectCatalog.FindEffectIndexFromPrefab(impactEffectPrefab),
                }.Fire();

                stuckBody.SetBuffCount(SS2Content.Buffs.BuffStickyOverloader.buffIndex, 0);
            }
            
            Destroy(base.gameObject);
            if (indicator) Destroy(indicator.gameObject);
        }
    }
}
