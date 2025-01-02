using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
namespace SS2
{
	[CreateAssetMenu(fileName = "BossDropTable", menuName = "Starstorm2/DropTables/BossDropTable")]
	public class BossDropTable : PickupDropTable
	{
		private void Add(List<PickupIndex> sourceDropList, float chance)
		{
			if (chance <= 0f || sourceDropList.Count == 0)
			{
				return;
			}
			foreach (PickupIndex pickupIndex in sourceDropList)
			{
				this.selector.AddChoice(pickupIndex, chance);				
			}
		}
		public override PickupIndex GenerateDropPreReplacement(Xoroshiro128Plus rng)
		{
			int itemCount = SS2Util.GetItemCountForPlayers(SS2Content.Items.ItemOnBossKill);
			this.selector.Clear();
			this.Add(Run.instance.availableTier1DropList, this.tier1Weight);
			this.Add(Run.instance.availableTier2DropList, this.tier2Weight + (0.1f * itemCount));
			this.Add(Run.instance.availableTier3DropList, this.tier3Weight + (0.1f * itemCount));
			return PickupDropTable.GenerateDropFromWeightedSelection(rng, this.selector);
		}

		public override int GetPickupCount()
		{
			return this.selector.Count;
		}

		public override PickupIndex[] GenerateUniqueDropsPreReplacement(int maxDrops, Xoroshiro128Plus rng)
		{
			return PickupDropTable.GenerateUniqueDropsFromWeightedSelection(maxDrops, rng, this.selector);
		}

		public float tier1Weight = 0.4f;

		public float tier2Weight = 0.5f;

		public float tier3Weight = 0.1f;

		private readonly WeightedSelection<PickupIndex> selector = new WeightedSelection<PickupIndex>(8);
	}
}
