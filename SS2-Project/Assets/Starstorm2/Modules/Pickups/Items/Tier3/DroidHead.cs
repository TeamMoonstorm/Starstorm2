using RoR2;
using RoR2.Items;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class DroidHead : ItemBase
    {
        private const string token = "SS2_ITEM_DROIDHEAD_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DroidHead", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Damage dealt by Security Drones, at base and per stack. Percentage (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float baseDamage = 1f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Base life time of the Security Drone, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float baseLifeTime = 20f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Life time of the Security Drone per stack, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float stackLifeTime = 10f;

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DroidHead;

            GameObject masterPrefab = SS2Assets.LoadAsset<GameObject>("DroidDroneMaster", SS2Bundle.Items);
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
                            droneSummon.masterPrefab = masterPrefab;
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
