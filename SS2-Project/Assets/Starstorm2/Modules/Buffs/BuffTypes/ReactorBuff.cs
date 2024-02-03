using Moonstorm.Components;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ReactorBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffReactor", SS2Bundle.Items);
        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matReactorBuffOverlay", SS2Bundle.Items);

        //To-Do: Maybe better invincibility implementation. Projectile deflection for cool points?
        public sealed class Behavior : BaseBuffBodyBehavior, RoR2.IOnIncomingDamageServerReceiver, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffReactor;
            private GameObject exhaust;

            private void Start()
            {
                var model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();

                if (model != null)
                {
                    var displayList = model.GetItemDisplayObjects(SS2Content.Items.PortableReactor.itemIndex);

                    if (displayList != null)
                    {
                        var displayObject = displayList[0];
                        if (displayObject != null)
                        {
                            var displayCL = displayObject.GetComponent<ChildLocator>();
                            if (displayCL != null)
                            {
                                exhaust = displayCL.FindChild("exhaust").gameObject;
                            }
                        }
                    }
                }

                if (exhaust != null)
                    exhaust.SetActive(true);
            }

            private void OnDestroy()
            {
                if (exhaust != null)
                    exhaust.SetActive(false);
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedMultAdd += 0.5f;
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.damageType != DamageType.VoidDeath)
                    damageInfo.rejected = true;
            }
        }
    }
}
