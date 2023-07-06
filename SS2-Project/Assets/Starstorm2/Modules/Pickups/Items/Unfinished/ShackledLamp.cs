using RoR2;
using RoR2.Items;
using RoR2.Projectile;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class ShackledLamp : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("ShackledLamp", SS2Bundle.Indev);

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ShackledLamp;

            private static GameObject projectilePrefab = SS2Assets.LoadAsset<GameObject>("LampBulletPlayer", SS2Bundle.Indev);
            private float attackCounter;

            private void Start()
            {
                body.onSkillActivatedAuthority += ChainEffect;
            }

            private void ChainEffect(GenericSkill skill)
            {
                if (!body.skillLocator.primary.Equals(skill))
                    return;

                attackCounter++;
                if (attackCounter >= 5)
                {
                    attackCounter %= 5f;
                    //Util.PlayAttackSpeedSound(EntityStates.GravekeeperBoss.FireHook.soundString, body.gameObject, body.attackSpeed);
                    float damage = body.damage * (2f + stack);
                    ProjectileManager.instance.FireProjectile(projectilePrefab, body.inputBank.aimOrigin, Util.QuaternionSafeLookRotation(body.inputBank.aimDirection), body.gameObject,
                        damage, 40f, Util.CheckRoll(body.crit, body.master));
                }
            }
            private void OnDestroy()
            {
                body.onSkillActivatedAuthority -= ChainEffect;
            }
        }
    }
}
