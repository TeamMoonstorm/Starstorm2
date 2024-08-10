using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    [RequireComponent(typeof(HealthComponent))]
    public class NukeSelfDamageController : MonoBehaviour
    {
        [SerializeField] private SerializableDamageColor _selfDamageColor;
        [SerializeField] private float _maxHPCoefficientAsDamage;
        [SerializeField] private float _timeBetweenTicks;
        public float charge
        {
            get
            {
                return _charge;
            }
            set
            {
                _charge = Mathf.Max(value, 0);
            }
        }
        private float _charge;
        public float selfDamageChargeRemap
        {
            get => _selfDamagechargeRemap;
            set => _selfDamagechargeRemap = Mathf.Clamp01(value);
        }
        private float _selfDamagechargeRemap;
        public float crosshairChargeRemap
        {
            get => _crosshairChargeRemap;
            set => _crosshairChargeRemap = Mathf.Clamp01(value);
        }
        private float _crosshairChargeRemap;
        public float startingChargeCoefficient { get; set; }
        public float chargeSoftCap { get; set; }
        public float chargeHardCap { get; set; }
        public int selfDamageBuffStacks { get; private set; }
        public bool isImmune => characterBody.HasBuff(SS2Content.Buffs.bdNukeSpecial);
        public HealthComponent healthComponent { get; private set; }
        public CharacterBody characterBody { get; private set; }
        private float _timer;

        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
            characterBody = GetComponent<CharacterBody>();
        }

        //Gotta make sure these values are at least valid for the first few moments to make the remap function return a proper value and not NaN
        private void Start()
        {
            charge = 0;
            crosshairChargeRemap = 0;
            selfDamageChargeRemap = 0;
            startingChargeCoefficient = 0;
            chargeSoftCap = 0;
            chargeHardCap = 1;
        }
        private void FixedUpdate()
        {
            selfDamageChargeRemap = Util.Remap(charge, chargeSoftCap, chargeHardCap, 0, 1);
            crosshairChargeRemap = Util.Remap(charge, startingChargeCoefficient, chargeHardCap, 0, 1);
            SetBuffStacks();
            _timer += Time.fixedDeltaTime;
            if (_timer > _timeBetweenTicks)
            {
                _timer -= _timeBetweenTicks;
                TryDealDamage();
            }
        }

        private void SetBuffStacks()
        {
            if (isImmune)
            {
                selfDamageBuffStacks = 0;
                if (characterBody.GetBuffCount(SS2Content.Buffs.bdNukeSelfDamage) > 0 && NetworkServer.active)
                {
                    characterBody.RemoveBuff(SS2Content.Buffs.bdNukeSelfDamage);
                }
                return;
            }

            int newBuffStacks = Mathf.FloorToInt(selfDamageChargeRemap * 10);
            if (newBuffStacks != selfDamageBuffStacks)
            {
                selfDamageBuffStacks = newBuffStacks;
                if (NetworkServer.active)
                    characterBody.SetBuffCount(SS2Content.Buffs.bdNukeSelfDamage.buffIndex, selfDamageBuffStacks);
            }
        }

        private void TryDealDamage()
        {
            if (isImmune || !NetworkServer.active || selfDamageBuffStacks < 1)
                return;

            float maxHPCoefficient = _maxHPCoefficientAsDamage * selfDamageChargeRemap;
            float damage = characterBody.maxHealth * maxHPCoefficient;
            DamageInfo damageInfo = new DamageInfo
            {
                attacker = gameObject,
                inflictor = gameObject,
                crit = false,
                damage = damage,
                damageType = DamageType.BypassArmor | DamageType.BypassBlock | DamageType.BypassOneShotProtection | DamageType.NonLethal,
                damageColorIndex = _selfDamageColor.DamageColorIndex,
                dotIndex = DotController.DotIndex.None,
                position = transform.position,
                procCoefficient = 0,
            };
            healthComponent.TakeDamage(damageInfo);
        }

        public void SetDefaults(Survivors.Nuke.IChargeableState chargeableState)
        {
            charge = chargeableState?.currentCharge ?? 0;
            chargeHardCap = chargeableState?.chargeCoefficientHardCap ?? 1;
            chargeSoftCap = chargeableState?.chargeCoefficientSoftCap ?? 0;
            startingChargeCoefficient = chargeableState?.startingChargeCoefficient ?? 0;
        }
    }
}