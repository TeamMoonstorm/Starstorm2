using MonoMod.Cil;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2.Navigation;
using RoR2.Projectile;
using static MSU.BaseBuffBehaviour;

namespace SS2.Equipments
{
#if DEBUG
    public sealed class AffixEthereal : SS2EliteEquipment
    {
        //public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("AffixEthereal", SS2Bundle.Equipments);

        //public override List<MSEliteDef> EliteDefs { get; } = new List<MSEliteDef>
        //{
        //    SS2Assets.LoadAsset<MSEliteDef>("edEthereal", SS2Bundle.Equipments),
        //};

        //public override bool FireAction(EquipmentSlot slot)
        //{
        //    return false;
        //}
        public override List<EliteDef> EliteDefs => _eliteDefs;
        private List<EliteDef> _eliteDefs;

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        public  BuffDef _buffEthereal; //TODO MSU 2.0 replace { get; } = SS2Assets.LoadAsset<BuffDef>("bdEthereal", SS2Bundle.Equipments);
        public Material _matEtherealOverlay; //=> SS2Assets.LoadAsset<Material>("matEtherealOverlay", SS2Bundle.Equipments);


        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void Initialize()
        {
            // Add relevant hooks
            IL.RoR2.HealthComponent.TakeDamage += EtherealDeathIL;

            // Add buff overlays
            BuffOverlays.AddBuffOverlay(_buffEthereal, _matEtherealOverlay);
        }

        private void EtherealDeathIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            bool ILFound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(11),
                x => x.MatchCallOrCallvirt<GlobalEventManager>(nameof(GlobalEventManager.ServerDamageDealt)),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<HealthComponent>("get_alive")
            );

            c.Index += 3;

            if (ILFound)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<HealthComponent>>((hc) =>
                {
                    if (hc.health <= 0 && hc.body.HasBuff(SS2Content.Buffs.bdEthereal))
                    {
                        hc.health = 1;
                        if (!hc.body.HasBuff(SS2Content.Buffs.bdHakai))
                        {
                            hc.body.AddBuff(SS2Content.Buffs.bdHakai);
                            hc.body.AddBuff(RoR2Content.Buffs.Intangible);
                        }
                    }
                });
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            // TODO: Load content!
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public sealed class EtherealBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => SS2Content.Buffs.bdEthereal;
            public static GameObject projectilePrefab = SS2Assets.LoadAsset<GameObject>("EtherealCircle", SS2Bundle.Equipments);
            private static System.Random random = new System.Random();
            public static float baseTimerDur = 12.5f;

            private CharacterModel model;
            private GameObject etherealEffect;

            private float timerDur;
            private float timer;

            public void Start()
            {
                timerDur = baseTimerDur; // / body.attackSpeed;   >
                timer = timerDur * 0.75f;

                Util.PlaySound("Play_ui_teleporter_activate", this.gameObject);

                model = CharacterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
                if (model != null)
                {
                    this.etherealEffect = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("EtherealAffixEffect", SS2Bundle.Equipments), model.transform);
                }
            }

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                    return;

                // TODO: Will HasAnyStacks break this fixed update? I hope not!
                if (HasAnyStacks)
                {
                    timer += Time.fixedDeltaTime;
                    if (timer >= timerDur)
                    {
                        timer = 0;
                        if (CharacterBody.isChampion)
                            timer = timerDur / 2f;

                        //you never know.
                        if (projectilePrefab != null)
                        {
                            PlaceCircle();
                        }
                    }
                }
                
            }

            public void OnDestroy()
            {
                // TODO: I wonder if HasAnyStacks here would cause any issues?
                if (HasAnyStacks && etherealEffect)
                    Destroy(this.etherealEffect);
            }


            private void PlaceCircle()
            {
                Debug.Log("Firing projectile: " + projectilePrefab);

                // We dont need a HasAnyStacks check since we check in the FixedUpdate before it calls this method.
                if (CharacterBody != null)
                {
                    if (CharacterBody.healthComponent.alive)
                    {
                        NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                        List<NodeGraph.NodeIndex> nodeList = groundNodes.FindNodesInRange(CharacterBody.transform.position, 0f, 32f, HullMask.Human);
                        NodeGraph.NodeIndex randomNode = nodeList[random.Next(nodeList.Count)];
                        if (randomNode != null)
                        {
                            Vector3 position;
                            groundNodes.GetNodePosition(randomNode, out position);
                            ProjectileManager.instance.FireProjectile(projectilePrefab, position, new Quaternion(0, 0, 0, 0), CharacterBody.gameObject, 1f, 0f, CharacterBody.RollCrit(), DamageColorIndex.Default, null, 0);
                        }
                    }
                }
            }
        }

    }
#endif
}
