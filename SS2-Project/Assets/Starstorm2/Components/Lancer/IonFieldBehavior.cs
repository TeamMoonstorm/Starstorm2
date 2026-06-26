using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public sealed class IonFieldBehavior : BaseBuffBehaviour
    {
        [BuffDefAssociation(useOnServer = true, useOnClient = false)]
        private static BuffDef GetBuffDef() => SS2.Survivors.Lancer.bdIonField;

        public static float tickInterval = 0.5f;
        public static float baseDamagePerTick = 1f;
        public static float baseRadius = 5f;
        public static float radiusPerStack = 0.5f;
        public static GameObject tickEffectPrefab;

        private float tickTimer;

        private void OnEnable()
        {
            tickTimer = 0f;
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active || !hasAnyStacks || !characterBody)
                return;

            tickTimer += Time.fixedDeltaTime;
            if (tickTimer < tickInterval)
                return;

            tickTimer -= tickInterval;

            int stacks = characterBody.GetBuffCount(buffDef);
            float damage = baseDamagePerTick * (1f + 0.2f * (characterBody.level - 1f)) * stacks;
            float radius = baseRadius + radiusPerStack * (stacks - 1);

            if (tickEffectPrefab)
            {
                EffectManager.SpawnEffect(tickEffectPrefab, new EffectData
                {
                    origin = characterBody.corePosition,
                    scale = radius
                }, true);
            }

            BlastAttack blastAttack = new BlastAttack();
            blastAttack.position = characterBody.corePosition;
            blastAttack.baseDamage = damage;
            blastAttack.baseForce = 0f;
            blastAttack.radius = radius;
            blastAttack.attacker = gameObject;
            blastAttack.inflictor = null;
            // Ion field is a debuff on enemies. We want the AoE to damage enemies (from the player perspective).
            // Since the debuffed body is an enemy, using its teamIndex would PROTECT enemies from the blast.
            // Use TeamIndex.Player so the blast damages everyone NOT on the player team
            blastAttack.teamIndex = TeamIndex.Player;
            blastAttack.crit = false;
            blastAttack.procCoefficient = 0f;
            blastAttack.damageColorIndex = DamageColorIndex.Item;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.damageType = DamageType.Generic;
            blastAttack.Fire();
        }
    }
}
