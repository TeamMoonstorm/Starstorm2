using System;
using JetBrains.Annotations;
using RoR2;
using SS2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace EntityStates.NemExecutioner
{
    public class SummonGhouls : BaseSkillState, MasterSummon.IInventorySetupCallback
    {
        public static GameObject masterPrefab;
        private static int summonLifetime = 24;
        private static float baseSummonInterval = 0.2f;
        private static float summonRadius = 8f;
        private static string summonTransformString = "Summon";
        private static float walkSpeedCoefficient = .5f;

        private static float up = 5f;
        private static float down = 10f;

        private float summonInterval;
        private float stopwatch;
        private Transform summonTransform;

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            summonInterval = baseSummonInterval / attackSpeedStat;

            Transform child = FindModelChild(summonTransformString);
            summonTransform = child ? child : characterBody.coreTransform;

            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;
        }

        public override void OnExit()
        {
            base.OnExit();

            characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.SetAimTimer(2f);
            characterBody.isSprinting = false;

            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0)
            {
                stopwatch += summonInterval;
                activatorSkillSlot.DeductStock(1);
                SummonGhoul();
            }

            if (activatorSkillSlot.stock <= 0)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public void SetupSummonedInventory([NotNull] MasterSummon masterSummon, [NotNull] Inventory summonedInventory)
        {
            summonedInventory.GiveItem(SS2Content.Items.HealthDecayWithRegen, summonLifetime);
        }

        private void SummonGhoul()
        {
            if (NetworkServer.active)
            {
                Ray aimRay = GetAimRay();
                Vector3 direction = aimRay.direction;
                direction.y = 0;
                aimRay.direction = direction.normalized;
                Vector3 origin = aimRay.GetPoint(summonRadius);
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * summonRadius;
                // random circle point + summon transform position
                Vector3 randomPosition = new Vector3(randomCircle.x + origin.x, origin.y, randomCircle.y + origin.z);

                Vector3 hitPosition = randomPosition;
                if (Physics.Raycast(randomPosition + Vector3.up * up, Vector3.down, out RaycastHit hit, down, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    hitPosition = hit.point;
                }

                CharacterMaster characterMaster = new MasterSummon
                {
                    masterPrefab = masterPrefab,
                    position = hitPosition,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    summonerBodyObject = gameObject,
                    ignoreTeamMemberLimit = true,
                    inventorySetupCallback = this,
                }.Perform();

                // TODO: GHOUL POOL!!!!!!!! POOL GHOULS!!!!!!!!!!
                Deployable deployable = characterMaster.gameObject.AddComponent<Deployable>();
                deployable.onUndeploy = new UnityEvent();
                deployable.onUndeploy.AddListener(new UnityAction(characterMaster.TrueKill));
                characterBody.master.AddDeployable(deployable, SS2.Survivors.NemExecutioner.Ghoul);
            }
            
        }
    }
}
