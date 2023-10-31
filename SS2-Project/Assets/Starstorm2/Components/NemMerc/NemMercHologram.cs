using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using EntityStates;
using EntityStates.NemMerc;
using UnityEngine.Networking;
using RoR2.CharacterAI;

namespace Moonstorm.Starstorm2.Components
{
    public class NemMercHologram : NetworkBehaviour
    {
        [NonSerialized]
        public static DeployableSlot deployableSlot = Survivors.NemMerc.hologram;
        [NonSerialized]
        public float lifetime;


        public GameObject owner
        {
            get => _owner;
            set
            {
                if (value != _owner)
                    dirty = true;
                _owner = value;
            }
        }
        private GameObject _owner;

        public HurtBox target
        {
            get => _target;
            set
            {
                if (value != _target)
                    dirty = true;
                _target = value;
            }
        }
        private HurtBox _target;
        private bool dirty;

        public TeamFilter teamFilter;

        public Transform indicatorStartTransform;
        public Transform indicatorTransform;
        public GameObject lineRenderer;
        public GameObject hologramIndicatorPrefab;

        public float targetSearchRadius = 25f;
        public float targetLostDistance = 35f;
        public float ownerCollisionRadius = 4f;


        private float searchFrequency = 6;
        private float searchStopwatch;

        private PositionIndicator indicator;
        private float stopwatch;
        private bool isRevealed;
        private SphereSearch search = new SphereSearch();
        private EntityStateMachine ownerBodyMachine;


        private BaseAI ownerAI;

        private void Start()
        {
            

            this.teamFilter = base.GetComponent<TeamFilter>();

            if (this.owner)
            {
                if(this.ShouldShowIndicator())
                {
                    this.indicator = GameObject.Instantiate(this.hologramIndicatorPrefab, base.transform.position, Quaternion.identity).GetComponent<PositionIndicator>();
                    this.indicator.targetTransform = this.indicatorStartTransform;
                }        
                CharacterBody component = owner.GetComponent<CharacterBody>();
                if (component)
                {
                    CharacterMaster master = component.master;
                    if (master)
                    {
                        if (NetworkServer.active)
                            master.AddDeployable(base.GetComponent<Deployable>(), Survivors.NemMerc.hologram);

                        this.ownerAI = component.master.GetComponent<BaseAI>();
                        if (this.teamFilter) this.teamFilter.teamIndex = master.teamIndex;
                    }
                }
                this.ownerBodyMachine = EntityStateMachine.FindByCustomName(this.owner, "Body");
            }
        }

        private void CheckNewTargetOnDamage(DamageReport damageReport)
        {
            if (!damageReport.victim || !damageReport.attacker) return;

            if(damageReport.attacker == this.owner && (R2API.DamageAPI.HasModdedDamageType(damageReport.damageInfo, DamageTypes.RedirectHologram.damageType)))
            {
                Vector3 between = damageReport.damageInfo.position - base.transform.position;
                if(between.magnitude <= this.targetSearchRadius)
                {                    
                    this.target = damageReport.victim.body.mainHurtBox;
                }
            }
        }

        private void FixedUpdate()
        {
            this.stopwatch += Time.fixedDeltaTime;

            if (NetworkServer.active && stopwatch >= lifetime)
            {
                Destroy(base.gameObject);
            }

            if(this.OwnerInCollisionRadius())
            {
                if (ownerBodyMachine && ownerBodyMachine.SetInterruptState(new NemAssaulter { target = this.target }, InterruptPriority.Pain))
                {
                    Destroy(base.gameObject);
                }
            }

            if(this.ShouldUpdateTarget())
            {
                this.target = null;
                this.searchStopwatch -= Time.fixedDeltaTime;
                if (this.searchStopwatch <= 0)
                {
                    this.searchStopwatch += 1 / this.searchFrequency;
                    this.SearchForTarget();
                }
            }

            if (this.ownerAI && this.target)
            {
                this.ownerAI.customTarget.gameObject = base.gameObject;              
            }


            if(this.indicator)
            {
                this.indicator.yOffset = 0; // XD????
                this.indicator.targetTransform = base.transform;
            }
            

        }


        // NEED TO GO BACK TO COLLIDERS SO U CAN USE ALLY HOLOGRAMS
        private bool OwnerInCollisionRadius()
        {
            return this.owner && (this.owner.transform.position - base.transform.position).magnitude <= this.ownerCollisionRadius;
        }

        private bool ShouldUpdateTarget()
        {
            if (!this.target) return true;
            if (!this.target.healthComponent.alive) return true;
            if ((this.target.transform.position - base.transform.position).magnitude > this.targetLostDistance) return true;
            return false;
        }

        private void OnDestroy()
        {
            if (this.indicator) Destroy(this.indicator.gameObject);
        }

        public void OnEnable()
        {
            GlobalEventManager.onServerDamageDealt += CheckNewTargetOnDamage;
        }
        public void OnDisable()
        {
            GlobalEventManager.onServerDamageDealt -= CheckNewTargetOnDamage;
        }


        // im supposed to do bit shit here but idk how it works :^)

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            writer.Write(this.owner);
            writer.Write(HurtBoxReference.FromHurtBox(this.target));
            return dirty;
        }
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            this.owner = reader.ReadGameObject();
            this.target = reader.ReadHurtBoxReference().ResolveHurtBox();
        }

        private void SearchForTarget()
        {
            TeamMask mask = TeamMask.GetEnemyTeams(TeamComponent.GetObjectTeam(this.owner));
            this.search.origin = base.transform.position;
            this.search.radius = this.targetSearchRadius;
            this.search.mask = LayerIndex.entityPrecise.mask;
            this.search.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
            this.target = this.search.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().FirstOrDefault();
        }

        private void Update()
        {           
            this.UpdateVisualizer();
        }

        //  NEED TO CONDITIONALLY SHOW POSITION INDICATOR
        //  YES IF ENEMY NEMMERC
        //  NO IF ALLY NEMMERC (UNLESS YOU ARE ALSO NEMMERC (ONLY ONCE ALLY NEMMERCS CAN USE EACHOTHERS HOLOGRAMS))
        private bool ShouldShowIndicator()
        {
            return Util.HasEffectiveAuthority(this.owner);// || this.teamFilter.teamIndex != CharacterMaster.readOnlyInstancesList[0].teamIndex;
        }
        public void UpdateVisualizer()
        {
            bool shouldShow = this.target; // && ShouldShowIndicator();
            if (this.target != this.lineRenderer.activeSelf)
            {
                this.lineRenderer.SetActive(shouldShow);
                this.indicatorStartTransform.gameObject.SetActive(shouldShow);
            }
            if (!shouldShow) return;

            this.indicatorTransform.position = this.target.transform.position;
            this.indicatorStartTransform.position = base.transform.position;
        }



    }
}
