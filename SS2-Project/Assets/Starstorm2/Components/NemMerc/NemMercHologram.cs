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
namespace Moonstorm.Starstorm2.Components
{
    public class NemMercHologram : NetworkBehaviour
    {
        [NonSerialized]
        public static bool lockDash;

        [NonSerialized]
        public float timeUntilReveal;
        [NonSerialized]
        public float lifetime;
        [NonSerialized]
        public GameObject owner;

        [NonSerialized]
        public HurtBox target;

        public Transform indicatorStartTransform;
        public Transform indicatorTransform;
        public GameObject lineRenderer;
        public GameObject hologramIndicatorPrefab;

        public SphereCollider collider;
        public GameObject model;
        public float targetSearchRadius = 25f;
        public float targetLostDistance = 35f;
        private float searchFrequency;
        private float searchStopwatch;

        private Indicator indicator;
        private float stopwatch;
        private bool isRevealed;
        private SphereSearch search = new SphereSearch();


        private void Start()
        {
            this.indicator = new Indicator(this.owner, this.hologramIndicatorPrefab);
            this.indicator.targetTransform = base.transform;
            this.indicator.active = true;
            // could use an offscreen indicator
        }


        /// <summary>
        /// 
        /// 
        /// 
        // / NETWORK THE FUCKING TARGET DIPSHIT ITS NOT HARD
        /// 
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="damageReport"></param>
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
            if (this.stopwatch >= timeUntilReveal && !isRevealed)
            {
                isRevealed = true;
                if (this.model) this.model.SetActive(true);
                if (this.collider) this.collider.enabled = true;
            }
            if (stopwatch >= lifetime)
            {
                Destroy(base.gameObject);
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

            this.indicator.targetTransform = base.transform;

        }

        private bool ShouldUpdateTarget()
        {
            if (!this.target) return true;
            if (!this.target.healthComponent.alive) return true;
            if ((this.target.transform.position - base.transform.position).magnitude > this.targetLostDistance) return true;
            return false;
        }

        public void OnEnable()
        {
            GlobalEventManager.onServerDamageDealt += CheckNewTargetOnDamage;
            if (this.indicator != null)
                this.indicator.active = true;
        }
        public void OnDisable()
        {
            GlobalEventManager.onServerDamageDealt -= CheckNewTargetOnDamage;
            if (this.indicator != null)
                this.indicator.active = false;
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


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != this.owner) return;

            GameObject bodyObject = other.gameObject;
            HurtBox hurtBox = other.GetComponent<HurtBox>();
            if(hurtBox && hurtBox.healthComponent)
            {
                bodyObject = hurtBox.healthComponent.gameObject;
            }

            Vector3 direction = Vector3.zero;
            if (this.target) direction = (this.target.transform.position - base.transform.position).normalized;
            EntityStateMachine body = EntityStateMachine.FindByCustomName(bodyObject, "Body");
            if (body && body.SetInterruptState(new NemAssaulter { dashVector = lockDash ? direction : Vector3.zero }, InterruptPriority.Pain))
            {
                Destroy(base.gameObject);
            }        
        }

        private void Update()
        {
            if(Util.HasEffectiveAuthority(this.owner))
                this.UpdateVisualizer();
        }
        public void UpdateVisualizer()
        {
            if (this.target != this.lineRenderer.activeSelf)
            {
                this.lineRenderer.SetActive(this.target);
                this.indicatorStartTransform.gameObject.SetActive(this.target);
            }
            if (!this.target) return;

            this.indicatorTransform.position = this.target.transform.position;
            this.indicatorStartTransform.position = base.transform.position;
        }



    }
}
