using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
	public class BloodTesterAnimator : MonoBehaviour
	{
		public ParticleSystem healEffectSystem;
		private int currentStage;
		private CharacterModel characterModel;

		private void Start()
		{
			this.characterModel = base.GetComponentInParent<CharacterModel>();
		}

		private void FixedUpdate()
		{
			CharacterMaster master = this.characterModel?.body?.master;
			if (master)
			{
				// :3
				if (master.money > Run.instance.GetDifficultyScaledCost(75, Run.instance.difficultyCoefficient)) SetStage(3);
				else if (master.money > Run.instance.GetDifficultyScaledCost(50, Run.instance.difficultyCoefficient)) SetStage(2);
				else if (master.money > Run.instance.GetDifficultyScaledCost(25, Run.instance.difficultyCoefficient)) SetStage(1);
				else SetStage(0);
			}
		}
		private void SetStage(int i)
        {
			if (currentStage == i) return;
			currentStage = i;
			ParticleSystem.EmissionModule emission = healEffectSystem.emission;
			switch (currentStage)
            {
				case 0:
					emission.rateOverTime = 0;
					break;
                case 1:
					emission.rateOverTime = 1;
					break;
				case 2:
					emission.rateOverTime = 5;
					break;
				case 3:
					emission.rateOverTime = 12;
					break;

            }
        }

		
	}
}
