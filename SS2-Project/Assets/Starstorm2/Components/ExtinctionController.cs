using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(GenericOwnership))]
    [RequireComponent(typeof(NetworkIdentity))]
    [RequireComponent(typeof(HitBoxGroup))]
    [RequireComponent(typeof(NetworkStateMachine))]
    [RequireComponent(typeof(EntityStateMachine))]
    public class ExtinctionController : NetworkBehaviour
    {
        private protected TeamFilter teamFilter;
        private protected SimpleLeash simpleLeash;
        private protected GenericOwnership genericOwnership;
        private protected RadialForce RadialForce;

        private CharacterBody cachedOwnerBody;

        private float extraCharge = 0f;
        private float extraChargeDecayRate = 0.2f;
        private float visualSpin = 0f;

        [Tooltip("Root of the display, anything below this should not contain game logic.")]
        public Transform extinctionDisplayRoot;

        [Tooltip("Inner ring, will get bigger at a quicker pace than the rest of the prefab.")]
        public Transform ringIndicator;

        [Tooltip("Speed rate at which the displayRoot will spin.")]
        public float visualSpinRate = 1200f;

        public float charge { get; private set; }

        public bool showExtinctionDisplay = true;

        private void Awake()
        {
            this.teamFilter = base.GetComponent<TeamFilter>();
            this.genericOwnership = base.GetComponent<GenericOwnership>();
            this.RadialForce = base.GetComponent<RadialForce>();
            this.genericOwnership.onOwnerChanged += this.OnOwnerChanged;
        }

        private void OnEnable()
        {
            if (NetworkServer.active)
            {
                GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobalServer;
            }
        }

        private void Start()
        {
            //Util.PlaySound(this.soundLoopString, base.gameObject); //do uh, whatever here
        }

        private void Update()
        {
            if (NetworkClient.active)
            {
                this.UpdateClient();
            }
        }

        public void FixedUpdate()
        {
            if (this.extinctionDisplayRoot)
            {
                this.extinctionDisplayRoot.gameObject.SetActive(this.showExtinctionDisplay);
            }
            if (extraCharge > 0)
                this.extraCharge -= this.extraChargeDecayRate * Time.deltaTime;
            else
                extraCharge = 0;
            this.charge = GetControllerCharge();

            float num = HGMath.CircleAreaToRadius((this.charge + 4.5f) * HGMath.CircleRadiusToArea(1f));
            this.RadialForce.radius = num;
        }

        private void OnDisable()
        {
            GlobalEventManager.onCharacterDeathGlobal -= this.OnCharacterDeathGlobalServer;
        }

        private void OnCharacterDeathGlobalServer(DamageReport obj)
        {
            if (obj.attacker == genericOwnership.ownerObject && obj.attacker != null && obj.damageInfo.inflictor == this)
            {
                this.extraCharge += 5f;
            }
        }

        private void OnOwnerChanged(GameObject newOwner)
        {
            this.cachedOwnerBody = (newOwner ? newOwner.GetComponent<CharacterBody>() : null);
            teamFilter.teamIndex = cachedOwnerBody.teamComponent.teamIndex;
        }


        public float GetControllerCharge()
        {
            return Mathf.Max(cachedOwnerBody.inventory.GetItemCount(SS2Content.Items.RelicOfExtinction) + extraCharge, 0);
        }

        [Client]
        private void UpdateClient()
        {
            if (!NetworkClient.active)
            {
                Debug.LogWarning("[Client] function 'System.Void Starstorm2.ExtinctionComponent::UpdateClient()' called on server");
                return;
            }
            this.visualSpin = this.charge;
            //Scale up
            float num = HGMath.CircleAreaToRadius(this.visualSpin * HGMath.CircleRadiusToArea(1f));
            this.ringIndicator.localScale = new Vector3(num / 2, num / 2, num / 2);
            this.gameObject.transform.localScale = new Vector3(num, num, num);

            //Spin Up
            Vector3 localEulerAngles = this.ringIndicator.localEulerAngles;
            localEulerAngles.y += this.visualSpin * Time.deltaTime * this.visualSpinRate;
            this.ringIndicator.localEulerAngles = localEulerAngles;

            //AkSoundEngine.SetRTPCValue(this.spinRtpc, this.visualSpin * this.spinRtpcScale, base.gameObject);
        }
    }
}