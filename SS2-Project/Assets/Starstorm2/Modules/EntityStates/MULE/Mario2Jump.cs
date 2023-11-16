using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.MULE
{
    public class Mario2Jump : GenericCharacterMain
    {
        public bool delayedInputReceived = false;
        public float charge;
		public float jumpCharge; 
        public override void ProcessJump()
        {
            if (inputBank.jump.down && characterMotor.isGrounded)
            {
                jumpInputReceived = false;
                delayedInputReceived = true;
                charge++;
            }
            else if (delayedInputReceived == true)
            {
				delayedInputReceived = false;
				if (characterMotor.isGrounded)
				{
					if (charge > 24)
						charge = 24;
					charge /= 24;
					jumpCharge = Util.Remap(charge, 0f, 1f, 1f, 2.4f);
					charge = 0;
					jumpInputReceived = true;
				}
            }

			if (hasCharacterMotor)
			{
				bool flag = false;
				bool flag2 = false;
				if (jumpInputReceived && characterBody && characterMotor.jumpCount < characterBody.maxJumpCount)
				{
					int itemCount = characterBody.inventory.GetItemCount(RoR2Content.Items.JumpBoost);
					float horizontalBonus = 1f;
					float verticalBonus = 1f;
					if (characterMotor.jumpCount >= characterBody.baseJumpCount)
					{
						flag = true;
						horizontalBonus = 1.5f;
						verticalBonus = 1.5f;
					}
					else if (itemCount > 0f && characterBody.isSprinting)
					{
						float num = characterBody.acceleration * characterMotor.airControl;
						if (characterBody.moveSpeed > 0f && num > 0f)
						{
							flag2 = true;
							float num2 = Mathf.Sqrt(10f * itemCount / num);
							float num3 = characterBody.moveSpeed / num;
							horizontalBonus = (num2 + num3) / num3;
						}
					}
					ApplyJumpVelocity(characterMotor, characterBody, horizontalBonus, verticalBonus * jumpCharge, false);
					jumpCharge = 1f;
					if (hasModelAnimator)
					{
						int layerIndex = modelAnimator.GetLayerIndex("Body");
						if (layerIndex >= 0)
						{
							if (characterMotor.jumpCount == 0 || characterBody.baseJumpCount == 1)
							{
								modelAnimator.CrossFadeInFixedTime("Jump", smoothingParameters.intoJumpTransitionTime, layerIndex);
							}
							else
							{
								modelAnimator.CrossFadeInFixedTime("BonusJump", smoothingParameters.intoJumpTransitionTime, layerIndex);
							}
						}
					}
					if (flag)
					{
						EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), new EffectData
						{
							origin = characterBody.footPosition
						}, true);
					}
					else if (characterMotor.jumpCount > 0)
					{
						EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CharacterLandImpact"), new EffectData
						{
							origin = characterBody.footPosition,
							scale = characterBody.radius
						}, true);
					}
					if (flag2)
					{
						EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
						{
							origin = characterBody.footPosition,
							rotation = Util.QuaternionSafeLookRotation(characterMotor.velocity)
						}, true);
					}
					characterMotor.jumpCount++;
				}
			}
		}
    }
}
