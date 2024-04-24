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
        public override NullableRef<ArtifactCode> ArtifactCode => _artifactCode;
        private ArtifactCode _artifactCode;
        public override ArtifactDef ArtifactDef => _artifactDef;
        private ArtifactDef _artifactDef;
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();
            
            helper.AddAssetToLoad<ArtifactDef>("Deviation", SS2Bundle.Artifacts);
            helper.AddAssetToLoad<ArtifactCode>("DeviationCode", SS2Bundle.Artifacts);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _artifactCode = helper.GetLoadedAsset<ArtifactCode>("DeviationCode");
            _artifactDef = helper.GetLoadedAsset<ArtifactDef>("Deviation");*/
            yield break;
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
