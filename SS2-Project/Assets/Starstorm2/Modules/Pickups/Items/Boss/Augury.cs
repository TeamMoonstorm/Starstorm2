using EntityStates.Pickups.Augury;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System;
using System.Collections;
using UnityEngine;
#if DEBUG
namespace SS2.Items
{
    public sealed class Augury : SS2Item
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;
        private static GameObject _attachment;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false; //MSUtil.IsModInstalled("Enforcer GUID goes here... also check if nemforcer is enabled or something."); 
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<GameObject>("AuguryBodyAttachment", SS2Bundle.Indev);
            helper.AddAssetToLoad<ItemDef>("Augury", SS2Bundle.Items);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _itemDef = helper.GetLoadedAsset<ItemDef>("Augury");
            _attachment = helper.GetLoadedAsset<GameObject>("AuguryBodyAttachment");
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