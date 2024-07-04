using System;
using System.Collections.Generic;
using RoR2.Items;
using UnityEngine;
using RoR2;
using static SS2.CurseManager;
namespace SS2
{
	// pickupdroptable mostly. but it removes the choice when selected

	[CreateAssetMenu(fileName = "CursePool", menuName = "Starstorm2/CursePool")]
	public class CursePool : ScriptableObject
	{
		private static readonly List<Curse> allCurses = new List<Curse>(); //ughhhhhh why is this in cursepool
		[Serializable]
		public struct Curse
        {
			public CurseIndex curseIndex;
			public Material iconMaterial; // bleh
			public int count;
			public float weight;
        }
		public Curse[] curses;

		private bool init;

		private readonly WeightedSelection<CurseIndex> selector = new WeightedSelection<CurseIndex>(21);

        private void Awake()
        {
			if(Application.isEditor && !init)
            {
				init = true;
				curses = new Curse[20]; // should iterate thru curseindex enum instead
				for(int i = 1; i < curses.Length; i++)
                {
					curses[i] = new Curse { curseIndex = (CurseIndex)i, count = 1, weight = 1 };
                }
            }
		}

		// bad planning by me. should have some sort of cursecatalog or cursedef for this
		public static Material GetCurseMaterial(CurseIndex index)
        {
			for(int i = 0; i < allCurses.Count; i++)
            {
				if (allCurses[i].curseIndex == index) return allCurses[i].iconMaterial;
            }
			return null;
        }

        protected static CurseIndex[] GenerateUniqueCursesFromWeightedSelection(int maxDrops, Xoroshiro128Plus rng, WeightedSelection<CurseIndex> weightedSelection)
		{
			int num = Math.Min(maxDrops, weightedSelection.Count);
			int[] array = Array.Empty<int>();
			CurseIndex[] array2 = new CurseIndex[num];
			for (int i = 0; i < num; i++)
			{
				int choiceIndex = weightedSelection.EvaluateToChoiceIndex(rng.nextNormalizedFloat, array);				
				WeightedSelection<CurseIndex>.ChoiceInfo choice = weightedSelection.GetChoice(choiceIndex);
				weightedSelection.RemoveChoice(choiceIndex);
				array2[i] = choice.value;
				Array.Resize<int>(ref array, i + 1);
				array[i] = choiceIndex;
			}
			return array2;
		}

		protected static CurseIndex GenerateCurseFromWeightedSelection(Xoroshiro128Plus rng, WeightedSelection<CurseIndex> weightedSelection)
		{
			if (weightedSelection.Count > 0)
			{
				int choiceIndex = weightedSelection.EvaluateToChoiceIndex(rng.nextNormalizedFloat);
				CurseIndex curseIndex = weightedSelection.choices[choiceIndex].value;
				weightedSelection.RemoveChoice(choiceIndex);
				return curseIndex;
			}
			return CurseIndex.None;
		}

		public CurseIndex GenerateCurse(Xoroshiro128Plus rng)
		{
			CurseIndex pickupIndex = GenerateCurseFromWeightedSelection(rng, this.selector);
			if (pickupIndex == CurseIndex.None)
			{
				SS2Log.Error("Could not generate curse index from CursePool.");
			}
			return pickupIndex;
		}

		public CurseIndex[] GenerateUniqueCurses(int maxDrops, Xoroshiro128Plus rng)
		{
			CurseIndex[] array = GenerateUniqueCursesFromWeightedSelection(maxDrops, rng, this.selector);
			return array;
		}

		public bool IsEmpty()
        {
			return this.selector.Count == 0;
        }

		protected void Regenerate(Run run)
		{
			this.selector.Clear();
			foreach(Curse curse in this.curses)
            {
				for (int i = 0; i < curse.count; i++)
					this.selector.AddChoice(curse.curseIndex, curse.weight);

				if (!allCurses.Contains(curse)) allCurses.Add(curse); // sry
            }
		}

		protected virtual void OnEnable()
		{
			CursePool.instancesList.Add(this);
			if (Run.instance)
			{
				SS2Log.Info("CursePool '" + base.name + "' has been loaded after the Run started.  This might be an issue with asset duplication across bundles, or it might be fine.  Regenerating...");
				this.Regenerate(Run.instance);
			}
		}
		protected virtual void OnDisable()
		{
			CursePool.instancesList.Remove(this);
		}

		static CursePool()
		{
			CurseManager.onCursesRefreshed += CursePool.RegenerateAll;
		}

		private static void RegenerateAll(Run run)
		{
			for (int i = 0; i < CursePool.instancesList.Count; i++)
			{
				CursePool.instancesList[i].Regenerate(run);
			}
		}

		private static readonly List<CursePool> instancesList = new List<CursePool>();
	}
}
