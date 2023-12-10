using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using RoR2.Networking;

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

		private Collider collider;

		private Collider[] victimColliders;

		public struct VictimCollider
        {
			public Collider collider;
			public int layer;
        }

		private void Awake()
		{
			this.collider = base.GetComponent<Collider>();
		}

		private void FixedUpdate()
		{
			this.UpdateVictimPosition();
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

		private void ForcePassengerState()
		{
			if (this.victimInfo.bodyStateMachine && this.victimInfo.hasEffectiveAuthority)
			{
				//Type type = this.passengerState.GetType();
				//if (this.victimInfo.bodyStateMachine.state.GetType() != type)
				//{
				//	this.victimInfo.bodyStateMachine.SetInterruptState(EntityStateCatalog.InstantiateState(this.passengerState), InterruptPriority.Vehicle);
				//}
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

			this.victimColliders = this.victimBodyObject.GetComponentsInChildren<Collider>(); // wisps and shit have other random colliders in children
			foreach(Collider collider in this.victimColliders)
            {
				collider.enabled = false;
            }

			if (this.victimInfo.bodyStateMachine && this.victimInfo.bodyStateMachine.CanInterruptState(InterruptPriority.Vehicle))
			{
				this.victimInfo.bodyStateMachine.SetNextState(new EntityStates.Chirr.GrabbedState());
			}
			//this.victimColliders = new VictimCollider[colliders.Length];
			//for(int i = 0; i < victimColliders.Length - 1; i++) // we cant do IgnoreCollision because KinematicCharacterMotor fucking sucks
			//         {
			//	this.victimColliders[i] = new VictimCollider { collider = colliders[i], layer = colliders[i].gameObject.layer };
			//	colliders[i].gameObject.layer = LayerIndex.noCollision.intVal;
			//         }
			//if (this.collider)
			//{
			//	foreach (VictimCollider collider in this.victimColliders)
			//	{

			//		Physics.IgnoreCollision(this.collider, collider.collider, true);
			//	}
			//}
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

			//for (int i = 0; i < victimColliders.Length - 1; i++)
			//{
			//	this.victimColliders[i].collider.gameObject.layer = this.victimColliders[i].layer;
			//}
			//if (this.collider)
			//{
			//	foreach (VictimCollider collider in this.victimColliders)
			//	{
			//		Physics.IgnoreCollision(this.collider, collider.collider, false);
			//	}
			//}


			if (this.victimInfo.characterMotor)
			{
				this.victimInfo.characterMotor.enabled = true;
			}		

			if (this.victimInfo.hasEffectiveAuthority)
			{
				if (this.victimInfo.bodyStateMachine && this.victimInfo.bodyStateMachine.CanInterruptState(InterruptPriority.Vehicle))
				{
					this.victimInfo.bodyStateMachine.SetNextStateToMain();
				}
				Vector3 newPosition = this.grabTransform.position; //////////////
				TeleportHelper.TeleportGameObject(this.victimInfo.transform.gameObject, newPosition);
			}
		}

	}
}