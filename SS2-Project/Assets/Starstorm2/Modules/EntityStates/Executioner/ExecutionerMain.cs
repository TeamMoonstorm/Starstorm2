//TODO: should check that secondary is ion gun before attempting to store/load charges

namespace EntityStates.Executioner
{
    /* public class ExecutionerMain : GenericCharacterMain
     {
         private Animator animator;
         private GenericSkill ionGunSkill;
         private IonGunChargeComponent storedChargeComp;

         public override void OnEnter()
         {
             base.OnEnter();
             animator = GetModelAnimator();

             //set up ion gun stock system
             ionGunSkill = skillLocator.secondary;
             storedChargeComp = GetComponent<IonGunChargeComponent>();
             if (ionGunSkill && storedChargeComp)
                 ionGunSkill.stock = storedChargeComp.storedCharges;

             //GlobalEventManager.onCharacterDeathGlobal += OnKillHandler;
         }

         public override void OnExit()
         {
             //GlobalEventManager.onCharacterDeathGlobal -= OnKillHandler;

             if (ionGunSkill && storedChargeComp)
                 storedChargeComp.storedCharges = ionGunSkill.stock;

             base.OnExit();
         }

         public override void FixedUpdate()
         {
             base.FixedUpdate();

             // rest idle!!
             if (animator) animator.SetBool("inCombat", !characterBody.outOfCombat || !characterBody.outOfDanger);
         }

         public override void Update()
         {
             base.Update();

             if (isAuthority && characterMotor.isGrounded)
             {
                 if (Input.GetKeyDown(Config.restKeybind))
                 {
                     outer.SetInterruptState(new Emotes.RestEmote(), InterruptPriority.Any);
                     return;
                 }
                 else if (Input.GetKeyDown(Config.tauntKeybind))
                 {
                     outer.SetInterruptState(new Emotes.TauntEmote(), InterruptPriority.Any);
                     return;
                 }
             }
         }
     }*/
}