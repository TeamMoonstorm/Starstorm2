using EntityStates;
using MSU;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using RoR2.ContentManagement;
#if DEBUG
namespace SS2.Artifacts
{
    public sealed class Deviation : SS2Artifact
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ArtifactAssetCollection>("acDeviation", SS2Bundle.Artifacts);

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        private void DeviationOverride(On.RoR2.TeleporterInteraction.IdleState.orig_OnInteractionBegin orig, BaseState self, Interactor activator)
        {
            SS2Log.Info(self.outer.gameObject.name);
            var tc = self.outer.gameObject.GetComponent<TestComponent>();
            if (!tc)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "Yummy eating input"
                });
                self.outer.gameObject.AddComponent<TestComponent>();
            }
            else
            {
                orig(self, activator);
            }
        }

        public override void OnArtifactEnabled()
        {
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin += DeviationOverride;
        }

        public override void OnArtifactDisabled()
        {
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin -= DeviationOverride;

        }
    }

    class DeviationState : TeleporterInteraction.BaseTeleporterState
    {
        public override TeleporterInteraction.ActivationState backwardsCompatibleActivationState
        {
            get
            {
                return TeleporterInteraction.ActivationState.Charged;
            }
        }

        private CombatDirector bonusDirector
        {
            get
            {
                return base.teleporterInteraction.bonusDirector;
            }
        }

        private CombatDirector bossDirector
        {
            get
            {
                return base.teleporterInteraction.bossDirector;
            }
        }
    }


    public class TestComponent : MonoBehaviour
    {
        //i swear this makes sense
    }

}
#endif
