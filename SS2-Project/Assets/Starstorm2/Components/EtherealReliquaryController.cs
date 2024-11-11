using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace SS2
{
    public class EtherealReliquaryController : NetworkBehaviour
    {
        [SyncVar]
        public bool hasUpgraded = false;

        private CharacterBody body;
        private ChildLocator cl;
        private CharacterModel cm;
        private Xoroshiro128Plus rng;

        private void Start()
        {
            body = GetComponent<CharacterBody>();
            cl = GetComponentInChildren<ChildLocator>();
            cm = GetComponentInChildren<CharacterModel>();

            if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(SS2Content.Artifacts.Adversity))
            {
                UpgradeReliquary();
            }
        }

        private void OnEnable()
        {
            ArtifactTrialMissionController.onShellDeathServer += ArtifactTrialMissionController_onShellDeathServer;
        }

        private void OnDisable()
        {
            ArtifactTrialMissionController.onShellDeathServer -= ArtifactTrialMissionController_onShellDeathServer;
        }

        private void ArtifactTrialMissionController_onShellDeathServer(ArtifactTrialMissionController artifactTrialMissionController, DamageReport dmgReport)
        {
            if (hasUpgraded)
            {
                EtherealBehavior.instance.OnEtherealTeleporterCharged();

                GameObject exists = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(SS2Assets.LoadAsset<InteractableSpawnCard>("iscStrangerPortal", SS2Bundle.Indev), new DirectorPlacementRule
                {
                    minDistance = 0f,
                    maxDistance = 50f,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = transform.position,
                    spawnOnTarget = transform
                }, rng));
                if (exists)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "hehe oortal in space"
                    });
                }
            }
        }

        private void UpgradeReliquary()
        {
            hasUpgraded = true;

            body.master.inventory.GiveItem(SS2Content.Items.EtherealItemAffix);

            GameObject rayParticles = cl.FindChild("RayParticles").gameObject;
            rayParticles.GetComponent<ParticleSystemRenderer>().material = Artifacts.Adversity._reliquarySolarFlareEthereal;

            MeshRenderer sphereRenderer = rayParticles.transform.parent.GetComponent<MeshRenderer>();
            Material[] sphereMaterials = sphereRenderer.materials;
            sphereMaterials[1] = Artifacts.Adversity._reliquaryShellEthereal;
            sphereRenderer.materials = sphereMaterials;

            cl.FindChild("PP").gameObject.GetComponent<PostProcessVolume>().profile = Artifacts.Adversity._ppReliquaryEthereal;
            cm.baseLightInfos[0].defaultColor = SS2Util.ColorRGB(0 , 92, 252);
            cm.baseLightInfos[1].defaultColor = SS2Util.ColorRGB(178, 100, 74);
            cm.baseLightInfos[2].defaultColor = SS2Util.ColorRGB(90, 193, 131);

            rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
        }
    }
}


