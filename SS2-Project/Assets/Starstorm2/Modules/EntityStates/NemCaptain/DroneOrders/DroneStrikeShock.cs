using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCaptain.Weapon
{
    public class DroneShockStrike : BaseDroneStrike
    {
        [SerializeField]
        public GameObject explosionPrefab;

        public override void OnOrderEffect()
        {
            Debug.Log("exploding");
            Ray aimRay = GetAimRay();
            RaycastHit raycastHit;
            Physics.Raycast(aimRay, out raycastHit, maxDistance, LayerIndex.CommonMasks.bullet);

            bool crit = RollCrit();
            DamageType damageType = DamageType.Shock5s;

            BlastAttack blast = new BlastAttack()
            {
                radius = radius,
                procCoefficient = procCoefficient,
                position = raycastHit.point,
                attacker = gameObject,
                teamIndex = teamComponent.teamIndex,
                crit = crit,
                baseDamage = characterBody.damage * dmgCoefficient,
                damageColorIndex = DamageColorIndex.Default,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = damageType
            };
            blast.Fire();

            EffectManager.SimpleEffect(explosionPrefab, raycastHit.point, Quaternion.identity, true);
        }
    }
}
