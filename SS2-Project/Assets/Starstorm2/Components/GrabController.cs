using EntityStates;
using RoR2;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using RoR2.Networking;
namespace Moonstorm.Starstorm2.Components
{
    //bastardized vehicleseat
    // use for anything, not just chirr

    // !!!!!! pretty jank networking atm when grabbing happens between players. check DroppedState.ReleaseLatencyFixTEMP

    // desperately needs a better solution for colliders

    ///VEHICLESEAT:
    ///doesnt handle colliders correctly,
    ///does weird interactible stuff
    ///forces passenger state changes that we dont always want
    ///can only assign passengers on server
    public class GrabController : NetworkBehaviour
    {
        public static float disableCollisionsOnExitTime = 0.2f; // we could probs just change layers temporarily, to something that only collides with world. 
                                                                // ^^ definitely should do actually. i dunno how but i just completely forgot about physics layers while making a good chunk of this

        public static bool shouldLog;
        public delegate void ModifyGrabStateDelegate(EntityStateMachine entityStateMachine, ref EntityState grabState);

        public static Dictionary<GameObject, GrabController> victimsToGrabControllers = new Dictionary<GameObject, GrabController>();

        [NonSerialized]
        [SyncVar(hook = "SetVictim")]
        public GameObject victimBodyObject;

        [NonSerialized]
        public VehicleSeat.PassengerInfo victimInfo;
        private bool victimUsedGravity;
        [NonSerialized]
        public Collider[] victimColliders = Array.Empty<Collider>();

        public event Action<VehicleSeat.PassengerInfo> onVictimGrabbed;
        public event Action<VehicleSeat.PassengerInfo> onVictimReleased;
        public ModifyGrabStateDelegate grabStateModifier;

        public Transform grabTransform; // USE CHILDLOCATOR
        public Vector3 grabOffset;

        public SerializableEntityStateType grabState = new SerializableEntityStateType(typeof(GrabbedState));
        public bool shouldForceAllyState;
        
        private Collider collider;      
        private TeamComponent teamComponent;
        private void Awake()
        {
            this.collider = base.GetComponent<Collider>();
            this.teamComponent = base.GetComponent<TeamComponent>();
        }

        private void FixedUpdate()
        {
            this.UpdateVictimPosition();
            this.ForcePassengerState();
            if (this.victimInfo.characterMotor)
            {
                this.victimInfo.characterMotor.velocity = Vector3.zero;
            }
        }

        public void UpdateVictimPosition() 
        {
            if (!this.grabTransform) return;

            Vector3 position = this.grabTransform.position;
            if (this.victimInfo.characterMotor)
            {
                this.victimInfo.characterMotor.velocity = Vector3.zero;
                this.victimInfo.characterMotor.Motor.BaseVelocity = Vector3.zero;                   
                this.victimInfo.characterMotor.Motor.SetPosition(position, false);              
            }
            else if(this.victimInfo.body && this.victimInfo.body.rigidbody)
            {
                this.victimInfo.body.rigidbody.MovePosition(position);
            }

            if (this.victimInfo.transform)
            {
                this.victimInfo.transform.position = position;
            }
        }

        public bool IsGrabbing() /////
        {
            return this.victimInfo.body;
        }

        private bool ShouldForcePassengerState()
        {
            return this.victimInfo.body && (this.victimInfo.body.teamComponent.teamIndex != this.teamComponent.teamIndex || shouldForceAllyState);
        }

