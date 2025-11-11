using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SS2;
using SS2.Components;
using UnityEngine.Networking;
using R2API;
using SS2.Orbs;
using RoR2.Orbs;

namespace EntityStates.DUT
{
    public class ChargeDamage : BaseSkillState
    {
        public static float baseDuration = 0.8f;

        public static float chargeRadius = 13f;
        public static float chargeDmgCoefficient = 0.8f;
        public static float chargeProcCoef = 0.2f;

        public static float selfHarmCoef = 2.4f;

        public static float minDischargeDmg = 0.8f;
        public static float maxDischargeDmg = 135.75f; //le railgunner
        public static float minRadius = 0.2f;
        public static float maxRadius = 3f;
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

                DUTGreenOrb orb = new DUTGreenOrb();
                orb.origin = damageInfo.position;
                orb.target = characterBody.mainHurtBox;

                OrbManager.instance.AddOrb(orb);
                OrbManager.instance.AddOrb(orb);
                OrbManager.instance.AddOrb(orb);
            }
        }

        public void Discharge()
        {
            float dmgCoefficient = Util.Remap(controller.charge, 0f, controller.chargeMax, minDischargeDmg, maxDischargeDmg);
            float radius = Util.Remap(controller.charge, 0f, controller.chargeMax, minRadius, maxRadius);
                
            Ray r = GetAimRay();

            SS2Log.Debug("discharging at " + controller.charge + " with a dmgcoef of " + dmgCoefficient);

            float procCoefficient = 1f;
            if (controller.charge >= controller.chargeMax * 0.5f)
                procCoefficient++;
            if (controller.charge >= controller.chargeMax)
                procCoefficient++;

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
                procCoefficient = procCoefficient,
                radius = radius,
                weapon = gameObject,
                tracerEffectPrefab = controller.tracerPrefab,
                //hitEffectPrefab = hitPrefab
            };

            //pierce if over 33% charge
            if (controller.charge >= controller.chargeMax / 3f)
                bullet.stopperMask = LayerIndex.world.mask;

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
            {
                SS2Log.Debug("DUT Charge: " + controller.charge);
                controller.ResetCharge();
            }
        }
    }
}
