using RoR2;
using RoR2.Items;
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

        [ConfigurableField(ConfigDesc = "Lifetime of the security drone, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float lifeTime = 15f;

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
                        var droneSummon = new MasterSummon();
                        droneSummon.position = victim.corePosition;
                        droneSummon.masterPrefab = masterPrefab;
                        droneSummon.summonerBodyObject = body.gameObject;
                        var droneMaster = droneSummon.Perform();
                        if (droneMaster)
                        {
                            droneMaster.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = lifeTime;

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
