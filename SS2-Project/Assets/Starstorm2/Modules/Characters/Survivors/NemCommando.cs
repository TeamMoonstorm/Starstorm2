using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using RoR2.UI;

namespace Moonstorm.Starstorm2.Survivors
{
    //[DisabledContent]
    public sealed class NemCommando : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemCommandoBody", SS2Bundle.Nemmando);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoMonsterMaster", SS2Bundle.Nemmando);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survivorNemCommando", SS2Bundle.Nemmando);

        private GameObject nemesisPod;
        private CharacterSelectController csc;

        public override void Initialize()
        {
            base.Initialize();
            if (Starstorm.ScepterInstalled)
            {
                ScepterCompat();
                CreateNemesisPod();
            }

            On.RoR2.UI.SurvivorIconController.Rebuild += SurvivorIconController_Rebuild;
        }

        private void SurvivorIconController_Rebuild(On.RoR2.UI.SurvivorIconController.orig_Rebuild orig, SurvivorIconController self)
        {
            if (SurvivorCatalog.GetSurvivorDef(self.survivorIndex).bodyPrefab == SS2Assets.LoadAsset<GameObject>("NemCommandoBody", SS2Bundle.NemCommando))
            {
                //Debug.Log("if you know you know");
                if (!SurvivorCatalog.SurvivorIsUnlockedOnThisClient(self.survivorIndex))
                {
                    self.gameObject.SetActive(false);
                }
            }
            orig(self);
        }

        private void Destroy(GameObject gameObject)
        {
            Destroy(gameObject);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterSubmission", SS2Bundle.Nemmando), "NemmandoBody", SkillSlot.Special, 0);
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterBossAttack", SS2Bundle.Nemmando), "NemmandoBody", SkillSlot.Special, 1);
        }

        public void CreateNemesisPod()
        {   
            //later
        }

        public override void ModifyPrefab()
        {
            var cb = BodyPrefab.GetComponent<CharacterBody>();
            //cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
            //cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }
    }
}
