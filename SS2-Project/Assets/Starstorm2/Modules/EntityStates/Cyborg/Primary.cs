using RoR2;
using UnityEngine;

namespace EntityStates.Borg
{
    public class BorgFireBaseShot : BaseSkillState
    {
        public static float damageCoefficient = 2.5f;
        public float baseDuration = 0.5f;
        public float recoil = 1f;
        public static int CurrentMuzzleIndex = 0;

        [HideInInspector]
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerHuntressSnipe");
        [HideInInspector]
        public GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/HitsparkCommandoShotgun");
        [HideInInspector]
        public GameObject critEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/critspark");

        //public static bool switchHand;

        private float duration;
        private float fireDuration;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;
        private BorgMuzzle borgMuzzle;



        public override void OnEnter()
        {
            //Store the muzzle we're firing now.
            borgMuzzle = (BorgMuzzle)CurrentMuzzleIndex;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireDuration = 0.25f * duration;
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            switch (borgMuzzle)
            {
                case BorgMuzzle.MuzzleEye:
                    muzzleString = "MuzzleEye";
                    //Anim
                    break;
                case BorgMuzzle.MuzzleLeft:
                    muzzleString = "MuzzleLeft";
                    PlayAnimation("Gesture, Override", "FireM1", "FireM1.playbackRate", duration);
                    break;
                case BorgMuzzle.MuzzleRight:
                    muzzleString = "MuzzleRight";
                    PlayAnimation("Gesture, Override", "FireM1Alt", "FireM1.playbackRate", duration);
                    break;

            }

            /*if (switchHand)
            {
                muzzleString = "Lowerarm.R_end";
                PlayAnimation("Gesture, Override", "FireM1", "FireM1.playbackRate", duration);
            }
            else
            {
                muzzleString = "Lowerarm.L_end";
                PlayAnimation("Gesture, Override", "FireM1Alt", "FireM1.playbackRate", duration);
            }*/

            SwitchMuzzle();
        }

        private void SwitchMuzzle()
        {
            if (borgMuzzle == BorgMuzzle.MuzzleLeft)
                CurrentMuzzleIndex = (int)BorgMuzzle.MuzzleRight;
            else if (borgMuzzle == BorgMuzzle.MuzzleRight)
                CurrentMuzzleIndex = (int)BorgMuzzle.MuzzleEye;
            else if (borgMuzzle == BorgMuzzle.MuzzleEye)
                CurrentMuzzleIndex = (int)BorgMuzzle.MuzzleLeft;
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        private void FireShot()
        {
            if (!hasFired)
            {
                hasFired = true;

                characterBody.AddSpreadBloom(0.35f);
                Ray aimRay = GetAimRay();
                //EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);

                /*if (!switchHand) // use a variable for that once sounds are no longer placeholders.
                    Util.PlaySound(Commando.CommandoWeapon.FirePistol.firePistolSoundString, base.gameObject);
                else
                    Util.PlaySound(Commando.CommandoWeapon.FirePistol2.firePistolSoundString, base.gameObject);*/
                string soundString = "BorgPrimary";//base.effectComponent.shootSound;
                //if (isCrit) soundString += "Crit";
                Util.PlaySound(soundString, gameObject);

                if (isAuthority)
                {
                    new BulletAttack
                    {
                        owner = gameObject,
                        weapon = gameObject,
                        origin = aimRay.origin,
                        aimVector = aimRay.direction,
                        minSpread = 0,
                        maxSpread = 0,
                        damage = damageCoefficient * damageStat,
                        force = 100,
                        radius = .35f,
                        tracerEffectPrefab = tracerEffectPrefab,
                        muzzleName = muzzleString,
                        hitEffectPrefab = Util.CheckRoll(critStat, characterBody.master) ? critEffectPrefab : effectPrefab,
                        isCrit = Util.CheckRoll(critStat, characterBody.master)
                    }.Fire();
                    //ProjectileManager.instance.FireProjectile(ExampleSurvivor.ExampleSurvivor.bfgProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageCoefficient * damageStat, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireDuration)
            {
                FireShot();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        private enum BorgMuzzle : int
        {
            MuzzleLeft = 0,
            MuzzleRight = 1,
            MuzzleEye = 2,
        }
    }
}