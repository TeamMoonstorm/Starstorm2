using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Skills;
using UnityEngine.AddressableAssets;
namespace SS2.Items
{
    public sealed class DroidHead : SS2Item
    {
        private const string token = "SS2_ITEM_DROIDHEAD_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acDroidHead", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage dealt by Security Drones, at base and per stack. Percentage (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseDamage = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base life time of the Security Drone, in seconds.")]
        [FormatToken(token, 1)]
        public static float baseLifeTime = 20f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Life time of the Security Drone per stack, in seconds.")]
        [FormatToken(token, 2)]
        public static float stackLifeTime = 10f;

        private static GameObject _droidDroneMaster;

        public override void Initialize()
        {
            _droidDroneMaster = AssetCollection.FindAsset<GameObject>("DroidDroneMaster");

            //Assignment of the FireTurret/Drone1 Command
            var/*stinky var*/ skillFamily = SS2Assets.LoadAsset<SkillFamily>("sfDroidDroneCommand", SS2Bundle.Items);
            skillFamily.variants = new SkillFamily.Variant[1];
            var/*stinky var*/ skillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC3/Drone Tech/CommandFireTurretSkillDef.asset").WaitForCompletion();
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            //Future proof in case this code is forgotten and not removed when the DroneDef is added to the AssetCollection after MSU supports it
            if (AssetCollection.FindAsset<DroneDef>("ddDroidDrone") == null)
            {
                DroneDef droneDef = SS2Assets.LoadAsset<DroneDef>("ddDroidDrone", SS2Bundle.Items);
                if (droneDef)
                {
                    contentPack.droneDefs.AddSingle(droneDef);
                }
            }
            base.ModifyContentPack(contentPack);
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DroidHead;

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                var victim = damageReport.victimBody;
                if (victim.inventory)
                {
                    var victimEquipment = victim.inventory.GetEquipmentIndex();
                    if (victim.teamComponent.teamIndex != body.teamComponent.teamIndex)
                    {
                        if (victim.isElite && victimEquipment != DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex)
                        {
                            var droneSummon = new MasterSummon();
                            droneSummon.position = victim.corePosition + (Vector3.up * 3);
                            droneSummon.masterPrefab = _droidDroneMaster;
                            droneSummon.summonerBodyObject = body.gameObject;
                            var droneMaster = droneSummon.Perform();
                            if (droneMaster)
                            {
                                droneMaster.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = baseLifeTime + (stackLifeTime * (stack - 1));

                                CharacterBody droidBody = droneMaster.GetBody();
                                droidBody.baseDamage *= (baseDamage * stack);

                                Inventory droidInventory = droneMaster.inventory;

                                droidInventory.SetEquipmentIndex(victimEquipment);

                                Util.PlaySound("DroidHead", body.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }   
}
