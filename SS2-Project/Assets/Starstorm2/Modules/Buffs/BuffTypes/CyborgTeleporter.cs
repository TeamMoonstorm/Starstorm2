using Moonstorm.Components;
using Moonstorm.Starstorm2.Equipments;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class CyborgTeleporter : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffCyborgTeleporter", SS2Bundle.Indev);

        public static float cooldownReduction = 0.5f;
        public static float percentHealthShieldPerSecond = 0.075f;
        public override void Initialize()
        {
            base.Initialize();

            On.RoR2.GenericSkill.RunRecharge += AddRecharge;
        }

        public sealed class CyborgTeleBuffBehavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation(useOnClient = false, useOnServer = true)]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffCyborgTeleporter;

            private void FixedUpdate()
            {
                float maxHP = base.body.healthComponent.fullHealth;
                base.body.healthComponent.AddBarrier(maxHP * CyborgTeleporter.percentHealthShieldPerSecond * Time.fixedDeltaTime);
            }
        }
        private void AddRecharge(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt)
        {
            if (self.characterBody.HasBuff(SS2Content.Buffs.BuffCyborgTeleporter))
            {
                dt *= 1f / (1 - CyborgTeleporter.cooldownReduction);
            }
            orig(self, dt);
        }

    }
    
}