        //ugliest code ever
        // might be good to force allies into a state that still reads inputs, instead of leaving them in main.
        private void ForcePassengerState()
        {
            if (this.victimInfo.bodyStateMachine && this.victimInfo.hasEffectiveAuthority)
            {
                // if victim isnt in grab state
                if (this.victimInfo.bodyStateMachine.state.GetType() != this.grabState.stateType && this.ShouldForcePassengerState())
                {
                    //and if the state is interruptible
                    //and if the state should be interrupted (including spawn state)
                    if ((this.victimInfo.bodyStateMachine.CanInterruptState(InterruptPriority.Vehicle) || this.victimInfo.bodyStateMachine.IsInInitialState()))
                    {
                        //interrupt all statemachines
                        if (shouldLog) SS2Log.Warning("GrabController.ForcePassengerState: Interrupted " + this.victimInfo.bodyStateMachine.state + " on " + this.victimBodyObject);

                        EntityState grabState = EntityStateCatalog.InstantiateState(this.grabState);
                        if(this.grabStateModifier != null) 
                        {
                            this.grabStateModifier(this.victimInfo.bodyStateMachine, ref grabState); // the fuck was i thinking? you give it your own grab state anyways...
                        }
                        this.victimInfo.bodyStateMachine.SetNextState(grabState);
                        SetStateOnHurt setStateOnHurt = this.victimInfo.body.GetComponent<SetStateOnHurt>();
                        if (setStateOnHurt)
                        {
                            foreach (EntityStateMachine machine in setStateOnHurt.idleStateMachine)
                            {
                                if (machine.CanInterruptState(InterruptPriority.Vehicle))
                                    machine.SetNextStateToMain();
                            }
                        }
                    }
                }
                // or if victim is in grab state and shouldnt be (when an enemy converts to an ally mid grab, for example) (thats the only example actually)
                else if (!ShouldForcePassengerState() && this.victimInfo.bodyStateMachine.state.GetType() == this.grabState.stateType)
                {
                    // return it to main state

                    if (shouldLog) SS2Log.Warning("GrabController.ForcePassengerState: Return to main state " + this.victimInfo.body);
                    this.victimInfo.bodyStateMachine.SetNextStateToMain();
                }

            }
        }
        private void OnDestroy()
        {
            this.SetVictim(null);
        }

        /////// CLIENT USES ATTEMPTGRAB ->
        // SetVictim() - SetVictimInternal() -> EndGrab/StartGrab
        // call CmdGrab()
        // SERVER RECIEVES COMMAND ->
        // AssignVictim() -> SetVictim() -> SetVictimInternal() -> set_victimBodyObject, starting clients SyncVarHook -> EndGrab/StartGrab
        // CLIENTS RECIEVE SYNCVAR ->
        // SetVictim() -> SetVictimInternal() -> EndGrab/StartGrab

        /////// SERVER USES ATTEMPTGRAB -> 
        // AssignVictim() -> SetVictim() -> SetVictimInternal() -> set_victimBodyObject, starting clients SyncVarHook -> EndGrab/StartGrab
        // CLIENTS RECIEVE SYNCVAR
        // SetVictim() -> SetVictimInternal() -> EndGrab/StartGrab
        public void AttemptGrab(GameObject bodyObject) //use attemptgrab(null) to release
        {
            if (NetworkServer.active) // if server, grab then send to clients
            {
                if (shouldLog) SS2Log.Info("GrabController.AttemptGrab: NetworkServer.active == true" + bodyObject);
                this.AssignVictim(bodyObject);
                return;
            }
            else if(Util.HasEffectiveAuthority(base.gameObject)) // should victims be able to release themselves under authority?
            {
                if (shouldLog) SS2Log.Info("GrabController.AttemptGrab: isAuthority == true" + bodyObject);
                this.CmdGrab(bodyObject);
            }
            else
            {
                SS2Log.Error("GrabController.AttemptGrab: AttemptGrab called on client without authority!");
                return;
            }
            if (shouldLog) SS2Log.Info("GrabController.AttemptGrab: NetworkServer.active == false" + bodyObject);
            
            this.SetVictim(bodyObject); // if client, grab then send to server, which sends it to other clients
        }

