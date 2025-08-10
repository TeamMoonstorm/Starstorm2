using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2;
using RoR2.UI;
using SS2.UI;
using System;
namespace SS2.Components
{
    public class PickupCarouselController : NetworkBehaviour // TODO : display above player head for other players
    {
		public Interactor interactor;
		public GameObject panelPrefab;
		private GameObject panelInstance;
		private PickupCarouselPanel panelInstanceController;
		private NetworkUIPromptController networkUIPromptController;
        public PickupIndex[] options;
        public uint chosenRewardIndex;
        void Awake()
        {
			networkUIPromptController = base.GetComponent<NetworkUIPromptController>();
			if (NetworkClient.active)
			{
				this.networkUIPromptController.onDisplayBegin += this.OnDisplayBegin;
				this.networkUIPromptController.onDisplayEnd += this.OnDisplayEnd;
			}		
			if(NetworkServer.active)
            {
				networkUIPromptController.messageFromClientHandler += HandleClientMessage;
				
			}
        }
        private void Start()
        {
            if(NetworkServer.active)
				networkUIPromptController.SetParticipantMasterFromInteractor(interactor);
		}
        private void OnDisplayBegin(NetworkUIPromptController controller, LocalUser user, CameraRigController camera)
        {
			this.panelInstance = Instantiate<GameObject>(this.panelPrefab, camera.hud.mainContainer.transform);
			this.panelInstanceController = this.panelInstance.GetComponent<PickupCarouselPanel>();
			this.panelInstanceController.controller = this;
			this.panelInstanceController.SetOptions(this.options, (int)chosenRewardIndex);
			OnDestroyCallback.AddCallback(this.panelInstance, new Action<OnDestroyCallback>(this.OnPanelDestroyed));
			if (user.cachedBody) user.cachedBody.hideCrosshair = true;			
		}

		private void OnDisplayEnd(NetworkUIPromptController controller, LocalUser user, CameraRigController camera)
		{
			Destroy(this.panelInstance);
			Destroy(base.gameObject);
			if(user.cachedBody) user.cachedBody.hideCrosshair = false;
			this.panelInstance = null;
			this.panelInstanceController = null;
		}

		private void OnPanelDestroyed(OnDestroyCallback onDestroyCallback)
		{
			NetworkWriter networkWriter = this.networkUIPromptController.BeginMessageToServer();
			this.networkUIPromptController.FinishMessageToServer(networkWriter);
		}
		private void HandleClientMessage(NetworkReader reader)
		{
			this.networkUIPromptController.SetParticipantMaster(null);
		}

		public override bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			writer.WritePackedUInt32(chosenRewardIndex);
			writer.WritePackedUInt32((uint)options.Length);
			for(int i = 0; i < options.Length; i++)
            {
				writer.Write(options[i]);
            }
			return initialState;
		}
		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			chosenRewardIndex = reader.ReadPackedUInt32();
			uint count = reader.ReadPackedUInt32();
			options = new PickupIndex[count];
			for(int i = 0; i < count; i++)
            {
				options[i] = reader.ReadPickupIndex();
            }
		}
	}
}
