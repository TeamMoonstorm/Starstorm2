using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;
namespace SS2.UI
{
	public class InspectInfoPanelHelper : MonoBehaviour
	{
		[SerializeField]
		public InspectPanelController inspectPanelController;
		private MPEventSystem eventSystem;
		public bool forceShow;
		public bool withSidecar;
		private void Awake()
		{
			MPEventSystemLocator component = base.GetComponent<MPEventSystemLocator>();
			this.eventSystem = component.eventSystem;
		}

		private void Update()
		{
			if (this.eventSystem.player.GetButtonDown(15))
			{
				Destroy(base.gameObject);
			}
		}
		public void ShowInfo(MPButton button, PickupDef pickupDef)
		{
			InspectInfo info = pickupDef;
			info.MarkForceShowInfo();
			this.inspectPanelController.Show(info, withSidecar);
		}

		

	}
}
