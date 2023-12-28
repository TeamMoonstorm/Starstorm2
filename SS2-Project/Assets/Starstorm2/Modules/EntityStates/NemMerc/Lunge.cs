using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2;
namespace EntityStates.NemMerc
{
    public class Lunge : BaseSkillState
    {
        public static float maximumDuration = 0.33f;
        public static float aimVelocity = 4f;
        public static float minimumY = -0.4f;
        public static float maximumY = 0.4f;
        public static float airControl = 0.75f;
        public static float riposteDuration = 1.5f;
        public static float collisionRadius = 5f;

        public float baseSpeed;

        private float previousAirControl;
        private Vector3 direction;
        private EntityStateMachine weapon;

        private Transform collisionTransform;
        public override void OnEnter()
        {
            base.OnEnter();

            //anim
            base.PlayAnimation("FullBody, Override", "Lunge", "Secondary.playbackRate", .3f);
            //vfx
            //sound
            Util.PlaySound("Play_nemmerc_secondary_lunge", base.gameObject);
            // SHOULD USE ROOT MOTION INSTEAD of velocity 
            this.weapon = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");


            this.collisionTransform = base.FindModelChild("WhirlwindCollision");
            this.direction = base.GetAimRay().direction;

            this.previousAirControl = base.characterMotor.airControl;
            base.characterMotor.airControl = Lunge.airControl;

            if (base.isAuthority)
            {
                direction.y = Mathf.Clamp(direction.y, Lunge.minimumY, Lunge.maximumY);
                base.characterBody.isSprinting = true;
                base.characterBody.RecalculateStats(); // XD???????????
                Vector3 a = direction.normalized * Lunge.aimVelocity * base.characterBody.moveSpeed;
                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = a;
            }

            ///maybe maybe not
            //if(NetworkServer.active)
            //{
            //    base.characterBody.AddTimedBuff(SS2Content.Buffs.BuffRiposte, Lunge.riposteDuration);
            //}
            //Riposter riposter = base.gameObject.AddComponent<Riposter>();
            //riposter.duration = Lunge.riposteDuration;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

        }


        public bool CheckCollisions()
        {
            //bool hit = Util.CharacterSpherecast(this.characterBody.gameObject, new Ray(base.characterBody.corePosition, Vector3.up), Lunge.collisionRadius, out RaycastHit hitInfo,
            //    0, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);

            if(this.collisionTransform)
            {
                Transform transform = this.collisionTransform;
                Vector3 center = transform.position;
                Vector3 halfExtents = transform.lossyScale * 0.5f;
                Quaternion rotation = transform.rotation;

                Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation, LayerIndex.entityPrecise.mask);
                return ProcessHits(hits);
            }
            
            return false;
        }

        public bool ProcessHits(Collider[] hits)
        {
            foreach(Collider collider in hits)
            {
                if (collider)
                {
                    HurtBox hurtBox = collider.GetComponent<HurtBox>();
                    if (hurtBox)
                    {
                        HealthComponent healthComponent = hurtBox.healthComponent;
                        TeamMask teams = TeamMask.GetUnprotectedTeams(this.characterBody.teamComponent.teamIndex);
                        if (healthComponent && healthComponent.alive
                            && teams.HasTeam(healthComponent.body.teamComponent.teamIndex))
                        {
                            if(NetworkServer.active)
                                ForceFlinch(healthComponent.body);

                            return true;
                        }
                    }
                }
            }        
            return false;
        }

        protected virtual void ForceFlinch(CharacterBody body)
        {
            SetStateOnHurt component = body.healthComponent.GetComponent<SetStateOnHurt>();
            if (component == null)
            {
                return;
            }
            if (component.canBeHitStunned && !body.isBoss && !body.isChampion)
            {
                component.SetPain();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterDirection.forward = this.direction;

            if (this.CheckCollisions())
            {
                this.outer.SetNextStateToMain();
                if(this.weapon)
                {
                    this.weapon.SetNextState(new WhirlwindBase());
                }
                return;
            }

            if(base.fixedAge >= Lunge.maximumDuration)
            {
                this.outer.SetNextStateToMain();
                if (this.weapon)
                {
                    this.weapon.SetNextState(new WhirlwindBase());
                }
            }
        }

        public override void Update()
        {
            base.Update();
            base.characterMotor.moveDirection = this.direction;
            base.characterDirection.forward = this.direction;
        }
        public override void OnExit()
        {
            base.OnExit();

            base.characterMotor.airControl = this.previousAirControl;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public class Riposter : MonoBehaviour, IOnIncomingDamageServerReceiver
        {
            public float duration;
            private float stopwatch;
            private EntityStateMachine body;
            private void Awake()
            {
                body = EntityStateMachine.FindByCustomName(base.gameObject, "Body");
                if (!body) Destroy(this);
            }
            private void Start()
            {
                this.stopwatch = this.duration;
            }

            private void FixedUpdate()
            {
                this.stopwatch -= Time.fixedDeltaTime;
                if(this.stopwatch <= 0f)
                {
                    Destroy(this);
                }
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.attacker && this.body)
                {
                    this.body.SetInterruptState(new PrepRetaliate { damageReceived = damageInfo.damage }, InterruptPriority.PrioritySkill);
                    Destroy(this);
                }
                    
            }
        }
    }
}
