using System;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using UnityEngine;
using Moonstorm.Starstorm2;
namespace EntityStates.AI.Walker
{
	public class Fear : BaseAIState
	{
		public GameObject fearTarget;

		private float aiUpdateTimer;

		protected Vector3 myBodyFootPosition;

		private float lastPathUpdate;

		private float fallbackNodeStartAge;
		private readonly float fallbackNodeDuration = 4f;
		public override void OnEnter()
		{
			base.OnEnter();
			if (base.ai)
			{
				this.lastPathUpdate = base.ai.broadNavigationAgent.output.lastPathUpdate;
				base.ai.broadNavigationAgent.InvalidatePath();
			}
			this.fallbackNodeStartAge = float.NegativeInfinity;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.ai && base.body)
			{
				this.aiUpdateTimer -= Time.fixedDeltaTime;
				this.UpdateFootPosition();
				if (this.aiUpdateTimer <= 0f)
				{
					this.aiUpdateTimer = BaseAIState.cvAIUpdateInterval.value;
					this.UpdateAI(BaseAIState.cvAIUpdateInterval.value);
					if (!base.body.HasBuff(SS2Content.Buffs.BuffFear))
					{
						this.outer.SetNextState(new LookBusy()); // MIGHT NEED TO GO BACK TO COMBAT INSTEAD.
					}
				}
			}
		}
		protected void UpdateFootPosition()
		{
			this.myBodyFootPosition = base.body.footPosition;
			BroadNavigationSystem.Agent broadNavigationAgent = base.ai.broadNavigationAgent;
			broadNavigationAgent.currentPosition = new Vector3?(this.myBodyFootPosition);
		}

		protected void UpdateAI(float deltaTime)
		{
			BaseAI.SkillDriverEvaluation skillDriverEvaluation = base.ai.skillDriverEvaluation;
			this.bodyInputs.moveVector = Vector3.zero;
			float d = 1f;
			if (!base.body || !base.bodyInputBank)
			{
				return;
			}
			Vector3 bodyPosition = base.bodyTransform.position;
			BroadNavigationSystem.Agent broadNavigationAgent = base.ai.broadNavigationAgent;
			BroadNavigationSystem.AgentOutput output = broadNavigationAgent.output;
			GameObject target = this.fearTarget ? this.fearTarget : (skillDriverEvaluation.target != null ? skillDriverEvaluation.target.gameObject : null);
			if ((target != null) ? target.gameObject : null)
			{
				Vector3 fearTargetPosition = target.transform.position;
				
				Vector3 goalPositoin = bodyPosition;
				Vector3 vector4 = (fearTargetPosition - this.myBodyFootPosition).normalized * 10f;

				goalPositoin -= vector4;

				if (this.fallbackNodeStartAge + this.fallbackNodeDuration < base.fixedAge) //dunno wat this does
				{
					base.ai.SetGoalPosition(goalPositoin);
				}

				base.ai.localNavigator.targetPosition = goalPositoin;
				base.ai.localNavigator.allowWalkOffCliff = true;
				base.ai.localNavigator.Update(deltaTime);

				this.bodyInputs.moveVector = base.ai.localNavigator.moveVector;
				this.bodyInputs.moveVector *= d;
			}
			if (output.lastPathUpdate > this.lastPathUpdate && !output.targetReachable && this.fallbackNodeStartAge + this.fallbackNodeDuration < base.fixedAge)
			{
				broadNavigationAgent.goalPosition = base.PickRandomNearbyReachablePosition();
				broadNavigationAgent.InvalidatePath();
			}
			this.lastPathUpdate = output.lastPathUpdate;
		}

		public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
		{
			this.bodyInputs.pressSkill1 = false;
			this.bodyInputs.pressSkill2 = false;
			this.bodyInputs.pressSkill3 = false;
			this.bodyInputs.pressSkill4 = false;
			this.bodyInputs.pressSprint = false;
			this.bodyInputs.pressActivateEquipment = false;
			this.bodyInputs.desiredAimDirection = Vector3.zero;
			BaseAI.Target aimTarget = base.ai.skillDriverEvaluation.aimTarget;
			if (aimTarget != null)
			{
				base.AimAt(ref this.bodyInputs, aimTarget);
			}
			base.ModifyInputsForJumpIfNeccessary(ref this.bodyInputs);
			return this.bodyInputs;
		}

		
	}
}
