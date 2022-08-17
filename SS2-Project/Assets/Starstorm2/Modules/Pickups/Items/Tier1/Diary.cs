using RoR2;
using RoR2.Items;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class Diary : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Diary");

        public sealed class Behavior : BaseItemBodyBehavior, IStatItemBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Diary;
            private float stopwatch;

            public void FixedUpdate()
            {
                if (!NetworkServer.active)
                    return;
                if (Run.instance && !Run.instance.isRunStopwatchPaused)
                {
                    if (body.master.playerCharacterMasterController)
                    {
                        stopwatch += Time.fixedDeltaTime;
                        if (stopwatch >= 2f)
                        {
                            if (body.teamComponent)
                            {
                                uint exp = (uint)(stack * Math.Pow(2, 1 + (TeamManager.instance.GetTeamLevel(body.teamComponent.teamIndex) / 3.75d)));
                                TeamManager.instance.GiveTeamExperience(TeamIndex.Player, exp);
                            }

                            stopwatch = 0f;
                        }
                    }
                }
            }

            public void RecalculateStatsStart()
            {
                if (NetworkServer.active)
                {
                    float level = TeamManager.instance.GetTeamLevel(body.teamComponent.teamIndex);
                    if (level > body.level && body.hasEffectiveAuthority)
                    {
                        //N - Kevin works in misterious ways i guess, idk man
                        if (Util.CheckRoll(50)) //G - What the fuck is this doing in recalculatestats
                        { //★ this is fucking stupid yeah
                            MSUtil.PlayNetworkedSFX("DiaryLevelUp", body.corePosition);
                        }
                    }
                }
            }

            public void RecalculateStatsEnd()
            { }
        }

    }
}
