/*using EntityStates;
using EntityStates.Captain.Weapon;
using RoR2;
using Moonstorm.Starstorm2.Buffs;
using UnityEngine;

namespace EntityStates.Nucleator
{
    class NucleatorSkillStateBase : BaseSkillState
    {
        public static float baseMaxChargeTime = 2F;
        private static GameObject chargeupVfxPrefab = ChargeCaptainShotgun.chargeupVfxPrefab;
        public static float overchargeThreshold = 0.66F;
        public static float maxOverchargePlayerDamageDealt = 0.5F;


        private Transform muzzleTransform;
        private GameObject chargeupVfxGameObject;

        public float charge;
        public float lastCharge;
        public float lastDamage;
        public float maxChargeTime;

        private float playerHealthFinal;
        private float playerHealth;
        private float damageDealt;
        private float nextDamageInstance = 0.05f;
        private bool isCrosshairInitialized = false;
        private GameObject defaultCrosshair;

        public override void OnEnter()
        {
            base.OnEnter();
            maxChargeTime = baseMaxChargeTime / attackSpeedStat;
            playerHealth = characterBody.healthComponent.combinedHealth;
            playerHealthFinal = playerHealth - playerHealth * maxOverchargePlayerDamageDealt;
            defaultCrosshair = characterBody.crosshairPrefab;
            characterBody.crosshairPrefab = Survivors.Nucleator.chargeCrosshair;

            if (!isCrosshairInitialized)
            {
                foreach (Transform child in Survivors.Nucleator.chargeCrosshair.transform)
                {
                    if (child.name == "Zoom Amount") Destroy(child.gameObject);
                }
                isCrosshairInitialized = true;
            }
            Transform modelTransform = GetModelTransform();

            if (modelTransform)
            {
                muzzleTransform = FindModelChild("MuzzleCenter");
                if (muzzleTransform)
                {
                    chargeupVfxGameObject = Object.Instantiate(chargeupVfxPrefab, muzzleTransform);
                    chargeupVfxGameObject.GetComponent<ScaleParticleSystemDuration>().newDuration = maxChargeTime;
                }
            }
        }
        public override void OnExit()
        {
            if (damageDealt > 0 && playerHealth - playerHealthFinal > damageDealt)
            {
                TakeDamage();
            }

            if (chargeupVfxGameObject)
            {
                Destroy(chargeupVfxGameObject);
                chargeupVfxGameObject = null;
            }

            characterBody.crosshairPrefab = defaultCrosshair;
            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.SetAimTimer(1f);
            lastCharge = charge;
            charge = fixedAge / maxChargeTime;
            if (charge <= 1.0f
                   && charge > overchargeThreshold
                   && charge - overchargeThreshold > nextDamageInstance)
            {
                damageDealt += TakeDamage();
                nextDamageInstance += (1 - overchargeThreshold) / 5;
            }

        }

        private float CalculateDamageInstance()
        {
            var chargeCoef = (charge - overchargeThreshold) / (1 - overchargeThreshold);
            var damageCoef = chargeCoef * maxOverchargePlayerDamageDealt;
            var damage = Mathf.Abs(chargeCoef * (playerHealthFinal - playerHealth)) - damageDealt;
            return damage;
        }

        private float TakeDamage()
        {
            var damage = CalculateDamageInstance();
            if (!characterBody.HasBuff(NucleatorSpecial.buff))
            {
                characterBody.healthComponent.TakeDamage(
                    new DamageInfo()
                    {
                        damageType = DamageType.BypassOneShotProtection | DamageType.BypassArmor | DamageType.NonLethal,
                        crit = false,
                        damage = damage,
                        position = characterBody.transform.position
                    });
            }
            return damage;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}

*/