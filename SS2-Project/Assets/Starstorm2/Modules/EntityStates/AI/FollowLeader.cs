using System;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using UnityEngine;
using Moonstorm.Starstorm2;
namespace EntityStates.AI.Walker
{
	// 
	public class FollowLeader : BaseAIState
	{
		public GameObject leader;

		private float aiUpdateTimer;

		protected Vector3 myBodyFootPosition;

		private float lastPathUpdate;

		private float fallbackNodeStartAge;
		private readonly float fallbackNodeDuration = 4f;
		public static float hardTrailDistance = 16f; // beyond this, path directly to leader. below this, path to targetTrailPosition
		public static float minTrailDistance = 8f; // minimum distance from leader for pathing
		public static float trailHeight = 10f;
		public static float targetReachedDistance = 5f;


		private Vector3 targetTrailPosition;
		public override void OnEnter()
		{
			base.OnEnter();
			if (base.ai)
			{
				this.lastPathUpdate = base.ai.broadNavigationAgent.output.lastPathUpdate;
				base.ai.broadNavigationAgent.InvalidatePath();
			}
			this.targetTrailPosition = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(minTrailDistance, hardTrailDistance);

			if (!this.leader && this.ai.master.minionOwnership.ownerMaster)
				this.leader = this.ai.master.minionOwnership.ownerMaster.GetBodyObject();
			this.fallbackNodeStartAge = float.NegativeInfinity;

			this.ai.currentEnemy.Reset();

            GlobalEventManager.onServerDamageDealt += CopyLeaderTarget;
		}
        public override void OnExit()
        {
            base.OnExit();
			GlobalEventManager.onServerDamageDealt -= CopyLeaderTarget;
		}

        private void CopyLeaderTarget(DamageReport damageReport)
        {
            if(damageReport.attacker == this.leader)
            {
				this.ai.currentEnemy.gameObject = damageReport.victim.gameObject;
				this.ai.currentEnemy.bestHurtBox = damageReport.victimBody.mainHurtBox;
				//this.outer.SetNextState(new Combat());
            }
        }

        // jank. hopefully only true when the skilldriver is a "combat" skill
        private bool IsCombatDriver(AISkillDriver driver)
        {
			if (!driver) return false;
			//if (driver.maxDistance == Mathf.Infinity) return false; // ?
			return driver.skillSlot != SkillSlot.None;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.ai && base.body)
			{
				
				if (IsCombatDriver(base.ai.skillDriverEvaluation.dominantSkillDriver))
				{
					this.outer.SetNextState(new Combat());
				}

				this.aiUpdateTimer -= Time.fixedDeltaTime;
				this.UpdateFootPosition();
				if (this.aiUpdateTimer <= 0f)
				{
					this.aiUpdateTimer = BaseAIState.cvAIUpdateInterval.value;
					this.UpdateAI(BaseAIState.cvAIUpdateInterval.value);
					
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
			//GameObject target = this.leader ? this.leader : (skillDriverEvaluation.target != null ? skillDriverEvaluation.target.gameObject : null);
			if (this.leader)
			{
				Vector3 goalPosition = this.leader.transform.position;

				float distanceBetween = (this.leader.transform.position - bodyPosition).magnitude;
				if(distanceBetween <= hardTrailDistance)
                {
					goalPosition += this.targetTrailPosition;
				}

				bool targetReached = (goalPosition - bodyPosition).magnitude < targetReachedDistance;

				bool bodyIsFlier = this.body.isFlying || !this.body.characterMotor;
				if (bodyIsFlier)
                {
					goalPosition.y += trailHeight;
                }


				if (this.fallbackNodeStartAge + this.fallbackNodeDuration < base.fixedAge) //dunno wat this does
				{
					base.ai.SetGoalPosition(goalPosition);
				}

				Vector3 targetPosition = output.nextPosition ?? this.myBodyFootPosition;

				// jank idc
				base.ai.localNavigator.targetPosition = targetPosition;//!targetReached ? goalPosition : bodyPosition;
				base.ai.localNavigator.allowWalkOffCliff = true;
				base.ai.localNavigator.Update(deltaTime);

				this.bodyInputs.moveVector = !targetReached ? base.ai.localNavigator.moveVector : Vector3.zero;
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
			//BaseAI.Target aimTarget = base.ai.skillDriverEvaluation.aimTarget;
			//if (aimTarget != null)
			//{
			//	base.AimAt(ref this.bodyInputs, aimTarget);
			//}
			//else
			this.bodyInputs.desiredAimDirection = this.bodyInputs.moveVector;
			base.ModifyInputsForJumpIfNeccessary(ref this.bodyInputs);
			return this.bodyInputs;
		}


	}
}
