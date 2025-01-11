using RoR2;
using SS2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Nuke
{
    //Apply surge buff, nothing fancy honestly.
    public class ApplySurge : GenericCharacterMain
    {
        public static float baseDuration;

        [SerializeField]
        public float impactRadius = 5f;
        [SerializeField]
        public float impactDamage = 3f;

        private float _duration;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            PlayAnimation("UpperBody, Override", "ApplySurge", "applySurge.playbackRate", _duration);

            if (NetworkServer.active && base.isAuthority)
            {
                characterBody.AddTimedBuff(SS2Content.Buffs.bdNukeSpecial, 10f);

                var blastAttack = new BlastAttack
                {
                    attacker = base.gameObject,
                    baseDamage = damageStat * impactDamage,
                    baseForce = 0f,
                    bonusForce = Vector3.up,
                    crit = RollCrit(),
                    damageType = DamageTypeCombo.GenericSpecial,
                    falloffModel = BlastAttack.FalloffModel.None,
                    procCoefficient = 0.1f,
                    radius = impactRadius,
                    position = base.characterBody.footPosition,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    teamIndex = base.teamComponent.teamIndex,
                };

                blastAttack.Fire();
            }
        }
        

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge > _duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}