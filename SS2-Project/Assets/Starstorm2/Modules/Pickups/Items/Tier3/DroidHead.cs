using RoR2;
using RoR2.Items;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class DroidHead : ItemBase
    {
        private const string token = "SS2_ITEM_DROIDHEAD_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DroidHead");

        [ConfigurableField(ConfigDesc = "Amount of extra damage for base and per stack, Percentage (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float baseDamage = 1f;

        [ConfigurableField(ConfigDesc = "Base lifetime of the security drone, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float baseLifeTime = 20f;

        [ConfigurableField(ConfigDesc = "Lifetime of the security drone per stack, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float stackLifeTime = 5f;

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DroidHead;

            GameObject masterPrefab = SS2Assets.LoadAsset<GameObject>("DroidDroneMaster");
            public void OnKilledOtherServer(DamageReport damageReport)
            {
                var victim = damageReport.victimBody;
                var victimEquipment = victim.inventory.GetEquipmentIndex();
                if (victim.teamComponent.teamIndex != body.teamComponent.teamIndex) //This wasnt necesary before. but now drones from droidhead spawns another drone when they die. just gotta make sure later to do an if(victimbody.gameObject != DroidHeadDroneObject)
                {
                    if (victim.isElite)
                    {
                        //var itemName = victim.equipmentSlot.name;
                        var droneSummon = new MasterSummon();
                        droneSummon.position = victim.corePosition;
                        droneSummon.masterPrefab = masterPrefab;
                        droneSummon.summonerBodyObject = body.gameObject;
                        var droneMaster = droneSummon.Perform(); 
                        if (droneMaster)
                        {
                            droneMaster.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = baseLifeTime + (stackLifeTime * (stack - 1));

                            CharacterBody droidBody = droneMaster.GetBody();
                            droidBody.baseDamage *= (baseDamage * stack);

                            Inventory droidInventory = droneMaster.inventory;
                            //droidInventory.SetEquipmentIndex(victimEquipment);

                            if (!victim.HasBuff(DLC1Content.Buffs.EliteVoid))
                            {
                                droidInventory.SetEquipmentIndex(victimEquipment);
                            }


                            if (Starstorm.RiskyModInstalled)
                            {
                                RiskyModCompat(droidInventory);
                            }

                            Util.PlaySound("DroidHead", body.gameObject);
                        }
                    }
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public void RiskyModCompat(Inventory droidInventory) //done this way because i forget how to mod every day
            {
                var markerItem = ItemCatalog.FindItemIndex("RiskyModAllyMarkerItem");
                var scalingItem = ItemCatalog.FindItemIndex("RiskyModAllyScalingItem");
                var regenItem = ItemCatalog.FindItemIndex("RiskyModAllyRegenItem");
                var voidDeathItem = ItemCatalog.FindItemIndex("RiskyModAllyAllowVoidDeathItem");
                if (markerItem != ItemIndex.None)
                {
                    droidInventory.GiveItem(markerItem);
                }
                if (scalingItem != ItemIndex.None)
                {
                    droidInventory.GiveItem(scalingItem);
                }
                if (regenItem != ItemIndex.None)
                {
                    droidInventory.GiveItem(regenItem, 40);
                }
                if (voidDeathItem != ItemIndex.None)
                {
                    droidInventory.GiveItem(voidDeathItem);
                }
            }
        }

    }
}
