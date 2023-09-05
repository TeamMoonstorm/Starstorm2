using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
	public class NemMercCloneTracker : NemMercTracker
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
