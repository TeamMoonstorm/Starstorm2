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
        public float Charge
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
        public float chargeRemap
        {
            get
            {
                return _chargeRemap;
            }
            set
            {
                _chargeRemap = Mathf.Clamp01(value);
            }
        }
        private float _chargeRemap;
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
        private void FixedUpdate()
        {
            SetBuffStacks();
            _timer += Time.fixedDeltaTime;
            if(_timer > _timeBetweenTicks)
            {
                _timer -= _timeBetweenTicks;
                TryDealDamage();
            }
        }

        private void SetBuffStacks()
        {
            if(isImmune)
            {
                selfDamageBuffStacks = 0;
                if (characterBody.GetBuffCount(SS2Content.Buffs.bdNukeSelfDamage) > 0 && NetworkServer.active)
                {
                    characterBody.RemoveBuff(SS2Content.Buffs.bdNukeSelfDamage);
                }
                return;
            }

            chargeRemap = Util.Remap(Charge, chargeSoftCap, chargeHardCap, 0, 1);
            int newBuffStacks = Mathf.FloorToInt(chargeRemap * 10);
            if(newBuffStacks != selfDamageBuffStacks)
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

            float maxHPCoefficient = _maxHPCoefficientAsDamage * chargeRemap;
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
            Charge = chargeableState?.currentCharge ?? 0;
            chargeHardCap = chargeableState?.chargeCoefficientHardCap ?? 0;
            chargeSoftCap = chargeableState?.chargeCoefficientSoftCap ?? 0;
        }
    }
}