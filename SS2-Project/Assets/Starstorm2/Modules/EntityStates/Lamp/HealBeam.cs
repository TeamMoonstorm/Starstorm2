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

            originalMoveSpeed = characterBody.moveSpeed;

            isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";

            currentBuffedEnemies = 0f;

            duration = baseDuration / attackSpeedStat;
            float healRate = healCoefficient * damageStat / duration;
            Ray aimRay = GetAimRay();
            Transform transform = FindModelChild("Muzzle");
            if (NetworkServer.active)
            {
                hits = new List<HurtBox>();
                sphereSearch = new SphereSearch();
                sphereSearch.mask = LayerIndex.entityPrecise.mask;
                sphereSearch.radius = radius;

                hits.Clear();
                sphereSearch.ClearCandidates();
                sphereSearch.origin = characterBody.corePosition;
                sphereSearch.RefreshCandidates();
                sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                sphereSearch.GetHurtBoxes(hits);

                if (hits.Count == 0)
                {
                    Debug.Log("found no one..");
                    activatorSkillSlot.AddOneStock();
                    outer.SetNextStateToMain();
                }
                else
                    Util.PlaySound("FollowerCast", gameObject);

                hbcs = new List<HealBeamController>();

                foreach (HurtBox h in hits)
                {
                    CharacterBody body = h.healthComponent.body;
                    if (!(body == null || body.teamComponent.teamIndex != characterBody.teamComponent.teamIndex || body.hasCloakBuff || body.bodyIndex == BodyCatalog.FindBodyIndex("LampBody") || currentBuffedEnemies >= maxBuffedEnemies))
                    {
                        Debug.Log("trying t buff");
                        currentBuffedEnemies++;
                        GameObject beam = isBlue ? healBeamPrefabBlue : healBeamPrefab;
                        GameObject beamInstance = Object.Instantiate(beam, transform);
                        Debug.Log("about to add hbc ");
                        HealBeamController healBeamController = beamInstance.GetComponent<HealBeamController>();
                        Debug.Log("added hbc");
                        healBeamController.healRate = healRate;
                        healBeamController.target = h;
                        healBeamController.ownership.ownerObject = gameObject;
                        Debug.Log("adding hbc to hbcs");
                        hbcs.Add(healBeamController);
                        NetworkServer.Spawn(gameObject);
                        body.AddTimedBuff(SS2Content.Buffs.bdLampBuff.buffIndex, duration);
                        Debug.Log("done");
                    }
                }

                if (hbcs.Count == 0)
                {
                    Debug.Log("wtf?");
                    activatorSkillSlot.AddOneStock();
                    outer.SetNextStateToMain();
                }

                /*BullseyeSearch bullseyeSearch = new BullseyeSearch();
                bullseyeSearch.teamMaskFilter = TeamMask.none;
                if (teamComponent)
                    bullseyeSearch.teamMaskFilter.AddTeam(teamComponent.teamIndex);
                bullseyeSearch.filterByLoS = false;
                bullseyeSearch.maxDistanceFilter = 18f;
                bullseyeSearch.maxAngleFilter = 360f;
                bullseyeSearch.searchOrigin = aimRay.origin;
                bullseyeSearch.searchDirection = aimRay.direction;
                bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
                bullseyeSearch.RefreshCandidates();
                bullseyeSearch.FilterOutGameObject(gameObject);
                target = bullseyeSearch.GetResults().FirstOrDefault();
                if (transform && target && !target.healthComponent.body.hasCloakBuff && target.healthComponent.body.bodyIndex != BodyCatalog.FindBodyIndex("LampBody"))
                {
                    Util.PlaySound("FollowerCast", gameObject);
                    GameObject beam = isBlue ? healBeamPrefabBlue : healBeamPrefab;
                    GameObject beamInstance = Object.Instantiate(beam, transform);
                    healBeamController = beamInstance.GetComponent<HealBeamController>();
                    healBeamController.healRate = healRate;
                    healBeamController.target = target;
                    healBeamController.ownership.ownerObject = gameObject;
                    NetworkServer.Spawn(gameObject);
                    target.healthComponent.body.AddTimedBuff(SS2Content.Buffs.bdLampBuff.buffIndex, duration);
                }
                else
                {
                    activatorSkillSlot.AddOneStock();
                    outer.SetNextStateToMain();
                }*/


            }
        }

        public override void OnExit()
        {
            /*if (healBeamController)
                healBeamController.BreakServer();*/
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
