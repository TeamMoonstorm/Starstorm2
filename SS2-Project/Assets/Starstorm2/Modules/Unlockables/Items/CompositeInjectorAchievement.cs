using System;
using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Pickups
{
	public class CompositeInjectorAchievement : BaseAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			UserProfile userProfile = base.userProfile;
			userProfile.onPickupDiscovered = (Action<PickupIndex>)Delegate.Combine(userProfile.onPickupDiscovered, new Action<PickupIndex>(this.OnPickupDiscovered));
			this.Check();
		}
		public override void OnUninstall()
		{
			UserProfile userProfile = base.userProfile;
			userProfile.onPickupDiscovered = (Action<PickupIndex>)Delegate.Remove(userProfile.onPickupDiscovered, new Action<PickupIndex>(this.OnPickupDiscovered));
			base.OnUninstall();
		}
		public override float ProgressForAchievement()
		{
			return (float)this.EquipmentDiscovered() / 15f;
		}
		private void OnPickupDiscovered(PickupIndex pickupIndex)
		{
			PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
			if (((pickupDef != null) ? pickupDef.equipmentIndex : EquipmentIndex.None) != EquipmentIndex.None)
			{
				this.Check();
			}
		}
		private int EquipmentDiscovered()
		{
			int num = 0;
			EquipmentIndex equipmentIndex = (EquipmentIndex)0;
			EquipmentIndex equipmentCount = (EquipmentIndex)EquipmentCatalog.equipmentCount;
			while (equipmentIndex < equipmentCount)
			{
				if (base.userProfile.HasDiscoveredPickup(PickupCatalog.FindPickupIndex(equipmentIndex)))
				{
					num++;
				}
				equipmentIndex++;
			}
			return num;
		}
		private void Check()
		{
			if (this.EquipmentDiscovered() >= 15)
			{
				base.Grant();
			}
		}
	}
}
