using Moonstorm.Components;
using Moonstorm.Starstorm2.Equipments;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class GreaterBanner : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffGreaterBanner", SS2Bundle.Equipments);


        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier, IStatItemBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffGreaterBanner;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
                {
                    args.critAdd += GreaterWarbanner.extraCrit;
                    args.regenMultAdd += GreaterWarbanner.extraRegeneration;

                    //args.cooldownReductionAdd += GreaterWarbanner.cooldownReduction;
                }
            }
            public void RecalculateStatsEnd()
            {
               
                //if (body.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
                //{
                //    if (body.skillLocator)
                //    {
                //        if (body.skillLocator.primary)
                //            body.skillLocator.primary.cooldownScale *= GreaterWarbanner.cooldownReduction;
                //        if (body.skillLocator.secondary)
                //            body.skillLocator.secondary.cooldownScale *= GreaterWarbanner.cooldownReduction;
                //        if (body.skillLocator.utility)
                //            body.skillLocator.utility.cooldownScale *= GreaterWarbanner.cooldownReduction;
                //        if (body.skillLocator.special)
                //            body.skillLocator.special.cooldownScale *= GreaterWarbanner.cooldownReduction;
                //    }
                //}
            }

            public void RecalculateStatsStart()
            {
                //BannerCDToken token = body.GetComponent<BannerCDToken>();
                //if (!token)
                //{
                //    token = body.gameObject.AddComponent<BannerCDToken>();
                //    token.body = body;
                //}

            }
            /*public void CooldownReductionWarBanner(CharacterBody OBSOLETE, RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
                {
                    SkillLocator locator = body.skillLocator;

                    if (locator.primary)
                    {
                        locator.primary.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    }
                    if (locator.secondary)
                    {
                        locator.secondary.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    }
                    if (locator.utility)
                    {
                        locator.utility.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    }
                    if (locator.special)
                    {
                        locator.special.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    }
                }
            }*/
        }

    }
    //public class BannerCDToken : MonoBehaviour //lmao
    //{
    //    //public GameObject[] ownedBanners = new GameObject[0];
    //    //public List<GameObject> ownedBanners = new List<GameObject>(0);
    //    public CharacterBody body;
    //    //public float timer = 0;
    //    //public List<int> lastUpdate;
    //    //public float soundCooldown = 5f;
    //
    //    void Awake()
    //    {
    //
    //    }
    //
    //    private void FixedUpdate()
    //    {
    //        if (body.HasBuff(SS2Content.Buffs.BuffGreaterBanner) && NetworkServer.active)
    //        {
    //
    //            //if (body.skillLocator)
    //            //{
    //            //    if (body.skillLocator.primary)
    //            //        body.skillLocator.primary.cooldownScale *= GreaterWarbanner.cooldownReduction;
    //            //    if (body.skillLocator.secondary)
    //            //        body.skillLocator.secondary.cooldownScale *= GreaterWarbanner.cooldownReduction;
    //            //    if (body.skillLocator.utility)
    //            //        body.skillLocator.utility.cooldownScale *= GreaterWarbanner.cooldownReduction;
    //            //    if (body.skillLocator.special)
    //            //        body.skillLocator.special.cooldownScale *= GreaterWarbanner.cooldownReduction;
    //            //}
    //            SkillLocator locator = body.skillLocator;
    //            bool hasAuthority = locator.hasEffectiveAuthority;
    //            if (locator)
    //            {
    //                for(int i = 0; i < locator.allSkills.Length; ++i)
    //                {
    //                    GenericSkill genericSkill = locator.allSkills[i];
    //                    //if (lastUpdate.Count < (i + 1))
    //                    //{
    //                    //    lastUpdate.Add(0); //gives it same number of skills in same order as allSkills
    //                    //}
    //                    if(genericSkill.stock < genericSkill.maxStock)
    //                    {
    //                        //SS2Log.Info("Skill Cooldown: " + genericSkill.rechargeStopwatch);
    //                        //SS2Log.Warning("Skill Cooldown: " + genericSkill.rechargeStopwatch);
    //                        // float nextHalf = Mathf.Floor(genericSkill.rechargeStopwatch) + .5f;
    //                        if(genericSkill.rechargeStopwatch > Mathf.Floor(genericSkill.rechargeStopwatch) + GreaterWarbanner.cooldownReduction)
    //                        {
    //                            genericSkill.rechargeStopwatch += GreaterWarbanner.cooldownReduction;
    //                            //locator.DeductCooldownFromAllSkillsServer(genericSkill, hasAuthority);
    //                            //timer = 0f;
    //                        }
    //                        //genericSkill.rechargeStopwatch += 1;
    //                    }
    //                }
    //            }
    //            //timer = 0f;
    //                
    //        }
    //        //if (body.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
    //        //{
    //        //    timer += Time.deltaTime;
    //        //}
    //
    //        //timer += Time.deltaTime;
    //
    //        //private void DeductCooldownFromAllSkillsAuthority(float deduction) //definitely not stolen from Phreel's Wicked Ring mod (thank you phreel)
    //        //{
    //        //    for (int i = 0; i < this.allSkills.Length; i++)
    //        //    {
    //        //        GenericSkill genericSkill = this.allSkills[i];
    //        //        if (genericSkill.stock < genericSkill.maxStock)
    //        //        {
    //        //            genericSkill.rechargeStopwatch += deduction;
    //        //        }
    //        //    }
    //        //}
    //
    //
    //    }
    //
    //    //public void DeductCooldownFromAllSkillsServer(GenericSkill skill, bool hasAuthority)
    //    //{
    //    //    if (hasAuthority)
    //    //    {
    //    //        DeductCooldownFromAllSkillsAuthority(skill);
    //    //        return;
    //    //    }
    //    //    CallRpcDeductCooldownFromAllSkillsServer(deduction);
    //    //}
    //
    //    //public void CallRpcDeductCooldownFromAllSkillsServer(float deduction)
    //    //{
    //    //    if (!NetworkServer.active)
    //    //    {
    //    //        Debug.LogError("RPC Function RpcDeductCooldownFromAllSkillsServer called on client.");
    //    //        return;
    //    //    }
    //    //    NetworkWriter networkWriter = new NetworkWriter();
    //    //    networkWriter.Write(0);
    //    //    networkWriter.Write((short)((ushort)2));
    //    //    networkWriter.WritePackedUInt32((uint)SkillLocator.kRpcRpcDeductCooldownFromAllSkillsServer);
    //    //    networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
    //    //    networkWriter.Write(deduction);
    //    //    this.SendRPCInternal(networkWriter, 0, "RpcDeductCooldownFromAllSkillsServer");
    //    //}
    //
    //    // Token: 0x060030BD RID: 12477 RVA: 0x000CF25A File Offset: 0x000CD45A
    //    //[ClientRpc]
    //    //private void RpcDeductCooldownFromAllSkillsServer(float deduction)
    //    //{
    //    //    if (this.hasEffectiveAuthority)
    //    //    {
    //    //        this.DeductCooldownFromAllSkillsAuthority(deduction);
    //    //    }
    //    //}
    //    //
    //    //private void DeductCooldownFromAllSkillsAuthority(float deduction)
    //    //{
    //    //    for (int i = 0; i < this.allSkills.Length; i++)
    //    //    {
    //    //        GenericSkill genericSkill = this.allSkills[i];
    //    //        if (genericSkill.stock < genericSkill.maxStock)
    //    //        {
    //    //            genericSkill.rechargeStopwatch += deduction;
    //    //        }
    //    //    }
    //    //}
    //
    //}
}
