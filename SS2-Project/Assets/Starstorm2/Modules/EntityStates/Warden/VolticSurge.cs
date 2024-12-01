using RoR2;
using RoR2.Projectile;
using SS2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Warden
{
    public class VolticSurge : BaseSkillState
    {
        public static float duration = 7f;

        private GameObject wardInstance;

        private static float projectileEraserRadius = 20;

        private static float minimumFireFrequency = 10;

        private static float baseRechargeFrequency = 2;

        // TODO: Add an effect for deleting projectiles for nice visual polish
        //public static GameObject tracerEffectPrefab;

        private float rechargeTimer;

        private float rechargeFrequency => baseRechargeFrequency * (base.characterBody ? base.characterBody.attackSpeed : 1f);

        private float fireFrequency => Mathf.Max(minimumFireFrequency, rechargeFrequency);

        private float timeBetweenFiring => 1f / fireFrequency;

        private bool isReadyToFire => rechargeTimer <= 0f;

        public override void OnEnter()
        {
            base.OnEnter();

            if (NetworkServer.active)
            {
                Vector3 position = inputBank.aimOrigin - (inputBank.aimDirection);
                wardInstance = UnityEngine.Object.Instantiate(SS2.Survivors.Warden.wardenSurgeWard, position, Quaternion.identity);
                wardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(characterBody.gameObject);
            }
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!NetworkServer.active)
            {
                return;
            }
            rechargeTimer -= GetDeltaTime();
            if (base.fixedAge > timeBetweenFiring)
            {
                base.fixedAge -= timeBetweenFiring;
                if (isReadyToFire && DeleteNearbyProjectile())
                {
                    rechargeTimer = 1f / rechargeFrequency;
                }
            }

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Exiting voltic surge");
        }

        private bool DeleteNearbyProjectile()
        {
            Vector3 vector = (base.characterBody ? base.characterBody.corePosition : Vector3.zero);
            TeamIndex teamIndex = (base.characterBody ? base.characterBody.teamComponent.teamIndex : TeamIndex.None);
            float num = projectileEraserRadius * projectileEraserRadius;
            
            bool result = false;
            List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
            List<ProjectileController> list = new List<ProjectileController>();

            int i = 0;
            for (int count = instancesList.Count; i < count; i++)
            {
                ProjectileController projectileController = instancesList[i];
                if (!projectileController.cannotBeDeleted && projectileController.teamFilter.teamIndex != teamIndex && (projectileController.transform.position - vector).sqrMagnitude < num)
                {
                    list.Add(projectileController);
                }
            }

            int j = 0;
            for (int count2 = list.Count; j < count2; j++)
            {
                ProjectileController projectileController2 = list[j];
                if ((bool)projectileController2)
                {
                    result = true;
                    Vector3 position = projectileController2.transform.position;
                    Vector3 start = vector;
                    //if ((bool)tracerEffectPrefab)
                    //{
                    //    EffectData effectData = new EffectData
                    //    {
                    //        origin = position,
                    //        start = start
                    //    };
                    //    EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
                    //}
                    EntityState.Destroy(projectileController2.gameObject);
                }
            }
            return result;
        }
    }
}
