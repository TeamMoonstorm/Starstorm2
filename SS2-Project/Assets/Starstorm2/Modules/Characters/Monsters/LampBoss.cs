using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Monsters
{
    //[DisabledContent]
    public sealed class LampBoss : MonsterBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("LampBossBody", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("LampBossMaster", SS2Bundle.Indev);
        //public override MSMonsterDirectorCardHolder directorCards { get; set; } = Assets.Instance.MainAssetBundle.LoadAsset<MSMonsterDirectorCardHolder>("WayfarerCardHolder");

        public override MSMonsterDirectorCard MonsterDirectorCard { get; } = SS2Assets.LoadAsset<MSMonsterDirectorCard>("msmdcLampBoss", SS2Bundle.Indev);

        internal static GameObject wayfarerBuffWardPrefab;

        //Try to get rid of this
        public override void Hook()
        {
            On.RoR2.CharacterBody.AddBuff_BuffDef += CharacterBody_AddBuff_BuffDef;
            //On.RoR2.Projectile.HookProjectileImpact.FixedUpdate += HookProjectileImpact_FixedUpdate;
        }


        //make real buff
        private static void CharacterBody_AddBuff_BuffDef(On.RoR2.CharacterBody.orig_AddBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            //wayfarer can't buff itself/other wayfarers
            if (self.bodyIndex == BodyCatalog.FindBodyIndex("LampBossBody") && buffDef == SS2Content.Buffs.bdLampBuff)
                return;
            orig(self, buffDef);
        }
    }
}
