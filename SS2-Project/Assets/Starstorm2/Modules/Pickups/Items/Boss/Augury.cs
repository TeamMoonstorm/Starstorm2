using EntityStates.Pickups.Augury;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class Augury : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Augury", SS2Bundle.Indev);

        public sealed class Behavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Augury;
            private NetworkedBodyAttachment attachment;
            private EntityStateMachine esm;
            private void Start()
            {
                attachment = Instantiate(SS2Assets.LoadAsset<GameObject>($"{nameof(Augury)}BodyAttachment", SS2Bundle.Indev)).GetComponent<NetworkedBodyAttachment>();
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