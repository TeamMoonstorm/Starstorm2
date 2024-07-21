using SS2;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using HG;

namespace EntityStates.Lamp
{
    public class HealBeam : BaseSkillState
    {
        public static float baseDuration;
        private float duration;
        public static float healCoefficient = 5f;
        public static float radius = 20f;
        public static float maxBuffedEnemies = 5f;
        public static GameObject healBeamPrefab;
        public static GameObject healBeamPrefabBlue;
        //public HurtBox target;
        //private HealBeamController healBeamController;
        private SphereSearch sphereSearch;
        private List<HurtBox> hits;
        private List<HealBeamController> hbcs;
        private float lineWidthRefVelocity;
        private float currentBuffedEnemies;

        private bool isBlue;

        private float originalMoveSpeed;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayCrossfade("Body", "IdleBuff", 0.3f);
            Util.PlaySound("FollowerCast", gameObject);

            originalMoveSpeed = characterBody.moveSpeed;

            isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";

            currentBuffedEnemies = 0f;

            duration = baseDuration / attackSpeedStat;
            float healRate = healCoefficient * damageStat / duration;

            Transform transform = FindModelChild("Muzzle");
            if (NetworkServer.active)
            {
                TeamMask mask = TeamMask.none;
                mask.AddTeam(base.teamComponent.teamIndex);
                hits = new List<HurtBox>();
                sphereSearch = new SphereSearch();
                sphereSearch.mask = LayerIndex.entityPrecise.mask;
                sphereSearch.radius = radius;
                sphereSearch.origin = characterBody.corePosition;
                sphereSearch.RefreshCandidates();
                sphereSearch.FilterCandidatesByHurtBoxTeam(mask);
                sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();            
                sphereSearch.GetHurtBoxes(hits);

                hbcs = new List<HealBeamController>();

                foreach (HurtBox h in hits)
                {
                    CharacterBody body = h.healthComponent.body;
                    if (CanHeal(body) && currentBuffedEnemies < maxBuffedEnemies)
                    {
                        currentBuffedEnemies++;
                        GameObject beam = isBlue ? healBeamPrefabBlue : healBeamPrefab;
                        GameObject beamInstance = Object.Instantiate(beam, transform);
                        HealBeamController healBeamController = beamInstance.GetComponent<HealBeamController>();
                        healBeamController.healRate = healRate;
                        healBeamController.target = h;
                        healBeamController.ownership.ownerObject = gameObject;
                        hbcs.Add(healBeamController);
                        NetworkServer.Spawn(gameObject);
                        body.AddTimedBuff(SS2Content.Buffs.bdLampBuff.buffIndex, duration);
                    }
                }
            }
        }

        private bool CanHeal(CharacterBody body)
        {
            return body.healthComponent.alive && !body.hasCloakBuff && body.bodyIndex != base.characterBody.bodyIndex;
        }

        public override void OnExit()
        {
            PlayCrossfade("Body", "Idle", 0.3f);
            if (hbcs.Count > 0)
            {
                foreach (HealBeamController hbc in hbcs)
                {
                    hbc.BreakServer();
                }
            }
            characterBody.moveSpeed = originalMoveSpeed;
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.moveSpeed = originalMoveSpeed * 0.1f;
            if ((fixedAge >= duration && isAuthority))
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        /*public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            HurtBoxReference.FromHurtBox(target).Write(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            HurtBoxReference hurtBoxReference = default(HurtBoxReference);
            hurtBoxReference.Read(reader);
            GameObject gameObject = hurtBoxReference.ResolveGameObject();
            target = ((gameObject != null) ? gameObject.GetComponent<HurtBox>() : null);
        }*/
    }
}
