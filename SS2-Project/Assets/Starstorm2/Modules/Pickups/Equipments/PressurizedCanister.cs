using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static AkMIDIEvent;

namespace Moonstorm.Starstorm2.Equipments
{
    [DisabledContent]
    public sealed class PressurizedCanister : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("PressurizedCanister", SS2Bundle.Equipments);

        //[ConfigurableField(SS2Config.IDItem, ConfigName = "No Jump Control", ConfigDesc = "Set to true to disable jump control on Pressurized Canister - activating the equipment will apply constant upward force regardless of whether you hold the jump button. This may lead to Funny and Memorable (tm) moments, especially if you like picking up Gestures of the Drowned.")]
        //public static bool funnyCanister = false;

        public override bool FireAction(EquipmentSlot slot)
        {
            var characterMotor = slot.characterBody.characterMotor;
            if (characterMotor)
            {
                slot.characterBody.AddItemBehavior<Behavior>(1);
                Debug.Log($"Adding behavior");
                //if (slot.hasAuthority)
                {
                } //this is so fucking true
                return true;
            }
            return false;
        }

        //notice how this is not added by this.AddItemBehavior() but the Fire Action
        public sealed class Behavior : CharacterBody.ItemBehavior
        {
            private static float duration = 0.8f;
            private static float thrustForce = 90f;
            private static int effectSpawnTotal = 10;

            //private AnimationCurve curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.7f * duration, 1), new Keyframe(duration, 0));

            private float stopwatch;
            private int effectInterval;
            private CharacterMotor characterMotor;

            private void Start()
            {
                characterMotor = body.characterMotor;
                if (characterMotor.isGrounded)
                    EffectManager.SimpleEffect(Resources.Load<GameObject>("prefabs/effects/SmokescreenEffect"), body.footPosition, Quaternion.identity, true);
                if (NetworkServer.active)
                {
                    characterMotor.Motor.ForceUnground();
                    EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("canExhaust", SS2Bundle.Equipments), body.transform.position, body.transform.rotation, true);
                    body.AddBuff(SS2Content.Buffs.bdCanJump.buffIndex);
                    body.characterMotor.Jump(1.2f, 2.2f, false); //it would be so awesome 
                    //body.baseJumpCount++; done through invisible buff so it clears after landing //it would be so cool
                    // characterMotor.velocity = new Vector3(characterMotor.velocity.x, Mathf.Max(characterMotor.velocity.y, 15f), characterMotor.velocity.z); //it would be the most incredible superher
                } //why was there all of this complicated stuff instead of just a jump..?
            }

            private void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if (body.hasAuthority)
                    AuthorityUpdate();

                //Gets removed if you switch equipments
                if (stopwatch > duration || body.equipmentSlot.equipmentIndex != EquipmentCatalog.FindEquipmentIndex("PressurizedCanister"))
                    Destroy(this);
            }

            public void AuthorityUpdate()
            {
                //if (characterMotor.enabled)
                //{
                //    if (funnyCanister || body.inputBank.jump.down)
                //    {
                //        characterMotor.ApplyForce(Vector3.up * thrustForce * (body.rigidbody.mass / 100f));
                //        if (stopwatch >= (duration / effectSpawnTotal * effectInterval))
                //        {
                //            EffectData effectData = new EffectData();
                //            effectData.origin = body.footPosition;
                //            effectData.scale = 0.5f;
                //            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/impacteffects/CharacterLandImpact"), effectData, true);
                //        }
                //    }
                //    if (stopwatch >= (duration / effectSpawnTotal * effectInterval))
                //        effectInterval++;
                //}

                //characterMotor.rootMotion += Vector3.up * (body.moveSpeed * EntityStates.Executioner2.ExecuteLeap.speedCoefficientCurve.Evaluate(stopwatch / (duration * 0.6f)) * Time.fixedDeltaTime * 10f);
                
                //characterMotor.velocity.y = 0f;
            }
        }
    }
}
