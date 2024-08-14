using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using R2API;
using MSU;
using System.Collections;
using RoR2.ContentManagement;

namespace SS2.Survivors
{
    public sealed class NemMerc : SS2Survivor, IContentPackModifier
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemMerc", SS2Bundle.NemMercenary);

        // configggggggg
        public static int maxClones = 1;
        public static int maxHolograms = 10;
        public static DeployableSlot clone;
        public static DeployableSlot hologram;

        public static DamageAPI.ModdedDamageType damageType;
        private GameObject _knifeProjectile;

        
        public override void Initialize()
        {
            _knifeProjectile = AssetCollection.FindAsset<GameObject>("KnifeProjectile");
            damageType = DamageAPI.ReserveDamageType();

            clone = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) =>
            {
                if (self.bodyInstanceObject)
                    return self.bodyInstanceObject.GetComponent<SkillLocator>().special.maxStock;
                return 1;
            });
            hologram = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxHolograms; });

            if (SS2Main.ScepterInstalled)
            {
                ScepterCompat();
            }

            On.RoR2.CharacterAI.BaseAI.Target.GetBullseyePosition += HopefullyHarmlessAIFix;
            ModifyPrefab();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemMercScepterSpecial", SS2Bundle.NemMercenary), "NemMercBody", SkillSlot.Special, 0);
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemMercScepterClone", SS2Bundle.NemMercenary), "NemMercBody", SkillSlot.Special, 1);
        }

        //makes AI not shit itself when targetting objects without a characterbody
        //which nemmerc AI needs to target holograms
        //99% certain this doesnt effect anything otherwise
        private bool HopefullyHarmlessAIFix(On.RoR2.CharacterAI.BaseAI.Target.orig_GetBullseyePosition orig, RoR2.CharacterAI.BaseAI.Target self, out Vector3 position)
        {
            if (!self.characterBody && self.gameObject)
            {

                self.lastKnownBullseyePosition = self.gameObject.transform.position;
                self.lastKnownBullseyePositionTime = Run.FixedTimeStamp.now;
            }
            return orig(self, out position);
        }


        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();

            cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();

            _knifeProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(damageType);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

    }
}
