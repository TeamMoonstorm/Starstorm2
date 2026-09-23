using EntityStates;
using EntityStates.NemToolbot;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class NemToolbotController : NetworkBehaviour
    {
        public enum WeaponType : byte
        {
            Shotgun = 0,
            RapidLaser = 1,
            GrenadeLauncher = 2,
            SniperLaser = 3
        }

        public static float shotgunMaxRange = 3f;
        public static float rapidLaserMaxRange = 6f;
        public static float grenadeLauncherMaxRange = 9f;

        public static int shotgunMaxAmmo = 10;
        public static int rapidLaserMaxAmmo = 50;
        public static int grenadeLauncherMaxAmmo = 10;
        public static int sniperLaserMaxAmmo = 10;

        public static float shotgunRegenRate = 1f;
        public static float rapidLaserRegenRate = 5f;
        public static float grenadeLauncherRegenRate = 1f;
        public static float sniperLaserRegenRate = 1f;

        public GenericSkill ballPrimary;
        public GenericSkill ballSecondary;
        public GenericSkill ballSpecial;

        // States apply form immediately; this also supplies the form to newly spawned peers.
        [SyncVar(hook = nameof(OnFormChanged))]
        private bool syncedBallForm;

        public WeaponType currentWeapon { get; private set; } = WeaponType.RapidLaser;
        public bool isBallForm { get; private set; }

        // Like GenericSkill stock, these resources belong to the firing authority.
        private int[] ammo;
        private readonly float[] regenRemainders = new float[4];
        private CharacterBody characterBody;
        private CharacterMotor characterMotor;
        private SkillLocator skillLocator;
        private EntityStateMachine bodyStateMachine;
        private EntityStateMachine hookStateMachine;
        private SerializableEntityStateType deployedMainState;
        private GenericSkill deployedPrimary;
        private GenericSkill deployedSecondary;
        private GenericSkill deployedSpecial;
        private ModelLocator modelLocator;
        // private Animator modelAnimator;
        private float originalAirControl;
        // private float originalAimWeight;
        private bool originalNormalizeToFloor;
        private bool armorApplied;
        private bool initialized;
        // private uint ballLoopSoundID;
        // private static readonly int aimWeightHash = Animator.StringToHash("aimWeight");

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            characterMotor = GetComponent<CharacterMotor>();
            skillLocator = GetComponent<SkillLocator>();
            bodyStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
            hookStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Hook");
            modelLocator = GetComponent<ModelLocator>();
            // if (modelLocator && modelLocator.modelTransform)
            //     modelAnimator = modelLocator.modelTransform.GetComponent<Animator>();

            ammo = new[] { shotgunMaxAmmo, rapidLaserMaxAmmo, grenadeLauncherMaxAmmo, sniperLaserMaxAmmo };
            if (!characterBody || !characterMotor || !skillLocator || !bodyStateMachine || !hookStateMachine ||
                !skillLocator.primary || !skillLocator.secondary || !skillLocator.special ||
                !ballPrimary || !ballSecondary || !ballSpecial)
            {
                SS2Log.Error("NemToolbotController: Body, motor, Body/Hook state machines and deployed/ball skill slots must be authored on " + gameObject.name);
                enabled = false;
                return;
            }

            deployedMainState = bodyStateMachine.mainStateType;
            deployedPrimary = skillLocator.primary;
            deployedSecondary = skillLocator.secondary;
            deployedSpecial = skillLocator.special;
            initialized = true;
        }

        private void OnEnable()
        {
            if (!initialized)
                return;
            RecalculateStatsAPI.GetStatCoefficients += ModifyStats;
            characterBody.onRecalculateStats += ModifyAcceleration;
        }

        private void OnDisable()
        {
            RecalculateStatsAPI.GetStatCoefficients -= ModifyStats;
            if (characterBody)
                characterBody.onRecalculateStats -= ModifyAcceleration;
            if (NetworkServer.active)
                syncedBallForm = false;
            ApplyForm(false);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            ApplyForm(syncedBallForm);
        }

        public bool SetBallForm(bool ballForm)
        {
            if (!initialized || !enabled)
                return false;
            if (NetworkServer.active)
                syncedBallForm = ballForm;
            ApplyForm(ballForm);
            return true;
        }

        private void OnFormChanged(bool ballForm)
        {
            syncedBallForm = ballForm;
            // The owner already applied its state transition; don't replay a delayed server echo.
            if (!Util.HasEffectiveAuthority(gameObject))
                ApplyForm(ballForm);
        }

        private void ApplyForm(bool ballForm)
        {
            if (!initialized || isBallForm == ballForm)
                return;

            if (!ballForm && hookStateMachine.state is FireGrapplingHook hook)
            {
                if (NetworkServer.active && hook.hookInstance)
                    Destroy(hook.hookInstance);
                // Cancel before changing secondary: Loader deducts the currently bound slot on impact.
                if (Util.HasEffectiveAuthority(gameObject))
                    hookStateMachine.SetState(EntityStateCatalog.InstantiateState(ref hookStateMachine.mainStateType));
            }

            isBallForm = ballForm;
            bodyStateMachine.mainStateType = ballForm
                ? new SerializableEntityStateType(typeof(BallMainState))
                : deployedMainState;
            skillLocator.primary = ballForm ? ballPrimary : deployedPrimary;
            skillLocator.secondary = ballForm ? ballSecondary : deployedSecondary;
            skillLocator.special = ballForm ? ballSpecial : deployedSpecial;

            if (ballForm)
            {
                // Util.PlaySound(BallMainState.enterSoundString, gameObject);
                // ballLoopSoundID = Util.PlaySound(BallMainState.loopSoundString, gameObject);
                originalAirControl = characterMotor.airControl;
                characterMotor.airControl = BallMainState.ballAirControl;
                if (modelLocator)
                {
                    originalNormalizeToFloor = modelLocator.normalizeToFloor;
                    modelLocator.normalizeToFloor = true;
                }
                // if (modelAnimator)
                // {
                //     originalAimWeight = modelAnimator.GetFloat(aimWeightHash);
                //     modelAnimator.SetFloat(aimWeightHash, 0f);
                // }
                if (NetworkServer.active)
                {
                    characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
                    armorApplied = true;
                }
            }
            else
            {
                // if (ballLoopSoundID != 0)
                //     AkSoundEngine.StopPlayingID(ballLoopSoundID);
                // Util.PlaySound(BallMainState.exitSoundString, gameObject);
                characterMotor.airControl = originalAirControl;
                if (modelLocator)
                    modelLocator.normalizeToFloor = originalNormalizeToFloor;
                // if (modelAnimator)
                //     modelAnimator.SetFloat(aimWeightHash, originalAimWeight);
                if (NetworkServer.active && armorApplied)
                {
                    characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
                    armorApplied = false;
                }
                System.Array.Clear(regenRemainders, 0, regenRemainders.Length);
                if (Util.HasEffectiveAuthority(gameObject))
                    characterBody.isSprinting = false;
            }
            characterBody.statsDirty = true;
        }

        private void ModifyStats(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body == characterBody && isBallForm)
                args.baseMoveSpeedAdd += body.baseMoveSpeed * (BallMainState.moveSpeedMultiplier - 1f);
        }

        private void ModifyAcceleration(CharacterBody body)
        {
            if (isBallForm)
            {
                // Vanilla divides by baseMoveSpeed; the form no longer mutates that base field.
                body.acceleration *= BallMainState.accelerationMultiplier / BallMainState.moveSpeedMultiplier;
            }
        }

        private void FixedUpdate()
        {
            if (isBallForm && characterBody.healthComponent.alive && Util.HasEffectiveAuthority(gameObject))
            {
                RegenWeaponAmmo(WeaponType.Shotgun, shotgunRegenRate);
                RegenWeaponAmmo(WeaponType.RapidLaser, rapidLaserRegenRate);
                RegenWeaponAmmo(WeaponType.GrenadeLauncher, grenadeLauncherRegenRate);
                RegenWeaponAmmo(WeaponType.SniperLaser, sniperLaserRegenRate);
            }
        }

        public static WeaponType GetWeaponFromRange(float distance)
        {
            if (distance < shotgunMaxRange)
                return WeaponType.Shotgun;
            if (distance < rapidLaserMaxRange)
                return WeaponType.RapidLaser;
            if (distance < grenadeLauncherMaxRange)
                return WeaponType.GrenadeLauncher;
            return WeaponType.SniperLaser;
        }

        public void SetWeapon(WeaponType weapon)
        {
            if (!IsValidWeapon(weapon))
                return;
            if (Util.HasEffectiveAuthority(gameObject))
                currentWeapon = weapon;
        }

        private static bool IsValidWeapon(WeaponType weapon)
        {
            if (weapon <= WeaponType.SniperLaser)
                return true;
            SS2Log.Error("NemToolbotController: Invalid weapon " + weapon);
            return false;
        }

        public static int GetMaxAmmo(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Shotgun: return shotgunMaxAmmo;
                case WeaponType.RapidLaser: return rapidLaserMaxAmmo;
                case WeaponType.GrenadeLauncher: return grenadeLauncherMaxAmmo;
                case WeaponType.SniperLaser: return sniperLaserMaxAmmo;
                default:
                    SS2Log.Error("NemToolbotController: Invalid weapon " + weapon);
                    return 0;
            }
        }

        public int GetAmmo(WeaponType weapon)
        {
            return IsValidWeapon(weapon) ? ammo[(int)weapon] : 0;
        }

        public bool HasAmmo(WeaponType weapon)
        {
            return enabled && GetAmmo(weapon) > 0;
        }

        public bool TryConsumeAmmo(WeaponType weapon)
        {
            if (!Util.HasEffectiveAuthority(gameObject) || !HasAmmo(weapon))
                return false;
            ammo[(int)weapon]--;
            return true;
        }

        private void RegenWeaponAmmo(WeaponType weapon, float regenRate)
        {
            int index = (int)weapon;
            int maxAmmo = GetMaxAmmo(weapon);
            if (ammo[index] >= maxAmmo)
            {
                regenRemainders[index] = 0f;
                return;
            }
            regenRemainders[index] += regenRate * Time.fixedDeltaTime;
            int toAdd = Mathf.FloorToInt(regenRemainders[index]);
            ammo[index] = Mathf.Min(ammo[index] + toAdd, maxAmmo);
            regenRemainders[index] = ammo[index] == maxAmmo ? 0f : regenRemainders[index] - toAdd;
        }

        public float GetDamageMultiplierFromSpeed(float speed)
        {
            float baseSpeed = characterBody.baseMoveSpeed * (isBallForm ? BallMainState.moveSpeedMultiplier : 1f);
            return baseSpeed > 0f ? Mathf.Max(1f, speed / baseSpeed) : 1f;
        }
    }
}
