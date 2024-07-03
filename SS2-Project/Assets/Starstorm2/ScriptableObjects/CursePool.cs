using System;
using System.Collections.Generic;
using RoR2.Items;
using UnityEngine;
using RoR2;

namespace SS2.ScriptableObjects
{
	public abstract class CursePool : ScriptableObject
	{
		public struct Curse
        {
			public Material iconMaterial;
			public int count;
        }

		protected static PickupIndex[] GenerateUniqueDropsFromWeightedSelection(int maxDrops, Xoroshiro128Plus rng, WeightedSelection<PickupIndex> weightedSelection)
		{
			int num = Math.Min(maxDrops, weightedSelection.Count);
			int[] array = Array.Empty<int>();
			PickupIndex[] array2 = new PickupIndex[num];
			for (int i = 0; i < num; i++)
			{
				int num2 = weightedSelection.EvaluateToChoiceIndex(rng.nextNormalizedFloat, array);
				WeightedSelection<PickupIndex>.ChoiceInfo choice = weightedSelection.GetChoice(num2);
				array2[i] = choice.value;
				Array.Resize<int>(ref array, i + 1);
				array[i] = num2;
			}
			return array2;
		}

		protected static PickupIndex GenerateDropFromWeightedSelection(Xoroshiro128Plus rng, WeightedSelection<PickupIndex> weightedSelection)
		{
			if (weightedSelection.Count > 0)
			{
				return weightedSelection.Evaluate(rng.nextNormalizedFloat);
			}
			return PickupIndex.none;
		}
		protected abstract PickupIndex GenerateDropPreReplacement(Xoroshiro128Plus rng);

		public PickupIndex GenerateDrop(Xoroshiro128Plus rng)
		{
			PickupIndex pickupIndex = this.GenerateDropPreReplacement(rng);
			if (pickupIndex == PickupIndex.none)
			{
				Debug.LogError("Could not generate pickup index from droptable.");
			}
			if (!pickupIndex.isValid)
			{
				Debug.LogError("Pickup index from droptable is invalid.");
			}
			return pickupIndex;
		}

		protected abstract PickupIndex[] GenerateUniqueDropsPreReplacement(int maxDrops, Xoroshiro128Plus rng);
		public PickupIndex[] GenerateUniqueDrops(int maxDrops, Xoroshiro128Plus rng)
		{
			PickupIndex[] array = this.GenerateUniqueDropsPreReplacement(maxDrops, rng);
			return array;
		}

		protected virtual void Regenerate(Run run)
		{
		}

		protected virtual void OnEnable()
		{
			CursePool.instancesList.Add(this);
			if (Run.instance)
			{
				Debug.Log("PickupDropTable '" + base.name + "' has been loaded after the Run started.  This might be an issue with asset duplication across bundles, or it might be fine.  Regenerating...");
				this.Regenerate(Run.instance);
			}
		}
		protected virtual void OnDisable()
		{
			CursePool.instancesList.Remove(this);
		}

		static CursePool()
		{
			Run.onRunStartGlobal += CursePool.RegenerateAll;
			Run.onAvailablePickupsModified += CursePool.RegenerateAll;
		}

		private static void RegenerateAll(Run run)
		{
			for (int i = 0; i < CursePool.instancesList.Count; i++)
			{
				CursePool.instancesList[i].Regenerate(run);
			}
		}

		public void ModifyTierWeights(float tier1, float tier2, float tier3)
		{
		}

		public bool canDropBeReplaced = true;

		private static readonly List<CursePool> instancesList = new List<CursePool>();
	}
}
