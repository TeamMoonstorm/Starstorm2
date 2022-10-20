/*using RoR2;
using RoR2.Projectile;
using Moonstorm.Starstorm2.Survivors;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Borg
{
    public class BorgTeleport : BaseSkillState
    {
        public float damageCoefficient = 16f;
        public float baseDuration = 0.5f;
        public float recoil = 1f;

        private float duration;
        private float fireDuration;
        private bool hasTpd;
        private Animator animator;
        private string muzzleString;
        private BorgInfoComponent TPInfo;

        public override void OnEnter()
        {
            base.OnEnter();
            TPInfo = GetComponent<BorgInfoComponent>();

            if (!TPInfo.isHooked)
            {
                On.RoR2.Stage.Start += (orig, self) =>
                {
                    orig(self);
                    skillLocator.special.SetBaseSkill(Starstorm2.Survivors.Borg.specialDef1);
                    TPInfo.tpReady = false;
                };
                TPInfo.isHooked = true;
            }

            //TeleportInfoComponent.tpPos;
            duration = baseDuration / attackSpeedStat;
            fireDuration = 0.25f * duration;
            animator = GetModelAnimator();
            if (TPInfo.tpReady)
            {
                PlayAnimation("Gesture, Override", "UseTP", "FireM1.playbackRate", duration);
            }
            else
            {
                PlayAnimation("Gesture, Override", "CreateTP", "FireM1.playbackRate", duration);
                skillLocator.special.SetBaseSkill(Starstorm2.Survivors.Borg.specialDef2);
            }
        }

        private void SetTp()
        {
            //TPInfo.tpPos = base.characterBody.transform.position + new Vector3(0, 1, 0);
            string soundString = "BorgSpecialPlace";//base.effectComponent.shootSound;
            Util.PlaySound(soundString, gameObject);
            //base.skillLocator.special.SetBaseSkill(BorgCore.specialDef2);
            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = RollCrit(),
                    damage = damageStat * damageCoefficient,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 0f,
                    owner = gameObject,
                    position = aimRay.origin,
                    procChainMask = default(ProcChainMask),
                    projectilePrefab = Starstorm2.Survivors.Borg.BorgPylon,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        private void UseTp()
        {
            skillLocator.special.SetBaseSkill(Starstorm2.Survivors.Borg.specialDef1);
            string soundString = "BorgSpecialTeleport";//base.effectComponent.shootSound;
            Util.PlaySound(soundString, gameObject);
            if (characterMotor)
            {
                //base.characterMotor.Motor.SetPositionAndRotation(TPInfo.tpPos, Quaternion.identity, true);
            }
            List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
            int count = instancesList.Count;
            for (int i = 0; i < count; i++)
            {
                if ((instancesList[i].owner == gameObject) && (instancesList[i].name == "Prefabs/Projectiles/BorgTPPylon(Clone)"))
                {
                    characterMotor.Motor.SetPositionAndRotation(instancesList[i].transform.position, Quaternion.identity, true);
                    instancesList[i].GetComponent<ProjectileImpactExplosion>().lifetime = 0;
                }
                //Chat.AddMessage(instancesList[i].name);
            }

            skillLocator.primary.RunRecharge(4);
            skillLocator.secondary.RunRecharge(4);
            skillLocator.utility.RunRecharge(4);
        }
        private void resetTp()
        {
            //UnityEngine.Object.Destroy(bodyInstanceObject);
            List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
            int count = instancesList.Count;
            for (int i = 0; i < count; i++)
            {
                if ((instancesList[i].owner == gameObject) && (instancesList[i].name == "Prefabs/Projectiles/BorgTPPylon(Clone)"))
                {
                    Object.Destroy(instancesList[i].gameObject);
                    skillLocator.special.SetBaseSkill(Starstorm2.Survivors.Borg.specialDef1);
                    TPInfo.tpReady = false;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((fixedAge >= fireDuration) && !hasTpd && !inputBank.skill4.down)
            {
                if (!TPInfo.tpReady)
                    SetTp();
                else
                    UseTp();
                hasTpd = true;
                TPInfo.tpReady = !TPInfo.tpReady;
            }
            if (fixedAge >= 2 && inputBank.skill4.down && isAuthority)
            {
                resetTp();
                outer.SetNextStateToMain();
            }

            if (fixedAge >= duration && isAuthority && !inputBank.skill4.down && hasTpd)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}*/