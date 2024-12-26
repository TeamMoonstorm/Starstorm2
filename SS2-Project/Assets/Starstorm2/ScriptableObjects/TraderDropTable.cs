using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
namespace SS2
{
	[CreateAssetMenu(fileName = "TraderDropTable", menuName = "Starstorm2/DropTables/TraderDropTable")]
	public class TraderDropTable : PickupDropTable
	{
		//can drop:
		//nothing
		//tier1, 2, 3
		//boss
		//shard
		// "value" increases the rarity VVVV
		// "nothing" reserved for very low value
		[Tooltip("Final drop table weights will be an interpolation between two breakpoints, based on value. Must be ordered by increasing requiredValue.")]
		public ValueBreakpoint[] valueBreakpoints;
		public PickupIndex[] GenerateDrops(int count, float tradeValue, Xoroshiro128Plus rng)
        {
			if(valueBreakpoints.Length == 0)
            {
				SS2Log.Error(base.name + " has zero value breakpoints");
				return null;
            }
			ValueBreakpoint lower = valueBreakpoints[0];
			ValueBreakpoint upper = valueBreakpoints[valueBreakpoints.Length - 1];
			if(valueBreakpoints.Length > 2)
            {
				for (int i = valueBreakpoints.Length - 1; i >= 0; i--)
				{
					if (tradeValue > valueBreakpoints[i].requiredValue)
					{
						lower = valueBreakpoints[i];
						if (i + 1 >= valueBreakpoints.Length) upper = lower;
						else upper = valueBreakpoints[i + 1];

						break;
					}
				}
			}		
			float t = tradeValue - lower.requiredValue;
			float tMax = upper.requiredValue - lower.requiredValue;
			ValueBreakpoint state = ValueBreakpoint.Lerp(ref lower, ref upper, t / tMax);
			this.selector.Clear();
			this.selector.AddChoice(ItemTier.NoTier, state.nothing); // using itemtier is pointless but it makes my brain happy
			this.selector.AddChoice(ItemTier.Tier1, state.tier1);
			this.selector.AddChoice(ItemTier.Tier2, state.tier2);
			this.selector.AddChoice(ItemTier.Tier3, state.tier3);
			this.selector.AddChoice(ItemTier.Boss, state.boss);
			this.selector.AddChoice(SS2Content.ItemTierDefs.Curio.tier, state.shard);
			PickupIndex[] drops = new PickupIndex[count];
			for(int i = 0; i < count; i++)
            {
				PickupIndex drop = PickupIndex.none;
				ItemTier tier = this.selector.Evaluate(rng.nextNormalizedFloat);
				switch (tier)
                {
					case ItemTier.NoTier: drop = PickupIndex.none; break;
					case ItemTier.Tier1: drop = rng.NextElementUniform(Run.instance.availableTier1DropList); break;
					case ItemTier.Tier2: drop = rng.NextElementUniform(Run.instance.availableTier2DropList); break;
					case ItemTier.Tier3: drop = rng.NextElementUniform(Run.instance.availableTier3DropList); break;
					case ItemTier.Boss: drop = rng.NextElementUniform(Run.instance.availableBossDropList); break;
					default: 
						if(tier == SS2Content.ItemTierDefs.Curio.tier) 
							drop = PickupCatalog.FindPickupIndex(SS2Content.Items.ShardScav.itemIndex); 
						break;
				}
				drops[i] = drop;
            }
			return drops;
		}
		public override PickupIndex GenerateDropPreReplacement(Xoroshiro128Plus rng)
		{
			return default(PickupIndex);
		}

		public override int GetPickupCount()
		{
			return this.selector.Count;
		}

		public override PickupIndex[] GenerateUniqueDropsPreReplacement(int maxDrops, Xoroshiro128Plus rng)
		{
			return null;
		}

		[Serializable]
		public struct ValueBreakpoint : IEquatable<ValueBreakpoint>
		{
			public float requiredValue;
			[Header("Weights")]			
			public float tier1;
			public float tier2;
			public float tier3;
			public float boss;
			public float nothing;
			public float shard;

			public static ValueBreakpoint Lerp(ref ValueBreakpoint a, ref ValueBreakpoint b, float t)
			{
				if (a.Equals(b)) return a;
				return new ValueBreakpoint
				{
					tier1 = Mathf.LerpUnclamped(a.tier1, b.tier1, t),
					tier2 = Mathf.LerpUnclamped(a.tier2, b.tier2, t),
					tier3 = Mathf.LerpUnclamped(a.tier3, b.tier3, t),
					boss = Mathf.LerpUnclamped(a.boss, b.boss, t),
					shard = Mathf.LerpUnclamped(a.shard, b.shard, t),
				};
			}
			public bool Equals(ValueBreakpoint other)
			{
				return requiredValue == other.requiredValue
					&& tier1 == other.tier1
					&& tier2 == other.tier2
					&& tier3 == other.tier3
					&& boss == other.boss
					&& shard == other.shard;
			}
		}
		private readonly WeightedSelection<ItemTier> selector = new WeightedSelection<ItemTier>(8);
	}
}
