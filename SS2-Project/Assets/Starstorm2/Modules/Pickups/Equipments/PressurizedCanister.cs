using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using System.Collections;
using RoR2.ContentManagement;
using System.Collections.Generic;

namespace SS2.Equipments
{
    // still need to make dedicated bonus jump handler but im lazy. this code sucks
    // INFINITE BANDAID GLITCH 2014 (NEWEST) (WORKS!!!)
    public sealed class PressurizedCanister : SS2Equipment, IContentPackModifier
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acPressurizedCannister", SS2Bundle.Equipments);

        public override bool Execute(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;
            if(!body.HasBuff(SS2Content.Buffs.bdCanJump))
                body.AddBuff(SS2Content.Buffs.bdCanJump.buffIndex);
            
            var characterMotor = body.characterMotor;
            if (characterMotor.isGrounded)
                EffectManager.SimpleEffect(Resources.Load<GameObject>("prefabs/effects/SmokescreenEffect"), body.footPosition, Quaternion.identity, true);
            if (characterMotor)
            {
                characterMotor.Motor.ForceUnground();
                EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("canExhaust", SS2Bundle.Equipments), body.transform.position, body.transform.rotation, true);

                float mass = body.rigidbody ? body.rigidbody.mass : 1;
                Vector3 a = characterMotor.moveDirection;
                a.y = 0f;
                float magnitude = a.magnitude;
                if (magnitude > 0f)
                {
                    a /= magnitude;
                }
                Vector3 vector = a * body.moveSpeed * 1.2f;
                vector.y = body.jumpPower * 2.2f;
                float shitass = characterMotor.velocity.y;
                vector.y -= shitass; // ??????????????????????????????????????????????????????????????
                vector *= mass;
                characterMotor.ApplyForce(vector, true, false);
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }


        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
        public sealed class PressurizedCanisterBehavior : BaseBuffBehaviour

        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdCanJump;

            private bool hasJumped;

            protected override void OnFirstStackGained()
            {
                CharacterBody.baseJumpCount++;
                CharacterBody.characterMotor.onHitGroundAuthority += fuck;
                //CharacterBody.characterMotor.onHitGroundServer += RemoveBuff;
                hasJumped = false;
            }

            public void FixedUpdate()
            {
                if (CharacterBody.inputBank.jump.justPressed && !hasJumped)
                {
                    EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("canExhaust", SS2Bundle.Equipments), CharacterBody.transform.position, CharacterBody.transform.rotation, false);
                    hasJumped = true;
                    CharacterBody.baseJumpCount--;
                }
            }

            private void RemoveBuff(ref CharacterMotor.HitGroundInfo hitGroundInfo)
            {
                CharacterBody.RemoveBuff(SS2Content.Buffs.bdCanJump);
            }
            private void fuck(ref CharacterMotor.HitGroundInfo hitGroundInfo)
            {
                if (!hasJumped) CharacterBody.baseJumpCount--;
            }

            protected override void OnAllStacksLost()
            {
                //CharacterBody.characterMotor.onHitGroundServer -= RemoveBuff;
                CharacterBody.characterMotor.onHitGroundAuthority -= fuck;
                if(!hasJumped)
                    CharacterBody.baseJumpCount--;
            }
        }
    }
}