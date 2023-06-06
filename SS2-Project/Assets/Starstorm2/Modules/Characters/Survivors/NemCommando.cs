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
            On.RoR2.GenericSkill.RecalculateMaxStock += CheckNemmandoMag;
        }

        private void CheckNemmandoMag(On.RoR2.GenericSkill.orig_RecalculateMaxStock orig, GenericSkill self)
        {
            orig(self);
            if (self.characterBody) //this makes sure this function doesnt happen on CaptainSupplyDropSkillDef, because it doesn't have a skillNameToken
            {
                //SS2Log.Info("name token: " + self.characterBody.baseNameToken);
                if (self.characterBody.baseNameToken == "SS2_NEMCOMMANDO_NAME" || self.characterBody.baseNameToken == "SS2_NEMMANDO_NAME") //so i thought the name would be NEMCOMMANDO but it's NEMMANDO? This is fine, but weird and might get fixed, so I'm gonna put both!
                {
                    if (self.skillNameToken == "SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME")
                    {
                        var body = self.characterBody;
                        if (body)
                        {
                            var token = body.gameObject.GetComponent<NemmandoPistolToken>();
                            if (!token)
                            {
                                token = body.gameObject.AddComponent<NemmandoPistolToken>();
                                token.secondaryStocks = self.maxStock;

                            }
                            else
                            {
                                if (token.secondaryStocks != self.maxStock)
                                {
                                    //SS2Log.Info("attempting to force reload");
                                    token.secondaryStocks = self.maxStock;
                                    var state = self.stateMachine;
                                    EntityStates.NemCommando.ReloadGun nextState = new EntityStates.NemCommando.ReloadGun();
                                    state.SetNextState(nextState);
                                }
                            }

                        }
                    }
                }
            }
            
        }

        //private void CheckNemmandoMag4(CharacterBody obj)
        //{
        //    SS2Log.Info(obj.baseNameToken);
        //    var locator = obj.skillLocator;
        //    if (locator)
        //    {
        //        //MonoBehavior.StartCoroutine(updateNemmandoMag(locator));
        //        SS2Log.Info("secondary: " + locator.secondary + " | " + locator.secondary.skillNameToken + " | " + locator.secondary.stock + " | " + locator.secondary.maxStock);
        //        //List<ItemIndex> list = obj.itemAcquisitionOrder;
        //        if(locator.secondary.skillNameToken == "SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME")
        //        {
        //            SS2Log.Info("is secondaryshoot");
        //            if (locator.secondary.maxStock != locator.secondary.stock)
        //            {
        //                //SS2Log.Info("Should force secondary reload here");
        //                SS2Log.Info("attempting to force reload");
        //                var state = locator.secondary.stateMachine;
        //                EntityStates.NemCommando.ReloadGun nextState = new EntityStates.NemCommando.ReloadGun();
        //                state.SetNextState(nextState);
        //            }
        //        }
        //        //SS2Log.Info("ahh " + list[list.Count - 1]);
        //    }
        //}

        //IEnumerator updateNemmandoMag(SkillLocator locator)
        //{
        //    yield return new WaitForSeconds(.1f);
        //    SS2Log.Info("secondary: " + locator.secondary + " | " + locator.secondary.skillNameToken + " | " + locator.secondary.stock + " | " + locator.secondary.maxStock);
        //    //List<ItemIndex> list = obj.itemAcquisitionOrder;
        //    if (locator.secondary.skillNameToken == "SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME")
        //    {
        //        SS2Log.Info("is secondaryshoot");
        //        if (locator.secondary.maxStock != locator.secondary.stock)
        //        {
        //            //SS2Log.Info("Should force secondary reload here");
        //            SS2Log.Info("attempting to force reload");
        //            var state = locator.secondary.stateMachine;
        //            EntityStates.NemCommando.ReloadGun nextState = new EntityStates.NemCommando.ReloadGun();
        //            state.SetNextState(nextState);
        //        }
        //    }
        //    //SS2Log.Info("ahh " + list[list.Count - 1]);
        //
        //}
            //private void CheckNemmandoMag3(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
            //{
            //    orig(self);
            //    if (self)
            //    {
            //        SS2Log.Info(self.baseNameToken);
            //        var locator = self.skillLocator;
            //        if (locator)
            //        {
            //            SS2Log.Info("secondary: " + locator.secondary + " | " + locator.secondary.skillNameToken + " | " + locator.secondary.stock + " | " + locator.secondary.maxStock);
            //            //List<ItemIndex> list = obj.itemAcquisitionOrder;
            //            if (locator.secondary.maxStock != locator.secondary.stock && locator.secondary.skillNameToken == "SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME")
            //            {
            //                //SS2Log.Info("Should force secondary reload here");
            //                SS2Log.Info("attempting to force reload");
            //                var state = locator.secondary.stateMachine;
            //                EntityStates.NemCommando.ReloadGun nextState = new EntityStates.NemCommando.ReloadGun();
            //                state.SetNextState(nextState);
            //            }
            //            //SS2Log.Info("ahh " + list[list.Count - 1]);
            //        }
            //    }
            //}

            //private void CheckNemmandoMag2(On.RoR2.GenericSkill.orig_RecalculateMaxStock orig, GenericSkill self)
            //{
            //    orig(self);
            //    SS2Log.Info("AHHHHHHHHHHHH " + self.skillNameToken + " | " + self.stock + " | " + self.maxStock);
            //    if(self.skillNameToken == "SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME" && self.stock != self.maxStock)
            //    {
            //        SS2Log.Info("attempting to force reload");
            //        var state = self.stateMachine;
            //        EntityStates.NemCommando.ReloadGun nextState = new EntityStates.NemCommando.ReloadGun();
            //        state.SetNextState(nextState);
            //    }
            //}

            //private void CheckNemmandoMag(Inventory obj)
            //{
            //    var master = obj.GetComponent<CharacterMaster>();
            //    if (master)
            //    {
            //        var body = master.GetBody();
            //        
            //        if (body)
            //        {
            //            SS2Log.Info(body.baseNameToken);
            //            var locator = body.skillLocator;
            //            if (locator)
            //            {
            //                SS2Log.Info("secondary: " + locator.secondary + " | " + locator.secondary.skillNameToken + " | " + locator.secondaryBonusStockSkill + " | " + locator.secondaryBonusStockSkill.skillNameToken);
            //                //List<ItemIndex> list = obj.itemAcquisitionOrder;
            //                if(locator.secondary.maxStock != locator.secondary.stock && locator.secondary.skillNameToken == "SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME")
            //                {
            //                    //SS2Log.Info("Should force secondary reload here");
            //                    SS2Log.Info("attempting to force reload");
            //                    var state = locator.secondary.stateMachine;
            //                    EntityStates.NemCommando.ReloadGun nextState = new EntityStates.NemCommando.ReloadGun();
            //                    state.SetNextState(nextState);
            //                }
            //                //SS2Log.Info("ahh " + list[list.Count - 1]);
            //            }
            //        }
            //    }
            //}

        private void CharacterSelectBarController_Awake(On.RoR2.CharacterSelectBarController.orig_Awake orig, CharacterSelectBarController self)
        {
            //hide nemcommando from css proper
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

        public override void ModifyPrefab()
        {
            var cb = BodyPrefab.GetComponent<CharacterBody>();
            //cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            //cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
            cb._defaultCrosshairPrefab = SS2Assets.LoadAsset<GameObject>("HalfCrosshair.prefab", SS2Bundle.NemCommando);
            //cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }
    }

    public class NemmandoPistolToken : MonoBehaviour
    {
        public int secondaryStocks = 8;
    }
}
