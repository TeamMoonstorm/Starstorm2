using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine.Networking;

namespace EntityStates.Chirr
{
    public class Befriend : BaseSkillState
    {
        public static float baseDuration;
        public static float baseFireDelay;
        public static float range;
        public static BuffDef removeBuff;

        private float duration;
        private float fireDelay;
        private bool hasMadeFriend = false;
        private HurtBox futureFriend;
        private ChirrNetworkInfo chirrInfo;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireDelay = baseFireDelay / duration;
            characterBody.SetAimTimer(2f);
            chirrInfo = characterBody.GetComponent<ChirrNetworkInfo>();

            PlayAnimation("Gesture, Override", "Special", "Special.playbackRate", duration);
            PlayAnimation("Gesture, Additive", "Special", "Special.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireDelay && !hasMadeFriend)
            {
                MakeFriend();
                hasMadeFriend = true;
            }
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void MakeFriend()
        {
            if (!chirrInfo.friend)
            {
                futureFriend = chirrInfo.futureFriend;
                CharacterBody newFriend;

                if (futureFriend && NetworkServer.active)
                {
                    newFriend = futureFriend.healthComponent.body;
                    chirrInfo.friendOrigIndex = newFriend.teamComponent.teamIndex;
                    newFriend.teamComponent.teamIndex = teamComponent.teamIndex;
                    newFriend.master.teamIndex = teamComponent.teamIndex;
                    BaseAI friendAI = newFriend.master.GetComponent<BaseAI>();
                    if (friendAI)
                    {
                        friendAI.currentEnemy.Reset();
                        friendAI.leader.gameObject = gameObject;

                        newFriend.healthComponent.Heal(newFriend.healthComponent.fullHealth, default(ProcChainMask));
                        newFriend.healthComponent.RechargeShieldFull();
                        newFriend.master.inventory.GetComponent<MinionOwnership>().SetOwner(characterBody.master);
                        chirrInfo.baseInventory = new Inventory();
                        chirrInfo.baseInventory.CopyItemsFrom(newFriend.master.inventory);
                        newFriend.master.inventory.AddItemsFrom(characterBody.inventory);
                        newFriend.master.inventory.ResetItem(RoR2Content.Items.WardOnLevel.itemIndex);
                        newFriend.master.inventory.ResetItem(RoR2Content.Items.BeetleGland.itemIndex);
                        newFriend.master.inventory.ResetItem(RoR2Content.Items.CrippleWardOnLevel.itemIndex);
                        newFriend.master.inventory.ResetItem(RoR2Content.Items.TPHealingNova.itemIndex);
                        newFriend.master.inventory.ResetItem(RoR2Content.Items.FocusConvergence.itemIndex);
                        newFriend.master.inventory.ResetItem(RoR2Content.Items.TitanGoldDuringTP.itemIndex);
                        newFriend.master.inventory.ResetItem(RoR2Content.Items.ExtraLife.itemIndex);
                        newFriend.SetBuffCount(removeBuff.buffIndex, 0);

                        friendAI.UpdateTargets();
                        chirrInfo.friend = newFriend;
                    }
                }
                else if (futureFriend && isAuthority)
                {
                    newFriend = futureFriend.healthComponent.body;
                    if (newFriend.master.GetComponent<BaseAI>())
                    {
                        chirrInfo.friend = newFriend;
                    }
                }
                else
                {
                    skillLocator.special.AddOneStock();
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
