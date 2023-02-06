using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Equipments
{
    public sealed class PressurizedCanister : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("PressurizedCanister", SS2Bundle.Equipments);

        public override bool FireAction(EquipmentSlot slot)
        {
            var characterMotor = slot.characterBody.characterMotor;
            if (characterMotor)
            {
                slot.characterBody.AddItemBehavior<Behavior>(1);
                Debug.Log($"Adding behavior");
                if (slot.hasAuthority)
                {
                }
                return true;
            }
            return false;
        }

        //notice how this is not added by this.AddItemBehavior() but the Fire Action
        public sealed class Behavior : CharacterBody.ItemBehavior
        {
            private static float duration = 3f;
            private static float thrustForce = 90f;
            private static int effectSpawnTotal = 10;


            private float stopwatch;
            private int effectInterval;
            private CharacterMotor characterMotor;

            private void Start()
            {
                characterMotor = body.characterMotor;
                if (characterMotor.isGrounded)
                    EffectManager.SimpleEffect(Resources.Load<GameObject>("prefabs/effects/SmokescreenEffect"), body.footPosition, Quaternion.identity, false);
                if (body.hasAuthority)
                {
                    characterMotor.Motor.ForceUnground();
                    characterMotor.velocity = new Vector3(characterMotor.velocity.x, Mathf.Max(characterMotor.velocity.y, 15f), characterMotor.velocity.z);
                }
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
                bool funnyCanister = SS2Config.EnableFunnyCanister.Value;
                if (characterMotor.enabled)
                {
                    if (funnyCanister || body.inputBank.jump.down)
                    {
                        characterMotor.ApplyForce(Vector3.up * thrustForce * (body.rigidbody.mass / 100f));
                        if (stopwatch >= (duration / effectSpawnTotal * effectInterval))
                        {
                            EffectData effectData = new EffectData();
                            effectData.origin = body.footPosition;
                            effectData.scale = 0.5f;
                            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/impacteffects/CharacterLandImpact"), effectData, true);
                        }
                    }
                    if (stopwatch >= (duration / effectSpawnTotal * effectInterval))
                        effectInterval++;
                }
            }
        }
    }
}
