using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
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
        public float ChargeRemap
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
        public float ChargeSoftCap { get; set; }
        public float ChargeHardCap { get; set; }
        public int SelfDamageBuffStacks { get; private set; }
        public bool IsImmune => CharacterBody.HasBuff(SS2Content.Buffs.bdNukeSpecial);
        public HealthComponent HealthComponent { get; private set; }
        public CharacterBody CharacterBody { get; private set; }
        private float _timer;

        private void Awake()
        {
            HealthComponent = GetComponent<HealthComponent>();
            CharacterBody = GetComponent<CharacterBody>();
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
            if(IsImmune)
            {
                SelfDamageBuffStacks = 0;
                if (CharacterBody.GetBuffCount(SS2Content.Buffs.bdNukeSelfDamage) > 0 && NetworkServer.active)
                {
                    CharacterBody.RemoveBuff(SS2Content.Buffs.bdNukeSelfDamage);
                }
                return;
            }

            ChargeRemap = Util.Remap(Charge, ChargeSoftCap, ChargeHardCap, 0, 1);
            int newBuffStacks = Mathf.FloorToInt(ChargeRemap * 10);
            if(newBuffStacks != SelfDamageBuffStacks)
            {
                SelfDamageBuffStacks = newBuffStacks;
                if (NetworkServer.active)
                    CharacterBody.SetBuffCount(SS2Content.Buffs.bdNukeSelfDamage.buffIndex, SelfDamageBuffStacks);
            }
        }

        private void TryDealDamage()
        {
            if (IsImmune || !NetworkServer.active || SelfDamageBuffStacks < 1)
                return;

            float maxHPCoefficient = _maxHPCoefficientAsDamage * ChargeRemap;
            float damage = CharacterBody.maxHealth * maxHPCoefficient;
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
            HealthComponent.TakeDamage(damageInfo);
        }

        public void SetDefaults(EntityStates.Nuke.Weapon.BaseNukeWeaponChargeState weaponState)
        {
            Charge = weaponState?.CurrentCharge ?? 0;
            ChargeHardCap = weaponState?.chargeCoefficientHardCap ?? 0;
            ChargeSoftCap = weaponState?.chargeCoefficientSoftCap ?? 0;
        }
    }
}