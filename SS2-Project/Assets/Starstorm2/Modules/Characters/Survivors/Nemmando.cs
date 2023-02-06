using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using System.Collections;

namespace Moonstorm.Starstorm2.Survivors
{
    [DisabledContent]
    public sealed class Nemmando : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoBody", SS2Bundle.Nemmando);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemmandoMonsterMaster", SS2Bundle.Nemmando);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("SurvivorNemmando", SS2Bundle.Nemmando);
        GameObject footstepDust { get; set; } = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

        public override void Initialize()
        {
            base.Initialize();
            if (Starstorm.ScepterInstalled)
            {
                ScepterCompat();
            }
            On.RoR2.CharacterBody.OnInventoryChanged += DisableGenesisLoopParticles;
            //RoR2.SceneDirector.onPrePopulateSceneServer += SceneStartDisableParticles;
        }

        private void DisableGenesisLoopParticles(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);

            //SS2Log.Debug("self: " + self + " | token:" + self.baseNameToken + " | name:" + self.name);
            if (self.inventory)
            {
                if(self.inventory.GetItemCount(RoR2Content.Items.NovaOnLowHealth) > 0)
                {
                    var token = self.gameObject.GetComponent<NemmandoLoopToken>();
                    if (!token)
                    {
                        token = self.gameObject.AddComponent<NemmandoLoopToken>();
                        token.DisableParticles(self);
                    }
                    else
                    {
                        token.DisableParticles(self);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterSubmission", SS2Bundle.Nemmando), "NemmandoBody", SkillSlot.Special, 0);
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("NemmandoScepterBossAttack", SS2Bundle.Nemmando), "NemmandoBody", SkillSlot.Special, 1);
        }

        public override void ModifyPrefab()
        {
            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
            //cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = footstepDust;
        }
    }
    public class NemmandoLoopToken : MonoBehaviour //okay so this method fucking sucks but it works. the only other item this could find is captains' probe, but nemmando shouldn't have that anyway. what? metamorphosis? 
    {
        public void DisableParticles(CharacterBody self)
        {
            StartCoroutine(DisableParticlesDelayed(self));
        }

        IEnumerator DisableParticlesDelayed(CharacterBody self)
        {
            yield return new WaitForSeconds(1f);
            if (self)
            {
                var cl = self.GetComponentInChildren<ChildLocator>();
                if (cl)
                {
                    if (cl.FindChild("ChargeSparks"))
                    {
                        cl.FindChild("ChargeSparks").gameObject.SetActive(false);
                    }
                }
            }
        }
        //(this bit here would go in nemmando not the token. it doesn't need it so it's commented out, but i'm worried i missed something and will later need to use this. so i'm leaving it here)
        //private void SceneStartDisableParticles(SceneDirector obj)
        //{
        //    foreach (var player in PlayerCharacterMasterController.instances)
        //    {
        //        if(player.body.baseNameToken == "SS2_NEMMANDO_NAME")
        //        {
        //            var token = player.body.gameObject.GetComponent<IHATEMODDING>();
        //            if (token)
        //            {
        //                token.DisableParticles(player.body);
        //            }
        //            if(player.body.inventory.GetItemCount(RoR2Content.Items.NovaOnLowHealth) > 0)
        //            {
        //                var cl = player.body.GetComponentInChildren<ChildLocator>();
        //                if (cl)
        //                {
        //                    if (cl.FindChild("ChargeSparks"))
        //                    {
        //                        cl.FindChild("ChargeSparks").gameObject.SetActive(false);
        //                    }
        //                }
        //            }
        //
        //        }
        //    }
        //}


    }
}
