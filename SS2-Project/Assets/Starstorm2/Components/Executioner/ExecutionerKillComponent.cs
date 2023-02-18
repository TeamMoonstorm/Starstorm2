using Moonstorm.Starstorm2.Orbs;
using Moonstorm.Starstorm2.Survivors;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class ExecutionerKillComponent : MonoBehaviour, IOnKilledServerReceiver
    {
        public static float lifeTime = 0.5f;
        public GameObject attacker;
        public float timeAlive;

        public void FixedUpdate()
        {
            if (timeAlive >= lifeTime)
                Destroy(this);
            timeAlive += Time.fixedDeltaTime;
        }

        public void OnKilledServer(DamageReport report)
        {
            int orbCount = 1;
            CharacterBody body = GetComponent<CharacterBody>();
            if (body)
                orbCount = Executioner.GetIonCountFromBody(body);
            /*if (report.attackerBody.skillLocator.secondary.stock == 0 || report.damageInfo.damageType.HasFlag(DamageType.Shock5s))
                orbCount *= 0;*/
            if (report.victimBody.teamComponent.teamIndex == TeamIndex.Lunar)
                orbCount *= 3;
            if (report.victimBody.isElite)
                orbCount *= 2;
            if (report.damageInfo.damageType.HasFlag(DamageType.BypassOneShotProtection))
                orbCount *= 2;

            //Christ we should really just scale the size of the orb or some shit...
            for (int i = 0; i < orbCount; i++)
            {
                ExecutionerIonOrb ionOrb = new ExecutionerIonOrb();
                ionOrb.origin = transform.position;
                ionOrb.target = Util.FindBodyMainHurtBox(attacker);
                OrbManager.instance.AddOrb(ionOrb);
            }

            if (orbCount >= 50)
            {
                ExecutionerIonSuperOrb superIonOrb = new ExecutionerIonSuperOrb();
                superIonOrb.origin = transform.position;
                superIonOrb.target = Util.FindBodyMainHurtBox(attacker);
                if (orbCount < 100f)
                    superIonOrb.buffDuration = 12f;
                OrbManager.instance.AddOrb(superIonOrb);
            }
        }

        public void Reset()
        {
            timeAlive = 0f;
        }
    }
}