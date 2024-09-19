using System.Collections.Generic;
using RoR2;
using UnityEngine;
using SS2;

namespace EntityStates.FlowerTurret
{
    public abstract class BaseFlowerTurretState : EntityState
	{
		protected NetworkedBodyAttachment networkedBodyAttachment;
		protected CharacterBody body;
		protected Animator animator;
		protected Transform muzzleTransform;
		protected Transform displayTransform;
		protected bool linkedToDisplay;

		public override void OnEnter()
		{
			base.OnEnter();
			this.networkedBodyAttachment = base.GetComponent<NetworkedBodyAttachment>();
			if (this.networkedBodyAttachment)
			{
				this.body = this.networkedBodyAttachment.attachedBody;
			}
			this.muzzleTransform = FindMuzzle();
		}

		private Transform FindMuzzle()
		{
			if (this.networkedBodyAttachment)
			{
				this.body = this.networkedBodyAttachment.attachedBody;
				if (this.body && this.body.modelLocator && this.body.modelLocator.modelTransform)
				{
					CharacterModel characterModel = this.body.modelLocator.modelTransform.GetComponent<CharacterModel>();
					if (characterModel)
					{
						List<GameObject> itemDisplayObjects = characterModel.GetItemDisplayObjects(SS2Content.Items.FlowerTurret.itemIndex);
						foreach (GameObject gameObject in itemDisplayObjects)
						{
							displayTransform = gameObject.transform;
							this.animator = gameObject.GetComponentInChildren<Animator>();
							ChildLocator childLocator = gameObject.GetComponentInChildren<ChildLocator>();
							if (childLocator)
							{
								Transform muzzle = childLocator.FindChild("Muzzle");
								if(muzzle)
									return muzzle;
							}
						}
					}

				}
			}
			return base.transform;
		}
	}
}
