using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
	public class BloodTesterAnimator : MonoBehaviour
	{
		public ParticleSystem healEffectSystem;
		public ParticleSystem stageUpEffectSystem;
		private int currentStage;
		private CharacterModel characterModel;

		private void Start()
		{
			this.characterModel = base.GetComponentInParent<CharacterModel>();
		}

		private void FixedUpdate()
		{
			CharacterMaster master = this.characterModel?.body?.master;
			if (master && Stage.instance)
			{
				// :3
				if (master.money > Run.instance.GetDifficultyScaledCost(75, Stage.instance.entryDifficultyCoefficient)) SetStage(3);
				else if (master.money > Run.instance.GetDifficultyScaledCost(50, Stage.instance.entryDifficultyCoefficient)) SetStage(2);
				else if (master.money > Run.instance.GetDifficultyScaledCost(25, Stage.instance.entryDifficultyCoefficient)) SetStage(1);
				else SetStage(0);
			}
		}
		private void SetStage(int i)
        {
			// SOUND WOULD BE EPIC!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			if (currentStage == i) return;
			//if (i > currentStage) stageUpEffectSystem.Play();
			currentStage = i;
			ParticleSystem.EmissionModule emission = healEffectSystem.emission;
			switch (currentStage)
            {
				case 0:
					emission.rateOverTimeMultiplier = 0;
					break;
                case 1:
					emission.rateOverTimeMultiplier = 1;
					break;
				case 2:
					emission.rateOverTimeMultiplier = 5;
					break;
				case 3:
					emission.rateOverTimeMultiplier = 12;
					break;

            }
        }

		
	}
}
