using RoR2;
using UnityEngine.Networking;

namespace EntityStates.Pickups.Augury
{
    public class AuguryDetonation : AuguryBaseState
    {
        public static string detonationSound = "";
        public static float duration = 5;
        public static float divisorValueForBosses = 2;

        public TeamIndex attackerTeam;
        private CharacterBody body;
        public override void OnEnter()
        {
            base.OnEnter();
            body = attachedBody;
            if (NetworkServer.active && attachedBody)
            {
                foreach (var tc in TeamComponent.GetTeamMembers(attackerTeam))
                {
                    var body = tc.body;
                    if (body)
                    {
                        if (body.isBoss || body.isChampion)
                        {
                            DamageForHalfOfMaxHealth(body);
                        }
                        else
                        {
                            var killerAndInflictor = attachedBody ? attachedBody.gameObject : null;
                            body.healthComponent.Suicide(killerAndInflictor, killerAndInflictor, DamageType.VoidDeath);
                            Util.PlaySound(body.sfxLocator.deathSound, gameObject);
                        }
                    }
                }
            }
            auguryEffect.fadeOut = true;
        }

        private void DamageForHalfOfMaxHealth(CharacterBody bossBody)
        {
            DamageInfo dInfo = new DamageInfo()
            {
                attacker = body ? body.gameObject : null,
                inflictor = body ? body.gameObject : null,
                crit = body.RollCrit(),
                damage = bossBody.maxHealth / 2,
                damageColorIndex = DamageColorIndex.DeathMark,
                damageType = DamageType.Generic,
                dotIndex = DotController.DotIndex.None,
                procCoefficient = 1,
            };
            bossBody.healthComponent.TakeDamage(dInfo);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                auguryEffect.explosionScaleCurve.enabled = false;
                outer.SetNextState(new AuguryRecharge());
            }
        }
    }
}
