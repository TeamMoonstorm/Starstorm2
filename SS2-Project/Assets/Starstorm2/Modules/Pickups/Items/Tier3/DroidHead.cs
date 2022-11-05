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
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
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
                        var itemName = victim.equipmentSlot.name;
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
                            
                            if (!itemName.Contains("Void"))
                            {
                                droidInventory.SetEquipmentIndex(victimEquipment);
                            }
                            else
                            {
                                SS2Log.Debug("void elite");
                            }
                            Util.PlaySound("DroidHead", body.gameObject);
                        }
                    }
                }
            }
        }
      
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        //public void RiskyModCompat(Inventory droidInventory)
        //{
        //    //droidInventory.GiveItem(RiskyMod.Allies.AlliesCore.AllyAllowVoidDeathItem);
        //
        //    //- RiskyMod.Allies.AlliesCore.AllyMarkerItem
        //    //- RiskyMod.Allies.AlliesCore.AllyScalingItem
        //    //- RiskyMod.Allies.AlliesCore.AllyAllowVoidDeathItem(easily - replaced ally, so they get to die to void reaver explosions)
        //    //- RiskyMod.Allies.AlliesCore.AllyRegenItem, 40 stack
        //
        //    //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterSubmission"), "NemmandoBody", SkillSlot.Special, 0);
        //    //AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterBossAttack"), "NemmandoBody", SkillSlot.Special, 1);
        //}

    }
}
