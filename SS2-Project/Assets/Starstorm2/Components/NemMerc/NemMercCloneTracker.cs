using System;
using UnityEngine;
namespace SS2.Components
{
    public class NemMercCloneTracker : NemMercTracker // this should absolutely just be in NemMercTracker but i wanted to see how inhereting monobehaviors worked
	{
		public override GameObject GetTrackingTarget()
		{
			return this.ownerTracker && !(this.ownerTracker is NemMercCloneTracker) ? this.ownerTracker.GetTrackingTarget() : this.trackingTarget;
		}
		public override bool IsTargetHologram()
		{
			return GetTrackingTarget() && this.ownerTracker ? this.ownerTracker.targetIsHologram : this.trackingTarget;
		}
		[NonSerialized]
		public NemMercTracker ownerTracker;
	}
}