        [Command]
        public void CmdGrab(GameObject bodyObject) // DONT CALL THIS. USE ATTEMPTGRAB
        {
            if (shouldLog) SS2Log.Info("GrabController.CmdGrab " + bodyObject);
            this.AssignVictim(bodyObject);
        }
        [Server]
        public void AssignVictim(GameObject bodyObject)
        {
            if (shouldLog) SS2Log.Info("GrabController.AssignVictim " + bodyObject);
            if (bodyObject)
            {
                CharacterBody component = bodyObject.GetComponent<CharacterBody>();
                if (component && component.currentVehicle)
                {
                    component.currentVehicle.EjectPassenger(bodyObject); // ejecting since vehicles are basically the same as grabs
                }
                if(victimsToGrabControllers.TryGetValue(bodyObject, out GrabController grabController)) // force whoever is grabbing bodyObject to release it
                {
                    grabController.AssignVictim(null); // should probably make a separate method for releasing victims instead of AssignVictim(null) ?
                }
            }
            this.SetVictim(bodyObject);
        }

        // copied this part from vehicleseat. it makes sense but feels rly jank. but im also fucking idiot so whatev
        // syncvarhook of victimBodyObject
        private void SetVictim(GameObject bodyObject)
        {
            if (shouldLog) SS2Log.Info("GrabController.SetVictim " + bodyObject);
            if (base.syncVarHookGuard) // i think i understand this? stops code from running an extra time during syncvar. vanilla does this
            {
                if (shouldLog) SS2Log.Info("GrabController.SetVictim syncVarHookGuard == true");
                this.victimBodyObject = bodyObject;
                return;
            }
            if (this.victimBodyObject == bodyObject) // stops initial client from rerunning setvictiminternal, since they already set it themselves
            {
                if (shouldLog)
                {
                    Debug.Log("ReferenceEquals(passengerBodyObject, newPassengerBodyObject)==true");
                }
                return;
            }
            this.SetVictimInternal(bodyObject);
        }

        private void SetVictimInternal(GameObject bodyObject)
        {
            if (shouldLog) SS2Log.Info("GrabController.SetVictimInternal " + bodyObject);
            if (this.victimBodyObject)
            {
                this.EndGrab(this.victimBodyObject);
            }
            this.victimBodyObject = bodyObject;
            this.victimInfo = default(VehicleSeat.PassengerInfo);
            this.victimColliders = Array.Empty<Collider>();
            this.grabOffset = Vector3.zero;
            if (this.victimBodyObject)
            {
                this.StartGrab(this.victimBodyObject);
            }
        }

