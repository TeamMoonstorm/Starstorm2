using RoR2;
using UnityEngine;
namespace SS2.Components
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
			CharacterBody body = this.characterModel?.body;
			if (body)
			{
				SetStage(body.HasBuff(SS2Content.Buffs.BuffBloodTesterRegen) ? 1 : 0);
			}
		}
		private void SetStage(int i)
        {
			if (currentStage == i) return;
			if (i > currentStage) stageUpEffectSystem.Play();
			currentStage = i;
			ParticleSystem.EmissionModule emission = healEffectSystem.emission;
			switch (currentStage)
            {
				case 0:
					emission.rateOverTimeMultiplier = 0;
					break;
                case 1:
					emission.rateOverTimeMultiplier = 10;
					break;

            }
        }

		
	}
}
