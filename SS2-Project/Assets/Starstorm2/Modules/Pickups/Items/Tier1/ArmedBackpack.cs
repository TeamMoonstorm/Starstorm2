using RoR2;
using RoR2.Items;
using System.Linq;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class ArmedBackpack : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("ArmedBackpack");

        [ConfigurableField(ConfigDesc = "Damage dealt by the backpack. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_ARMEDBACKPACK_DESC", StatTypes.Percentage, 0)]
        public static float backpackDamage = 1f;

        [ConfigurableField(ConfigDesc = "Proc chance of the backpack. (100 = 100%)")]
        [TokenModifier("SS2_ITEM_ARMEDBACKPACK_DESC", StatTypes.Percentage, 0)]
        public static float procChance = 15f;

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ArmedBackpack;
            public void OnDamageDealtServer(DamageReport report)
            {
                if (Util.CheckRoll(procChance, body.master))
                {
                    if (report.damageInfo.procCoefficient > 0)
                    {

                        BullseyeSearch bullseyeSearch = new BullseyeSearch();
                        //EffectManager.SimpleMuzzleFlash();
                        TeamComponent team = GetComponent<TeamComponent>();
                        bullseyeSearch.teamMaskFilter = TeamMask.all;
                        bullseyeSearch.teamMaskFilter.RemoveTeam(team.teamIndex);
                        bullseyeSearch.filterByLoS = true;
                        bullseyeSearch.searchOrigin = body.aimOrigin;
                        bullseyeSearch.searchDirection = -body.characterDirection.targetVector;
                        bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
                        bullseyeSearch.maxDistanceFilter = 1000;
                        bullseyeSearch.maxAngleFilter = 45f;
                        bullseyeSearch.RefreshCandidates();
                        HurtBox target = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();

                        if (!target)
                            return;

                        MSUtil.PlayNetworkedSFX("ExecutionerPrimary", gameObject.transform.position);
                        new BulletAttack
                        {
                            owner = gameObject,
                            weapon = gameObject,
                            origin = body.aimOrigin,
                            aimVector = target.transform.position - body.aimOrigin,
                            minSpread = 0,
                            maxSpread = 0,
                            damage = backpackDamage * body.damage,
                            force = 50,
                            isCrit = Util.CheckRoll(body.crit),
                            damageType = DamageType.Generic
                        }.Fire();

                    }
                }
            }
        }
    }
}