        private void StartGrab(GameObject victim)
        {
            if (shouldLog) SS2Log.Info("GrabController.StartGrab " + victim);

            this.victimInfo = new VehicleSeat.PassengerInfo(this.victimBodyObject);

            if(Util.HasEffectiveAuthority(base.gameObject))
            {
                CharacterNetworkTransform networkTransform = victim.GetComponent<CharacterNetworkTransform>();
                if(networkTransform) networkTransform.hasEffectiveAuthority = true; /// >:)
            }

            DisableCollisionsForDuration component = victim.GetComponent<DisableCollisionsForDuration>();
            if (component) Destroy(component); // REALLY REALLY NEED TO FIGURE OUT A BETTER SOLUTION FOR COLLIDERS

            this.victimColliders = this.victimBodyObject.GetComponentsInChildren<Collider>(); // wisps and gups and shit have other random colliders and we gotta dig to find them
            if (this.victimInfo.characterModel)
            {
                // oh lord
                Collider[] modelColliders = this.victimInfo.characterModel.gameObject.GetComponentsInChildren<Collider>().Where(c => !c.GetComponent<HurtBox>()).ToArray();
                this.victimColliders = HG.ArrayUtils.Join(modelColliders, this.victimColliders);
            }

            

            StringBuilder stringBuilder = new StringBuilder();
            if (shouldLog) SS2Log.Info("Disabling colliders on " + victim);
            foreach (Collider collider in this.victimColliders)
            {
                //if (collider.gameObject.layer != LayerIndex.entityPrecise) // DO THIS ASAP instead of shitty linq
                //{
                //    if (shouldLog) stringBuilder.Append("Disabled: ").Append(collider.name).AppendLine();
                //    collider.enabled = false;
                //}
                if (shouldLog) stringBuilder.Append("Disabled: ").Append(collider.name).AppendLine();
                collider.enabled = false;
            }
            if (shouldLog) SS2Log.Info(stringBuilder.ToString());

            this.ForcePassengerState();


            this.victimUsedGravity = true;
            if (this.victimInfo.characterMotor)
            {
                this.victimInfo.characterMotor.enabled = false;
            }
            else if(this.victimInfo.body && this.victimInfo.body.rigidbody) /// ?????????????????
            {
                GameObject prefab = BodyCatalog.GetBodyPrefab(this.victimInfo.body.bodyIndex);
                if (prefab)
                {
                    Rigidbody rigidbody = prefab.GetComponent<Rigidbody>();
                    if (rigidbody)
                    {
                        this.victimUsedGravity = rigidbody.useGravity;
                    }
                }
                this.victimInfo.body.rigidbody.useGravity = false;
            }

            if(this.victimInfo.body)
            {
                // makes it so we get the kill for throwing something off the map without damaging it first
                this.victimInfo.body.healthComponent.lastHitAttacker = base.gameObject;

                // lastHitTime is used for visuals like screen flashing. only wnat to do this for enemies
                if(FriendlyFireManager.ShouldDirectHitProceed(this.victimInfo.body.healthComponent, this.teamComponent.teamIndex))
                    this.victimInfo.body.healthComponent.lastHitTime = Run.FixedTimeStamp.now;

                // this does the same but makes the screen flash for players so no
                //this.victimInfo.body.healthComponent.UpdateLastHitTime(0, this.grabTransform.position, false, base.gameObject);
            }

            GrabController.victimsToGrabControllers.Add(victimBodyObject, this);
            if(this.onVictimGrabbed != null)
            {
                this.onVictimGrabbed(this.victimInfo);
            }

        }

        private void EndGrab(GameObject victim)
        {
            if (shouldLog) SS2Log.Info("GrabController.EndGrab " + victim);

            if (Util.HasEffectiveAuthority(base.gameObject))
            {
                CharacterNetworkTransform networkTransform = victim.GetComponent<CharacterNetworkTransform>();
                if (networkTransform) networkTransform.UpdateAuthority();
            }
            // maybe should have this be a default callback that can be overridden
            DisableCollisionsForDuration component = victim.AddComponent<DisableCollisionsForDuration>(); /////////// AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            component.collidersSelf = this.victimColliders;
            component.collidersOther = new Collider[] { this.collider };
            component.disableSelfColliders = true;
            component.timer = disableCollisionsOnExitTime;
            
            if(this.victimInfo.body && this.victimInfo.body.rigidbody)
            {
                this.victimInfo.body.rigidbody.useGravity = this.victimUsedGravity;
            }
            if (this.victimInfo.characterMotor)
            {
                this.victimInfo.characterMotor.enabled = true;
            }

            if (this.victimInfo.hasEffectiveAuthority)
            {
                if (this.victimInfo.bodyStateMachine && this.victimInfo.bodyStateMachine.state.GetType() == this.grabState.stateType)
                {
                    if (shouldLog) SS2Log.Info("GrabController.EndGrab: Setting state to main");
                    this.victimInfo.bodyStateMachine.SetNextStateToMain();                                      
                }
                //Vector3 newPosition = this.grabTransform.position; ////////////// do properly later (release transform?)
                //TeleportHelper.TeleportGameObject(this.victimInfo.transform.gameObject, newPosition); 
            }


            GrabController.victimsToGrabControllers.Remove(victimBodyObject);
            if (this.onVictimReleased != null)
            {
                this.onVictimReleased(this.victimInfo);
            }
        }

    }
}