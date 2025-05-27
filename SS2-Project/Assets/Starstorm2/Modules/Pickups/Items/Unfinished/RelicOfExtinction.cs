using R2API;
using RoR2;
using RoR2.Items;

using MSU;
using System.Collections.Generic;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using UnityEngine.Networking;

namespace SS2.Items
{
    public sealed class RelicOfExtinction : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acRelicOfExtinction", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of health increase. (1 = 100%)")]
        [FormatToken("SS2_ITEM_RELICOFEXTINCTION_DESC", 0)]
        public static float baseRange = 12f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of which acceleration is divided by.")]
        [FormatToken("SS2_ITEM_RELICOFEXTINCTION_DESC", 1)]
        public static float scalingRange = 8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of which acceleration is divided by.")]
        [FormatToken("SS2_ITEM_RELICOFEXTINCTION_DESC", 1)]
        public static float orbMovementSpeed = .015f;

        public static Vector3 teleporterPosition;

        //public static ItemAssetCollection AssetCollectionReference;
        public static GameObject extinctionReference;
        public override void Initialize()
        {
            //AssetCollectionReference = AssetCollection;
            extinctionReference = AssetCollection.FindAsset<GameObject>("ExtinctionOrb");
            //On.RoR2.SceneDirector.PlaceTeleporter += PlaceTeleporterExtinctionGrabPosition;
            SceneDirector.onPostPopulateSceneServer += PostPopulateSceneGrabExtinctionPosition;
        }

        private void PostPopulateSceneGrabExtinctionPosition(SceneDirector self)
        {
            if (self.teleporterSpawnCard && self.teleporterInstance)
            {
                teleporterPosition = self.teleporterInstance.transform.position;
            }
            else
            {
                teleporterPosition = Vector3.zero;
            }
        }

        //private void PlaceTeleporterExtinctionGrabPosition(On.RoR2.SceneDirector.orig_PlaceTeleporter orig, SceneDirector self)
        //{
        //    orig(self);
        //    if (self.teleporterSpawnCard && self.teleporterInstance)
        //    {
        //        teleporterPosition = self.teleporterInstance.transform.position;
        //    }
        //    else
        //    {
        //        teleporterPosition = Vector3.zero;
        //    }
        //}

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        //N: Maybe this can be reduced to just a RecalcStatsAPI call? that'd be ideal.
        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfExtinction;


            public GameObject extinctionObject;

            public void OnEnable()
            {
                extinctionActive = true;
            }

            public void OnDisable()
            {
                extinctionActive = false;
            }

            public int RandomizeParity()
            {
                var val = UnityEngine.Random.Range(0, 1);

                if (val == 0)
                {
                    return -1;
                }
                return 1;


            }


            private bool extinctionActive
            {
                get
                {
                    return extinctionObject != null;
                }
                set
                {
                    if (extinctionActive == value)
                    {
                        return;
                    }

                    if (value)
                    {
                        //AssetCollection.FindAsset<Material>("matTrimSheetMetalBlue"); //LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator");
                        if (teleporterPosition == Vector3.zero)
                        {
                            teleporterPosition.x = body.corePosition.x + (75 * RandomizeParity());
                            teleporterPosition.y = body.corePosition.y + (75 * RandomizeParity());
                            teleporterPosition.z = body.corePosition.z + (75 * RandomizeParity());
                        }
                        extinctionObject = UnityEngine.Object.Instantiate<GameObject>(extinctionReference, teleporterPosition, Quaternion.identity);
                        //extinctionObject.transform.localScale = extinctionObject.transform.localScale / 5f;
                        var eoh = extinctionObject.gameObject.AddComponent<ExtinctionOrbHandler>();
                        eoh.target = body;
                        eoh.SetStacks();
                        eoh.BeginScale(true);
                        NetworkServer.Spawn(extinctionObject);
                        //extinctionObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject, null);
                        return;
                    }

                    if (extinctionObject)
                    {
                        var eoh2 = extinctionObject.gameObject.GetComponent<ExtinctionOrbHandler>();
                        eoh2.shouldExist = false;
                        eoh2.BeginScale(false);
                    }
                    else
                    {
                        extinctionObject = null;
                    }

                }
            }
        }
    }

    public class ExtinctionOrbHandler : MonoBehaviour
    {
        public CharacterBody target;
        public Rigidbody rigid;
        public bool shouldExist = true;

        float timer = 0;
        float size = 1;
        float sizeMax = 12;
        float scalar = .005f;

        int count = 1;
        private bool alternate = false;

        public void Start()
        {
            rigid = this.GetComponent<Rigidbody>();
        }


        public void SetStacks()
        {
            count = target.GetItemCount(SS2Content.Items.RelicOfExtinction);
            sizeMax = RelicOfExtinction.baseRange + ((count - 1) * RelicOfExtinction.scalingRange);
        }

        public void FixedUpdate()
        {
            if (target)
            {
                //var currentPos = this.transform.position;

                var maxSpeed = target.moveSpeed;
                if (target.isSprinting)
                {
                    maxSpeed /= target.sprintingSpeedMultiplier;
                }

                //maxSpeed += 10f; //it moving at exactly player speed is way too slow

                var magnitude = (target.corePosition - transform.position).magnitude * maxSpeed / 16;
                var dir = CalculatePullDirection();
                var magnitudeAdjusted = Mathf.Max(Mathf.Min(magnitude, maxSpeed * 2), 5);

                var idealVelocity = dir * magnitudeAdjusted;

                if (rigid)
                {
                    var inaccuracy = idealVelocity - rigid.velocity;
                    rigid.velocity += inaccuracy / 2 * Time.deltaTime;
                    if (rigid.velocity.magnitude > maxSpeed * 2)
                    {
                        rigid.velocity = rigid.velocity.normalized * maxSpeed;
                    }
                }
                else
                {
                    rigid = this.GetComponent<Rigidbody>();
                    SS2Log.Error("Finding rigid in fixedupdate :(");
                }


                //magnitude -= (sizeMax / 2f);
                //transform.position = Vector3.MoveTowards(transform.position, target.corePosition, .01f * magnitude);

                timer += Time.fixedDeltaTime;

                if (timer > .1f)
                {
                    timer = 0;

                    SS2Log.Error("Position : " + rigid.velocity.x + ", " + rigid.velocity.y + ", " + rigid.velocity.z + " ||| " + magnitudeAdjusted + " : " + maxSpeed * 2);

                    if (alternate)
                    {
                        new BlastAttack
                        {
                            radius = (size * (10f / 12f)) * 2,
                            baseDamage = 5,
                            procCoefficient = 0,
                            crit = false,
                            damageColorIndex = DamageColorIndex.Item,
                            attackerFiltering = AttackerFiltering.Default,
                            falloffModel = BlastAttack.FalloffModel.None,
                            attacker = target.gameObject,
                            teamIndex = target.teamComponent.teamIndex,
                            position = transform.position,
                            //baseForce = 0,
                            damageType = DamageType.AOE

                        }.Fire();
                        alternate = false;
                    }
                    else
                    {
                        alternate = true;
                    }

                    new BlastAttack
                    {
                        radius = size * (10f / 12f),
                        baseDamage = 10,
                        procCoefficient = 0,
                        crit = false,
                        damageColorIndex = DamageColorIndex.Item,
                        attackerFiltering = AttackerFiltering.AlwaysHit,
                        falloffModel = BlastAttack.FalloffModel.None,
                        attacker = target.gameObject,
                        teamIndex = target.teamComponent.teamIndex,
                        position = transform.position,
                        //baseForce = 0,
                        damageType = DamageType.AOE

                    }.Fire();

                    new BlastAttack
                    {
                        radius = (size * (10f / 12f)) / 2,
                        baseDamage = 100,
                        procCoefficient = 0,
                        crit = false,
                        damageColorIndex = DamageColorIndex.Item,
                        attackerFiltering = AttackerFiltering.AlwaysHit,
                        falloffModel = BlastAttack.FalloffModel.None,
                        attacker = target.gameObject,
                        teamIndex = target.teamComponent.teamIndex,
                        position = transform.position,
                        //baseForce = 0,
                        damageType = DamageType.AOE

                    }.Fire();

                    var newCount = target.GetItemCount(SS2Content.Items.RelicOfExtinction);
                    if (count != newCount)
                    {
                        SS2Log.Warning("UPdating size " + count + " | " + newCount);
                        BeginScale(newCount > count);
                        count = newCount;

                    }
                }
            }
        }

        private Vector3 CalculatePullDirection()
        {
            if (target)
            {
                return (target.corePosition - transform.position).normalized;
            }
            return base.transform.forward;
        }

        public void BeginScale(bool increase)
        {
            StartCoroutine(ScaleSize(increase));
        }

        public IEnumerator ScaleSize(bool increase)
        {
            if (increase)
            {
                while (size < sizeMax && shouldExist)
                {
                    yield return .1f;

                    size += .025f;

                    if (size > sizeMax)
                    {
                        size = sizeMax;
                    }
                    transform.localScale = new Vector3(size / 12f, size / 12f, size / 12f);
                }
            }
            else
            {
                float change = .05f;
                if (shouldExist)
                {
                    while (size > sizeMax)
                    {
                        yield return .1f;

                        size -= change;
                        transform.localScale = new Vector3(size / 12f, size / 12f, size / 12f);
                    }
                }
                else
                {
                    while (size > 0)
                    {
                        yield return .1f;

                        size -= change;
                        change += scalar;
                        transform.localScale = new Vector3(size / 12f, size / 12f, size / 12f);
                    }
                    Destroy(this.gameObject);
                }
            }
        }
    }
}



//N: Nebby here, just stating that the code related to Relic of Extinction should be treated as a cancerous tumour, it was coded by anreol, and just like anything he does for stuff outside his own projects, never finished it.
//I'll get to finish it eventually with a full rewrite, he can kiss my ass.
/*public sealed class RelicOfExtinction : SS2Item
{
    public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfExtinction", SS2Bundle.Indev);

    public sealed class Behavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation]
        private static ItemDef GetItemDef() => SS2Content.Items.RelicOfExtinction;

        private GameObject prolapsedInstance;
        public bool shouldFollow = true;
        public void Start()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (!prolapsedInstance)
            {
                prolapsedInstance = UnityEngine.Object.Instantiate(SS2Assets.LoadAsset<GameObject>("ExtinctionHole", SS2Bundle.Indev), body.corePosition, Quaternion.identity);
                prolapsedInstance.GetComponent<GenericOwnership>().ownerObject = body.gameObject;
                prolapsedInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
            }
        }

        //Should kill itself once it loses the owner
        /*public void OnDestroy()
        {
            if (prolapsed)
            {
                UnityEngine.Object.Destroy(prolapsed);
            }
        }
    }
}*/

