using EntityStates.Pickups.Augury;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if DEBUG
namespace SS2.Items
{
    public sealed class Augury : SS2Item
    {
        private static GameObject _attachment;

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acAugury", SS2Bundle.Indev);

        public override void Initialize()
        {
            _attachment = AssetCollection.FindAsset<GameObject>("AuguryBodyAttachment");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false; //TODO: MSUtil.IsModInstalled("Enforcer GUID goes here... also check if nemforcer is enabled or something."); 
        }

        public sealed class AuguryBehavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Augury;
            private NetworkedBodyAttachment attachment;
            private EntityStateMachine esm;
            private void Start()
            {
                attachment = Instantiate(_attachment).GetComponent<NetworkedBodyAttachment>();
                attachment.AttachToGameObjectAndSpawn(body.gameObject);
                esm = attachment.gameObject.GetComponent<EntityStateMachine>();

                HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onTakeDamageReceivers, this);
            }

            private void FixedUpdate()
            {
                if (!body.healthComponent.alive)
                {
                    Destroy(this);
                }
            }

            private void OnDestroy()
            {
                if (attachment)
                {
                    Destroy(attachment.gameObject);
                    attachment = null;
                }

                int i = Array.IndexOf(body.healthComponent.onTakeDamageReceivers, this);
                if (i > -1)
                {
                    HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onTakeDamageReceivers, body.healthComponent.onTakeDamageReceivers.Length, i);
                }
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (esm.state is AuguryIdle idle)
                {
                    idle.OnTakeDamage(damageReport);
                }
            }
        }
    }
}
#endif