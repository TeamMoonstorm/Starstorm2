using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using R2API;
using RoR2.Projectile;
namespace Moonstorm.Starstorm2.Survivors
{
    //[DisabledContent]
    public sealed class NemMerc : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemMercBody", SS2Bundle.NemMercenary);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemMercMonsterMaster", SS2Bundle.NemMercenary);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survivorNemMerc", SS2Bundle.NemMercenary);

        // configggggggg
        public static int maxClones = 1;
        public static int maxHolograms = 10;
        public static DeployableSlot clone;
        public static DeployableSlot hologram;
        public override void Initialize()
        {
            base.Initialize();

            clone = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) =>
            {
                if (self.bodyInstanceObject)
                    return self.bodyInstanceObject.GetComponent<SkillLocator>().special.maxStock;
                return 1;
            });
            hologram = DeployableAPI.RegisterDeployableSlot((self, deployableCountMultiplier) => { return maxHolograms; });
            
            if (Starstorm.ScepterInstalled)
            {
                ScepterCompat();
            }
        }

        public override void Hook()
        {
            base.Hook();
            On.RoR2.CharacterAI.BaseAI.Target.GetBullseyePosition += HopefullyHarmlessAIFix;

            // ITEM DISPLAY DEBUG
            // On.RoR2.CharacterModel.InstantiateDisplayRuleGroup += CharacterModel_InstantiateDisplayRuleGroup;
        }

        //private void CharacterModel_InstantiateDisplayRuleGroup(On.RoR2.CharacterModel.orig_InstantiateDisplayRuleGroup orig, CharacterModel self, DisplayRuleGroup displayRuleGroup, ItemIndex itemIndex, EquipmentIndex equipmentIndex)
        //{
        //    SS2Log.Info(ItemCatalog.GetItemDef(itemIndex).nameToken);
        //    orig(self, displayRuleGroup, itemIndex, equipmentIndex);
        //}



        //makes AI not shit itself when targetting objects without a characterbody
        //which nemmerc AI needs to target holograms
        //99% certain this doesnt effect anything otherwise
        private bool HopefullyHarmlessAIFix(On.RoR2.CharacterAI.BaseAI.Target.orig_GetBullseyePosition orig, RoR2.CharacterAI.BaseAI.Target self, out Vector3 position)
        {
            if(!self.characterBody && self.gameObject)
            {

                self.lastKnownBullseyePosition = self.gameObject.transform.position;
                self.lastKnownBullseyePositionTime = Run.FixedTimeStamp.now;
            }
            return orig(self, out position);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemMercScepterSpecial", SS2Bundle.NemMercenary), "NemMercBody", SkillSlot.Special, 0);
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemMercScepterClone", SS2Bundle.NemMercenary), "NemMercBody", SkillSlot.Special, 1);
        }

        public override void ModifyPrefab()
        {
            var cb = BodyPrefab.GetComponent<CharacterBody>();
            //cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            ///  ??????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
            
            cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();

            GameObject g1 = SS2Assets.LoadAsset<GameObject>("KnifeProjectile", SS2Bundle.NemMercenary);
            g1.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.RedirectHologram.damageType);

            GameObject g2 = SS2Assets.LoadAsset<GameObject>("NemMercHologram", SS2Bundle.NemMercenary);
            g2.RegisterNetworkPrefab();
        }
    }
}
