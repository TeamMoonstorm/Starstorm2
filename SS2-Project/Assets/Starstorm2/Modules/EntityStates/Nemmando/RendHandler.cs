using RoR2;
using UnityEngine;
using Moonstorm;

namespace EntityStates.Nemmando
{
    public class RendHandler : BaseSkillState
    {
        [SerializeField]
        public float RendBaseDamageMultiplier;
        //public static GameObject EpicGougeMultiEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/SpurtGenericBlood");
        public void RendMultiplier(BulletAttack bulletAttack)
        {
            bulletAttack.modifyOutgoingDamageCallback = delegate (BulletAttack _bulletAttack, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
            {
                if (hitInfo.hitHurtBox)
                {
                    if (hitInfo.hitHurtBox.healthComponent)
                    {
                        if (hitInfo.hitHurtBox.healthComponent.body)
                        {
                            CharacterBody momgarfielissmokingthatza = hitInfo.hitHurtBox.healthComponent.body;

                            if (momgarfielissmokingthatza && momgarfielissmokingthatza.HasBuff(Moonstorm.Starstorm2.SS2Content.Buffs.BuffGouge))
                            {
                                
                                damageInfo.damage *= RendBaseDamageMultiplier;
                                damageInfo.damageColorIndex = DamageColorIndex.Bleed;

                                //Moonstorm.Starstorm2.SS2Util.RemoveDotStacks(momgarfielissmokingthatza, Moonstorm.Starstorm2.Buffs.Gouge.index, 1);


                                /*EffectManager.SpawnEffect(EpicGougeMultiEffect, new EffectData
                                {
                                    origin = momgarfielissmokingthatza.transform.localPosition

                                }, true);*/

                            }

                        }
                    }
                }
            };
        }
    }
}

