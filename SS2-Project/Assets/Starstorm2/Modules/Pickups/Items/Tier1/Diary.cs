using R2API.Networking.Interfaces;
using R2API.Networking;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]

    public sealed class Diary : ItemBase
    {
        private const string token = "SS2_ITEM_DIARY_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Diary", SS2Bundle.Items);

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Experience bonus from each diary. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float experienceBonus = 1;
        public override void Initialize()
        {
            base.Initialize();
            NetworkingAPI.RegisterMessageType<Behavior.DiarySFXMessage>();
        }
        public sealed class Behavior : BaseItemMasterBehavior
        {
            //plays the diary sfx only if players can see the experience bar being affected
            public static void PlayDiarySFXLocal(GameObject holderBodyobject)
            {
                if (!holderBodyobject)
                {
                    return;
                }
                List<UICamera> uICameras = UICamera.instancesList;
                for(int i = 0; i < uICameras.Count; i++)
                {
                    UICamera uICamera = uICameras[i];
                    CameraRigController cameraRigController = uICamera.cameraRigController;
                    if(cameraRigController && cameraRigController.viewer && cameraRigController.viewer.hasAuthority && cameraRigController.target == holderBodyobject)
                    {
                        Util.PlaySound("DiaryLevelUp", holderBodyobject);
                        return;
                    }
                }
            }
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            private static ItemDef GetItemDef() => SS2Content.Items.Diary;
            private bool expectBossKill = false;
            private float localTime;
            private List<TimedExpOffset> pendingOffsets = new List<TimedExpOffset>();
            private float nextAward;
            private void AddPendingOffset(float awardTime, long awardAmount)
            {
                this.pendingOffsets.Add(new TimedExpOffset
                {
                    awardTime = awardTime,
                    offset = awardAmount
                });
                if (this.nextAward > awardTime)
                {
                    this.nextAward = awardTime;
                }
            }
            private void FixedUpdate()
            {
                this.localTime += Time.fixedDeltaTime;
                if (this.pendingOffsets.Count > 0 && this.nextAward <= this.localTime)
                {
                    this.nextAward = float.PositiveInfinity;
                    for (int i = this.pendingOffsets.Count - 1; i >= 0; i--)
                    {
                        TimedExpOffset timedExpOffset = pendingOffsets[i];
                        if (timedExpOffset.awardTime <= this.localTime)
                        {
                            master.SS2OffsetExperience(timedExpOffset.offset, true);
                            this.pendingOffsets.RemoveAt(i);
                            if (master.hasBody)
                            {
                                GameObject bodyObject = master.GetBodyObject();
                                PlayDiarySFXLocal(bodyObject);
                                NetworkIdentity networkIdentity = bodyObject.GetComponent<NetworkIdentity>();
                                if (networkIdentity)
                                {
                                    //we need to give clients an opportunity to play the diary sfx
                                    new DiarySFXMessage(networkIdentity.netId).Send(NetworkDestination.Clients);
                                }

                            }
                        }
                        else if (timedExpOffset.awardTime < this.nextAward)
                        {
                            this.nextAward = timedExpOffset.awardTime;
                        }
                    }
                }
            }
            private void OnEnable()
            {
                On.RoR2.ExperienceManager.AwardExperience += ExperienceManager_AwardExperience;
                On.RoR2.DeathRewards.OnKilledServer += DeathRewards_OnKilledServer;
            }

            private void ExperienceManager_AwardExperience(On.RoR2.ExperienceManager.orig_AwardExperience orig, ExperienceManager self, Vector3 origin, CharacterBody body, ulong amount)
            {
                orig(self, origin, body, amount);
                if (expectBossKill)
                {
                    List<ulong> list = self.CalculateDenominations(amount);
                    uint num = 0U;
                    for (int i = 0; i < list.Count; i++)
                    {
                        this.AddPendingOffset(this.localTime + 0.5f + 1.5f * ExperienceManager.orbTimeOffsetSequence[(int)num], (long)Mathf.CeilToInt(list[i] * experienceBonus * stack));
                        num += 1U;
                        if ((ulong)num >= (ulong)((long)ExperienceManager.orbTimeOffsetSequence.Length))
                        {
                            num = 0U;
                        }
                    }
                    
                }
            }

            private void DeathRewards_OnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport damageReport)
            {
                expectBossKill = self.characterBody && self.characterBody.isBoss && damageReport.attackerTeamIndex == master.teamIndex;
                orig(self, damageReport);
                expectBossKill = false;
            }

            private void OnDisable()
            {
                On.RoR2.ExperienceManager.AwardExperience -= ExperienceManager_AwardExperience;
                On.RoR2.DeathRewards.OnKilledServer -= DeathRewards_OnKilledServer;
            }
            private struct TimedExpOffset
            {
                public float awardTime;
                public long offset;
            }
            public class DiarySFXMessage : INetMessage
            {
                private NetworkInstanceId BodyObjectID;

                public DiarySFXMessage()
                {
                }

                public DiarySFXMessage(NetworkInstanceId bodyObjectID)
                {
                    BodyObjectID = bodyObjectID;
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(BodyObjectID);
                }

                public void Deserialize(NetworkReader reader)
                {
                    BodyObjectID = reader.ReadNetworkId();
                }

                public void OnReceived()
                {
                    if (NetworkServer.active)
                    {
                        return;
                    }
                    GameObject gameObject = Util.FindNetworkObject(BodyObjectID);
                    if (gameObject)
                    {
                        PlayDiarySFXLocal(gameObject);
                    }
                }
            }
        }
    }
}
/*public sealed class Behavior : BaseItemBodyBehavior, IStatItemBehavior
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
}*/
