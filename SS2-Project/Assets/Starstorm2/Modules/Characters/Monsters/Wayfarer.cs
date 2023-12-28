using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Monsters
{
    //HEY!!!! YOU!!!!!!!!!!!!!
    //this is the old wayfarer. DON'T ENABLE THIS. it probably does not and will not ever work.
    //you want to mess with wayfarer? do it in LampBoss.cs
    //why is there a new class file named slightly more less obviously instead of just using this one?
    //...
    //...
    //it's just like that, ok?

    //ok
    [DisabledContent]
    public sealed class Wayfarer : MonsterBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("WayfarerBody", SS2Bundle.Indev
            );
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("WayfarerMaster", SS2Bundle.Indev);
        //public override MSMonsterDirectorCardHolder directorCards { get; set; } = Assets.Instance.MainAssetBundle.LoadAsset<MSMonsterDirectorCardHolder>("WayfarerCardHolder");

        //public override MSMonsterDirectorCard MonsterDirectorCard { get => throw new System.NotImplementedException(); }

        internal static GameObject wayfarerBuffWardPrefab;

        //Try to get rid of this
        public override void Hook()
        {
            //On.RoR2.CharacterBody.AddBuff_BuffDef += CharacterBody_AddBuff_BuffDef;
            //On.RoR2.Projectile.HookProjectileImpact.FixedUpdate += HookProjectileImpact_FixedUpdate;
        }


        //make real buff
        private static void CharacterBody_AddBuff_BuffDef(On.RoR2.CharacterBody.orig_AddBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            //wayfarer can't buff itself/other wayfarers
            if (self.bodyIndex == BodyCatalog.FindBodyIndex("WayfarerBody") && buffDef == RoR2Content.Buffs.WarCryBuff)
                return;
            orig(self, buffDef);
        }

        private static void HookProjectileImpact_FixedUpdate(On.RoR2.Projectile.HookProjectileImpact.orig_FixedUpdate orig, RoR2.Projectile.HookProjectileImpact self)
        {
            //destroy chain hook before it tries to reel in (prevents NRE spam)
            if (self.hookState == RoR2.Projectile.HookProjectileImpact.HookState.Reel &&
                self.projectileController.Networkowner?.GetComponent<CharacterBody>()?.bodyIndex == BodyCatalog.FindBodyIndex("WayfarerBody"))
            {
                Object.Destroy(self);
            }
            else
                orig(self);
        }
    }
}
