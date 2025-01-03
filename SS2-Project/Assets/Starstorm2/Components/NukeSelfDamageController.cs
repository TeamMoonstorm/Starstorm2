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
    /// <summary>
    /// Class that handles nucleator's Self Damage system and as well keeps track of its charge. In the case of nucleator's mechanics, the "charge coefficient" is equal to the "damageCoefficient".
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class NukeSelfDamageController : MonoBehaviour //FIXME: should be a network behaviour
    {
        [Tooltip("A reference to nucleator's animator, its used to control the \"charge\" param, which is used in a multitude of states for blending purposes")]
        [SerializeField] private Animator nukeAnimator;
        
        [Tooltip("The damage color used by nucleator's self damage")]
        [SerializeField] private SerializableDamageColor _selfDamageColor;
        
        [Tooltip("The maximum amount of damage nucleator can take in a single hit. represented as a coefficient of its max health (0.25 == 25% max health per tick at max charge")]
        [SerializeField] private float _maxHPCoefficientAsDamage;

        [Tooltip("The amount of time between damage ticks.")]
        [SerializeField] private float _timeBetweenTicks;
        
        /// <summary>
        /// Represents the raw "charge" of the nucleator's controller, this raw value can go from 0 to infinity theoretically.
        /// </summary>
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

        /// <summary>
        /// Remapped value of <see cref="charge"/>, clamped between 0 and 1, this is used to determine how much self damage nucleator should take. See <see cref="FixedUpdate"/> where this value is calculated.
        /// </summary>
        public float selfDamageChargeRemap
        {
            get => _selfDamagechargeRemap;
            set => _selfDamagechargeRemap = Mathf.Clamp01(value);
        }
        private float _selfDamagechargeRemap;

        /// <summary>
        /// Remapped value of <see cref="charge"/>, clamped between 0 and 1, this is used for its corsshair's fill mechanic. See <see cref="FixedUpdate"/> where the value is calculated.
        /// </summary>
        public float crosshairChargeRemap
        {
            get => _crosshairChargeRemap;
            set => _crosshairChargeRemap = Mathf.Clamp01(value);
        }
        private float _crosshairChargeRemap;

        /// <summary>
        /// Represents the starting charge coefficient currently used by the controller. This value is given by a state that implementes <see cref="Survivors.Nuke.IChargeableState"/>
        /// <br></br>
        /// Remember, the "charge" values are used as damage coefficient for skills too.
        /// </summary>
        public float startingChargeCoefficient { get; set; }

        /// <summary>
        /// Represents the "soft" cap of the charge system, players may continue to charge for extra damage but they'll start taking damage. This value is given by a state that implements <see cref="Survivors.Nuke.IChargeableState"/>
        /// </summary>
        public float chargeSoftCap { get; set; }

        /// <summary>
        /// Represents the "hard" cap of the charge system, at this point, the player is forced out of their charge state and fires it's attack. The value is given by a state that implements <see cref="Survivors.Nuke.IChargeableState"/>
        /// </summary>
        public float chargeHardCap { get; set; }

        /// <summary>
        /// The current amount of self damage buff stacks, this is used purely as a cosmetic way.
        /// </summary>
        public int selfDamageBuffStacks { get; private set; }

        /// <summary>
        /// Wether Nucleator has his special buff applied.
        /// </summary>
        public bool isImmune => characterBody.HasBuff(SS2Content.Buffs.bdNukeSpecial);

        /// <summary>
        /// Cached reference to Nucleator's Health Component
        /// </summary>
        public HealthComponent healthComponent { get; private set; }

        /// <summary>
        /// Cached reference to Nucleator's characterBody
        /// </summary>
        public CharacterBody characterBody { get; private set; }

        private float _selfDamageStopwatch;

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
            //calculate remaps
            selfDamageChargeRemap = Util.Remap(charge, chargeSoftCap, chargeHardCap, 0, 1);
            crosshairChargeRemap = Util.Remap(charge, startingChargeCoefficient, chargeHardCap, 0, 1);

            //Set the buff stacks, this is kinda stinky honestly.
            SetBuffStacks();

            //increment timer then try to deal damage.
            _selfDamageStopwatch += Time.fixedDeltaTime;
            if (_selfDamageStopwatch > _timeBetweenTicks)
            {
                _selfDamageStopwatch -= _timeBetweenTicks;
                TryDealDamage();
            }
        }

        private void SetBuffStacks()
        {
            //If nucleator is immune, set stacks to 0 then remove all instances of the buff.
            if (isImmune)
            {
                selfDamageBuffStacks = 0;
                if (characterBody.GetBuffCount(SS2Content.Buffs.bdNukeSelfDamage) > 0 && NetworkServer.active)
                {
                    characterBody.RemoveBuff(SS2Content.Buffs.bdNukeSelfDamage);
                }
                return;
            }

            //Since the in-min of the selfDamageRemap starts at "chargeSoftCap", this value will only become greater than 0 once "charge" is over said threshold, nucleator can have up to 10 stacks of self damage.
            int newBuffStacks = Mathf.FloorToInt(selfDamageChargeRemap * 10);
            //If theyre different, set them in the network
            if (newBuffStacks != selfDamageBuffStacks)
            {
                selfDamageBuffStacks = newBuffStacks;
                if (NetworkServer.active)
                    characterBody.SetBuffCount(SS2Content.Buffs.bdNukeSelfDamage.buffIndex, selfDamageBuffStacks);
            }
        }

        private void Update()
        {
            //Update the animator's charge value, use the crosshair charge as that one goes from 0 to 1
            if(nukeAnimator)
            {
                //FIXME: Create a hash for "charge" then use the hash instead.
                nukeAnimator.SetFloat("charge", _crosshairChargeRemap);
            }
        }

        private void TryDealDamage()
        {
            //Only deal damage if appropiate
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

        /// <summary>
        /// Method that should be called when nucleator enters a new chargable state.
        /// <br></br>
        /// <see cref="EntityStates.Nuke.BaseNukeChargeState"/>
        /// </summary>
        /// <param name="chargeableState">The chargable entity sate that just entered.</param>
        public void SetDefaults(Survivors.Nuke.IChargeableState chargeableState)
        {
            //If the interface is null, set vlaues to default.
            charge = chargeableState?.currentCharge ?? 0;
            chargeHardCap = chargeableState?.chargeCoefficientHardCap ?? 1;
            chargeSoftCap = chargeableState?.chargeCoefficientSoftCap ?? 0;
            startingChargeCoefficient = chargeableState?.startingChargeCoefficient ?? 0;
        }
    }
}