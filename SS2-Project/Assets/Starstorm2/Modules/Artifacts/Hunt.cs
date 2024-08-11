using System.Collections;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;

#if DEBUG
namespace SS2.Artifacts
{
    public class Hunt : SS2Artifact
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ArtifactAssetCollection>("acHavoc", SS2Bundle.Artifacts);
        public static ArtifactDef huntArtifact;


        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override void OnArtifactDisabled()
        {
            On.RoR2.Stage.Start -= SpawnHunter;
            On.RoR2.CharacterBody.OnDeathStart -= ReviveHunter;
        }

        public override void OnArtifactEnabled()
        {
            On.RoR2.Stage.Start += SpawnHunter;
            On.RoR2.CharacterBody.OnDeathStart += ReviveHunter;
        }

        public void SpawnHunter(On.RoR2.Stage.orig_Start orig, Stage self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (self)
                {
                    if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(huntArtifact))
                    {
                        Debug.Log("Hunt Artifact Called");
                        //DirectorPlacementRule rule = new();
                        //rule.placementMode = DirectorPlacementRule.PlacementMode.Random;
                        //DirectorSpawnRequest request = new(card, rule, Run.instance.spawnRng);
                        //request.teamIndexOverride = TeamIndex.Void;
                        //DirectorCore.instance.TrySpawnObject(request);
                    }
                }
            }
        }

        public void ReviveHunter(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            if (NetworkServer.active)
            {
                if (self.baseNameToken == "SS2_HUNTER")
                {
                    //Transform target = PlayerCharacterMasterController.instances[UnityEngine.Random.RandomRange(0, PlayerCharacterMasterController.instances.Count)].master.GetBody().transform;
                    //if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(huntArtifact) && target)
                    //{
                    //    Debug.Log("SkeleArtifact: Reviving Skeleton");
                    //    DirectorPlacementRule rule = new();
                    //    rule.placementMode = DirectorPlacementRule.PlacementMode.Random;
                    //    DirectorSpawnRequest request = new(card, rule, Run.instance.spawnRng);
                    //    request.teamIndexOverride = TeamIndex.Void;
                    //    DirectorCore.instance.TrySpawnObject(request);
                    //}
                }
            }

            orig(self);
        }

        public override IEnumerator LoadContentAsync()
        {
            yield break;
        }
    }
}
#endif
