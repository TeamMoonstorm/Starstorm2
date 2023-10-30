using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Moonstorm.Starstorm2.Survivors
{
    //[DisabledContent]
    public sealed class NemCommando : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemCommandoBody", SS2Bundle.NemCommando);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemCommandoMonsterMaster", SS2Bundle.NemCommando);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survivorNemCommando", SS2Bundle.NemCommando);

        private GameObject nemesisPod;
        private CharacterSelectController csc;

        public override void Initialize()
        {
            base.Initialize();
            if (Starstorm.ScepterInstalled)
            {
                ScepterCompat();
                //CreateNemesisPod();
            }
            On.RoR2.CharacterSelectBarController.Awake += CharacterSelectBarController_Awake;
        }

        private void CharacterSelectBarController_Awake(On.RoR2.CharacterSelectBarController.orig_Awake orig, CharacterSelectBarController self)
        {
            //hide nemcommando from css proper
            SS2Content.Survivors.survivorNemMerc.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.survivorNemMerc.survivorIndex); // hello nem comado
            SS2Content.Survivors.survivorNemCommando.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.survivorNemCommando.survivorIndex);
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
    }

    public class NemmandoPistolToken : MonoBehaviour
    {
        public int secondaryStocks = 8;
    }
}
