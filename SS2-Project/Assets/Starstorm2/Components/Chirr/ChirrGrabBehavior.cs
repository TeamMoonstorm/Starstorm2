using EntityStates;
using RoR2;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Chirr;
using System.Collections.Generic;
namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(GrabController))]

    // adds buffs on grab and handles throwing
    public class ChirrGrabBehavior : NetworkBehaviour
    {
        public static Dictionary<BodyIndex, float> bodyToGrabDuration;

        [SystemInitializer(typeof(BodyCatalog))]
        private static void InitDictionary()
        {
            BodyIndex brother = BodyCatalog.FindBodyIndex("BrotherBody");
            BodyIndex brotherHurt = BodyCatalog.FindBodyIndex("BrotherHurtBody");
            bodyToGrabDuration = new Dictionary<BodyIndex, float>();
            bodyToGrabDuration.Add(brother, 5f);
            bodyToGrabDuration.Add(brotherHurt, 5f);
        }

        private bool hasEffectiveAuthority
        {
            get => Util.HasEffectiveAuthority(base.gameObject);
        }

        public static bool shouldLog;
        private GrabController grabController;
        private TeamComponent teamComponent;
        private CharacterBody body;
        public float buffLingerDuration = 5f;
        
        private void Awake()
        {
            this.grabController = base.GetComponent<GrabController>();
            this.teamComponent = base.GetComponent<TeamComponent>();
            this.body = base.GetComponent<CharacterBody>();
            this.grabController.onVictimGrabbed += AddBuff;
            this.grabController.onVictimReleased += RemoveBuff;
            this.grabController.grabStateModifier += ModifyGrabState;
        }

        private void ModifyGrabState(EntityStateMachine bodyMachine, ref EntityState grabState) // ???????????????????????????????????
        {
            GrabbedState grabbedState = grabState as GrabbedState;
            if(grabbedState != null)
            {
                grabbedState.inflictor = this.grabController;
                grabbedState.duration = GetGrabDurationFromBody(bodyMachine.GetComponent<CharacterBody>().bodyIndex);
            }
        }
        private float GetGrabDurationFromBody(BodyIndex bodyIndex)
        {
            return bodyToGrabDuration.TryGetValue(bodyIndex, out float duration) ? duration : Mathf.Infinity;
        }
        private void RemoveBuff(VehicleSeat.PassengerInfo info)
        {
            if (!NetworkServer.active) return;

            if(info.body.HasBuff(SS2Content.Buffs.BuffChirrGrabFriend))
                info.body.RemoveBuff(SS2Content.Buffs.BuffChirrGrabFriend);

            if (info.body && info.body.teamComponent.teamIndex == this.teamComponent.teamIndex)
            {                
                info.body.AddTimedBuff(SS2Content.Buffs.BuffChirrGrabFriend, buffLingerDuration);
            }
        }

        private void AddBuff(VehicleSeat.PassengerInfo info)
        {        
            if (info.body && info.body.teamComponent.teamIndex == this.teamComponent.teamIndex)
            {
                if (this.body) this.body.outOfCombatStopwatch = 0f;

                if(NetworkServer.active)
                    info.body.AddBuff(SS2Content.Buffs.BuffChirrGrabFriend);
            }
        }



        // setstateonhurt code kinda
        // server tells clients to throw the victim, client with authority over the victim sets it to DroppedState
        // definitely just need to do this somehow in GrabController.EndGrab. this is a fucking nightmare timing wise
        [Server]
        public void ThrowVictim(GameObject victim, Vector3 velocity, float extraGravity, bool friendly)
        {
            if (Util.HasEffectiveAuthority(victim)) // dont do RPC if we already have authority
            {
                if (shouldLog) SS2Log.Info("ChirrGrabBehavior.ThrowVictim: victimInfo.hasEffectiveAuthority == true");
                this.ThrowVictimInternal(victim, velocity, extraGravity, friendly);              
                return;
            }
            if (shouldLog) SS2Log.Info("ChirrGrabBehavior.ThrowVictim: victimInfo.hasEffectiveAuthority == false");
            this.RpcThrowVictim(victim, velocity, extraGravity, friendly); // if no authority, find the client who has it
        }

        [ClientRpc] // runs on all clients. only client with authority over the victim calls ThrowVictimInternal
        private void RpcThrowVictim(GameObject victim, Vector3 velocity, float extraGravity, bool friendly)
        {
            if (shouldLog) SS2Log.Info("ChirrGrabBehavior.RpcThrowVictim " + velocity);
            if (Util.HasEffectiveAuthority(victim))
            {
                if (shouldLog) SS2Log.Info("ChirrGrabBehavior.RpcThrowVictim: victimInfo.hasEffectiveAuthority == true");
                this.ThrowVictimInternal(victim, velocity, extraGravity, friendly);
            }
        }

        private void ThrowVictimInternal(GameObject victim, Vector3 velocity, float extraGravity, bool friendly)
        {
            if (shouldLog) SS2Log.Info("ChirrGrabBehavior.ThrowVictimInternal " + velocity);
            GameObject bodyObject = victim;
            EntityStateMachine bodyMachine = EntityStateMachine.FindByCustomName(bodyObject, "Body");
            
            if (bodyMachine)
            {
                //BaseCharacterMain calls RootMotionAccumulator.ExtractRootMotion and adds it to CharacterMotor.rootMotion before exiting the state
                //This causes anything that uses rootmotion to move unexpectedly if we change the state but want the body to stay still
                //in this case, allies with rootmotion would suddenly jerk in the direction they were moving when thrown
                //enemies would not, because they are transitioning from GrabbedState instead of BaseCharacterMain
                // &^^^^^^^^i spent fucking ages thinking this was a collider issue. fuck my life.

                // ^^ a better solution than this would be to force allies to a state that still reads inputs, instead of their main state.
                BaseCharacterMain dicks = bodyMachine.state as BaseCharacterMain;
                if(dicks != null)
                {
                    if (dicks.hasRootMotionAccumulator) dicks.rootMotionAccumulator.accumulatedRootMotion = Vector3.zero;
                    if (dicks.hasCharacterMotor) dicks.characterMotor.rootMotion = Vector3.zero;
                    if (dicks.hasRailMotor) dicks.railMotor.rootMotion = Vector3.zero;
                }

                bodyMachine.SetInterruptState(new DroppedState
                {
                    initialVelocity = velocity,
                    inflictor = base.gameObject,
                    friendlyDrop = friendly,
                    extraGravity = extraGravity
                },
                InterruptPriority.Vehicle);
            }
            // POTS AND SHIT DONT HAVE STATEMACHINES
            else
            {
                ForceDetonateOnImpact component = bodyObject.AddComponent<ForceDetonateOnImpact>();
                component.initialVelocity = velocity;
                component.inflictor = base.gameObject;
            }
        }
    }


    // scuffed copy of DroppedState for stuff without statemachines.
    // *might* be a good idea to have everything use this component instead?
    public class ForceDetonateOnImpact : MonoBehaviour
    {
        public GameObject inflictor;
        public Vector3 initialVelocity;

        private CharacterBody characterBody;
        private bool bodyHadGravity;
        private bool bodyWasKinematic;
        private Rigidbody rigidbody;
        private bool tempRigidbody;
        private SphereCollider tempSphereCollider;
        private void Awake()
        {
            this.characterBody = base.GetComponent<CharacterBody>();

            if (!this.characterBody) return;

            GameObject prefab = BodyCatalog.GetBodyPrefab(this.characterBody.bodyIndex);
            if (prefab)
            {
                Rigidbody rigidbody = prefab.GetComponent<Rigidbody>();
                if (rigidbody)
                {
                    this.bodyHadGravity = rigidbody.useGravity;
                    this.bodyWasKinematic = rigidbody.isKinematic;
                }
            }       
        }
        private void Start()
        {
            this.rigidbody = base.GetComponent<Rigidbody>();
            if (!rigidbody)
            {
                rigidbody = base.gameObject.AddComponent<Rigidbody>();
                this.tempRigidbody = true;
                this.tempSphereCollider = base.gameObject.AddComponent<SphereCollider>();
            }
            rigidbody.velocity = initialVelocity;
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerIndex.world.intVal || collision.gameObject.layer == LayerIndex.entityPrecise.intVal)
            {

                Util.PlaySound("ChirrThrowHitGround", base.gameObject);
                if (NetworkServer.active)
                {
                    EffectManager.SpawnEffect(DroppedState.hitGroundEffect, new EffectData
                    {
                        origin = this.characterBody.footPosition,
                        scale = 1.25f,
                    }, true);


                    if (this.inflictor)
                    {
                        CharacterBody inflictorBody = this.inflictor.GetComponent<CharacterBody>();
                        float damageStat = inflictorBody ? inflictorBody.damage : 12f;

                        BlastAttack blastAttack = new BlastAttack();
                        blastAttack.position = this.characterBody.footPosition;
                        blastAttack.baseDamage = AimDrop.damageCoefficient * damageStat;
                        blastAttack.baseForce = DroppedState.force;
                        blastAttack.bonusForce = Vector3.up * DroppedState.bounceForce;
                        blastAttack.radius = DroppedState.blastRadius;
                        blastAttack.attacker = this.inflictor;
                        blastAttack.inflictor = this.inflictor;
                        blastAttack.teamIndex = inflictorBody.teamComponent.teamIndex;
                        blastAttack.crit = inflictorBody.RollCrit();
                        blastAttack.procChainMask = default(ProcChainMask);
                        blastAttack.procCoefficient = DroppedState.procCoefficient;
                        blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
                        blastAttack.damageColorIndex = DamageColorIndex.Default;
                        blastAttack.damageType = DamageType.Stun1s;
                        blastAttack.attackerFiltering = AttackerFiltering.Default;
                        //blastAttack.impactEffect = 
                        BlastAttack.Result result = blastAttack.Fire();

                    };
                }
                if (this.rigidbody)
                {
                    this.rigidbody.useGravity = bodyHadGravity;
                    this.rigidbody.isKinematic = bodyWasKinematic;
                }
                if (this.tempRigidbody) Destroy(this.rigidbody);
                if (this.tempSphereCollider) Destroy(this.tempSphereCollider);

                Destroy(this);
            }
        }
    }
}
