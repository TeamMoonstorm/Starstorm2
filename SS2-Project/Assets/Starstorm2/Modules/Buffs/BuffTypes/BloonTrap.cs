using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class BloonTrap : BuffBase, IBodyStatArgModifier
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffBloonTrap", SS2Bundle.Indev);

        private static float slowAmount = 0.5f;

        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.HealthComponent.GetHealthBarValues += AddSuck;
        }

        public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.moveSpeedMultAdd -= slowAmount;
        }

        private HealthComponent.HealthBarValues AddSuck(On.RoR2.HealthComponent.orig_GetHealthBarValues orig, HealthComponent self)
        {
            HealthComponent.HealthBarValues result = orig.Invoke(self);
            if (!self.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToExecutes) && self.body.HasBuff(SS2Content.Buffs.BuffBloonTrap))
            {
                result.cullFraction = Mathf.Max(result.cullFraction, 0.2f);
            }
            return result;
        }
    }
}
