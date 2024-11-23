using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SS2;
using SS2.Components;
using UnityEngine.Networking;
using R2API;

namespace EntityStates.DUT
{
    public class ChargeDamage : BaseSkillState
    {
        public static float baseDuration = 0.8f;

        public static float chargeRadius = 13f;
        public static float chargeDmgCoefficient = 0.8f;
        public static float chargeProcCoef = 0.2f;

        public static float selfHarmCoef = 0.8f;

        public static float minDischargeDmg = 0.8f;
        public static float maxDischargeDmg = 135.75f; //le railgunner
        public static float baseRecoil = 1f;
        public static string muzzleName = "Muzzle";
        private float timer;
        private DUTController controller;

        public override void OnEnter()
        {
            base.OnEnter();
            controller = characterBody.GetComponent<DUTController>();
            if (controller == null)
            {
                SS2Log.Error("Failed to find DU-T controller on body " + characterBody);
                outer.SetNextStateToMain();
            }
            Charge();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                if (!IsKeyDownAuthority() && fixedAge >= baseDuration)
                {
                    Discharge();
                    outer.SetNextStateToMain();
                    return;
                }
            }

            timer += Time.fixedDeltaTime;
            if (timer >= baseDuration)
            {
                timer = 0f;
                Charge();
            }
        }

        public void Charge()
        {
            //AHHHHHHHHHHHHHHHHHH
            switch(controller.currentChargeType)
            {
                case DUTController.ChargeType.Damage:
                {
                    SiphonEnemies();
                    break;
                }
                case DUTController.ChargeType.Healing:
                {
                    SiphonSelf();
                    break;
                }
                default:
                    break;
            }    
        }

        public void SiphonEnemies()
        {
            bool crit = RollCrit();
            BlastAttack blast = new BlastAttack()
            {
                radius = chargeRadius,
                procCoefficient = chargeProcCoef,
                position = characterBody.corePosition,
                attacker = gameObject,
                teamIndex = teamComponent.teamIndex,
                crit = crit,
                baseDamage = characterBody.damage * chargeDmgCoefficient,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = DamageType.Generic,
            };
            DamageAPI.AddModdedDamageType(blast, SS2.Survivors.DUT.DUTDamageType);
            blast.Fire();
        }

        public void SiphonSelf()
        {
            if (NetworkServer.active && healthComponent)
            {
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.damage = characterBody.baseDamage * selfHarmCoef;
                damageInfo.position = characterBody.corePosition;
                damageInfo.force = Vector3.zero;
                damageInfo.damageColorIndex = DamageColorIndex.Default;
                damageInfo.crit = false;
                damageInfo.attacker = null;
                damageInfo.inflictor = null;
                damageInfo.damageType = DamageType.NonLethal;
                damageInfo.procCoefficient = 0f;
                damageInfo.procChainMask = default(ProcChainMask);
                healthComponent.TakeDamage(damageInfo);

                controller.AddCharge(3f);
            }
        }

        public void Discharge()
        {
            float dmgCoefficient = Util.Remap(controller.charge, 0f, controller.chargeMax, minDischargeDmg, maxDischargeDmg);
            Ray r = GetAimRay();

            BulletAttack bullet = new BulletAttack
            {
                aimVector = r.direction,
                origin = r.origin,
                damage = dmgCoefficient * damageStat,
                damageType = DamageType.Generic,
                damageColorIndex = DamageColorIndex.Default,
                minSpread = 0f,
                maxSpread = characterBody.spreadBloomAngle,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                force = 10f, //make this scale later
                isCrit = RollCrit(),
                owner = gameObject,
                muzzleName = muzzleName,
                smartCollision = true,
                procChainMask = default(ProcChainMask),
                procCoefficient = 1f, //make this scale too LOL
                radius = 1f, //and this...
                weapon = gameObject
                //tracerEffectPrefab = tracerPrefab,
                //hitEffectPrefab = hitPrefab
            };
            bullet.Fire();

            //also needs to scale...
            AddRecoil(-0.4f * baseRecoil, -0.8f * baseRecoil, -0.3f * baseRecoil, 0.3f * baseRecoil);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (controller != null)
                controller.AddCharge(-controller.charge);
        }
    }
}
