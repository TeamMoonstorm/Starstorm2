using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using RoR2.Networking;
using System.Text;
using System.Linq;
namespace Moonstorm.Starstorm2.Components
{

	//bastardized vehicleseat

	///VEHICLESEAT:
	///doesnt handle colliders correctly,
	///does weird interactible stuff
	///forces passenger state changes that we dont always want
	///can only assign passengers on server
	public class GrabController : NetworkBehaviour
	{
		public static bool shouldLog;

		[NonSerialized]
		[SyncVar(hook = "SetVictim")]
		public GameObject victimBodyObject;

		[NonSerialized]
		public VehicleSeat.PassengerInfo victimInfo;

		public Transform grabTransform; // USE CHILDLOCATOR

		public SerializableEntityStateType grabState;

		public bool shouldForceAllyState;

		private Collider collider;

		private Collider[] victimColliders;

		public struct VictimCollider
        {
			public Collider collider;
			public int layer;
        }

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
			Vector3 position = this.grabTransform.position;
			if (this.victimInfo.characterMotor)
			{
				this.victimInfo.characterMotor.velocity = Vector3.zero;
				this.victimInfo.characterMotor.Motor.BaseVelocity = Vector3.zero;
				this.victimInfo.characterMotor.Motor.SetPosition(position, true);
			}
			else if (this.victimInfo.transform)
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
		private void ForcePassengerState()
		{
			if (this.victimInfo.bodyStateMachine && this.victimInfo.hasEffectiveAuthority)
			{
				// if victim isnt in grab state
				if (this.victimInfo.bodyStateMachine.state.GetType() != typeof(GrabbedState) && this.ShouldForcePassengerState()) 
                {
					//and if the state is interruptible
					//and if the state should be interrupted
					if ((this.victimInfo.bodyStateMachine.CanInterruptState(InterruptPriority.Vehicle) || this.victimInfo.bodyStateMachine.IsInInitialState()))
					{
						//interrupt all statemachines
						this.victimInfo.bodyStateMachine.SetNextState(new GrabbedState());
						SetStateOnHurt setStateOnHurt = this.victimInfo.body.GetComponent<SetStateOnHurt>();
						if(setStateOnHurt)
                        {
							foreach (EntityStateMachine machine in setStateOnHurt.idleStateMachine)
							{
								if (machine.CanInterruptState(InterruptPriority.Vehicle))
									machine.SetNextStateToMain();
							}
						}				
					}
				}
				// or if victim is in grab state and shouldnt be (when an enemy converts to an ally mid grab, for example)
				else
                {
					// return it to main state
					this.victimInfo.bodyStateMachine.SetNextStateToMain();
                }
				
			}
		}

        private void OnDestroy()
        {
			this.SetVictim(null);
        }

        public void AttemptGrab(GameObject bodyObject)
        {
			if(NetworkServer.active)
            {
				if (shouldLog) SS2Log.Info("GrabController.AttemptGrab: NetworkServer.active == true" + bodyObject);
				this.AssignVictim(bodyObject);
				return;
            }
			if (shouldLog) SS2Log.Info("GrabController.AttemptGrab: NetworkServer.active == false" + bodyObject);
			this.CmdGrab(bodyObject);
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
					component.currentVehicle.EjectPassenger(bodyObject);
				}
			}
			this.SetVictim(bodyObject);
		}

		private void SetVictim(GameObject bodyObject)
        {
			if (shouldLog) SS2Log.Info("GrabController.SetVictim " + bodyObject);
			if(base.syncVarHookGuard) // i think i understand this? stops code from running an extra time during syncvar. vanilla does this
            {
				if (shouldLog) SS2Log.Info("GrabController.SetVictim syncVarHookGuard == true");
				this.victimBodyObject = bodyObject;
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
			if(this.victimBodyObject)
            {
				this.StartGrab(this.victimBodyObject);
            }
		}

		private void StartGrab(GameObject victim)
		{
			if (shouldLog) SS2Log.Info("GrabController.StartGrab " + victim);

			this.victimInfo = new VehicleSeat.PassengerInfo(this.victimBodyObject);

			
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
				if(shouldLog) stringBuilder.Append("Disabled: ").Append(collider.name).AppendLine();
				collider.enabled = false;
            }
			if (shouldLog) SS2Log.Info(stringBuilder.ToString());

			this.ForcePassengerState();

			if (this.victimInfo.characterMotor)
			{
				this.victimInfo.characterMotor.enabled = false;
			}
			
		}

		private void EndGrab(GameObject victim)
		{
			if (shouldLog) SS2Log.Info("GrabController.EndGrab " + victim);

			foreach (Collider collider in this.victimColliders)
			{
				collider.enabled = true;
			}

			if (this.victimInfo.characterMotor)
			{
				this.victimInfo.characterMotor.enabled = true;
			}		

			if (this.victimInfo.hasEffectiveAuthority)
			{
				if (this.victimInfo.bodyStateMachine && (this.victimInfo.bodyStateMachine.CanInterruptState(InterruptPriority.Vehicle)))
				{
					this.victimInfo.bodyStateMachine.SetNextStateToMain();
				}
				Vector3 newPosition = this.grabTransform.position; ////////////// do properly later
				TeleportHelper.TeleportGameObject(this.victimInfo.transform.gameObject, newPosition);
			}
		}

	}
}