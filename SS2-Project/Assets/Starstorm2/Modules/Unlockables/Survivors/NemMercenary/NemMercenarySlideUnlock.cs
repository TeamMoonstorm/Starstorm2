using RoR2;
using RoR2.Achievements;
using UnityEngine;
using System;
namespace SS2.Unlocks.NemMercenary
{
    public sealed class NemMercenarySlideAchievement : BaseAchievement
    {

        private CharacterMotor motor;
        private bool failed;

        private bool characterOk;

        private ToggleAction groundCheck;

        private ToggleAction teleporterCheck;

        private bool hasEverTouchedGround = false;
        public static float gracePeriod = 0.5f;
        private float graceTimer;
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemMercBody");
        }
        private void SubscribeGroundCheck()
        {
            RoR2Application.onFixedUpdate += this.FixedUpdateCheckGrounded;
        }
        private void UnsubscribeGroundCheck()
        {
            RoR2Application.onFixedUpdate -= this.FixedUpdateCheckGrounded;
        }
        private void SubscribeTeleporterCheck()
        {
            TeleporterInteraction.onTeleporterChargedGlobal += this.CheckTeleporter;
        }
        private void UnsubscribeTeleporterCheck()
        {
            TeleporterInteraction.onTeleporterChargedGlobal -= this.CheckTeleporter;
        }
        private void OnMostRecentSceneDefChanged(SceneDef sceneDef)
        {
            this.failed = false;
            this.hasEverTouchedGround = false;
            this.graceTimer = NemMercenarySlideAchievement.gracePeriod;
        }
        private void FixedUpdateCheckGrounded()
        {
            if (!this.hasEverTouchedGround && this.motor && this.motor.isGrounded) this.hasEverTouchedGround = true;


            if (this.hasEverTouchedGround && this.motor)
            {
                if (!this.motor.isGrounded)
                {
                    this.graceTimer -= Time.fixedDeltaTime;
                    if (this.graceTimer <= 0f)
                        this.Fail();
                }
                else
                    this.graceTimer = NemMercenarySlideAchievement.gracePeriod;                
            }
        }
        private void CheckTeleporter(TeleporterInteraction teleporterInteraction)
        {
            if (this.characterOk && !this.failed)
            {
                base.Grant();
            }
        }

        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            this.characterOk = true;
        }
        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementMet();
            this.characterOk = false;
            this.Fail();
        }
        private void Fail()
        {
            this.failed = true;
            this.groundCheck.SetActive(false);
            this.teleporterCheck.SetActive(false);
        }

        private void OnBodyChanged()
        {
            if (this.characterOk && !this.failed && base.localUser.cachedBody)
            {
                this.motor = base.localUser.cachedBody.characterMotor;
                this.groundCheck.SetActive(true);
                this.teleporterCheck.SetActive(true);
            }
        }

        public override void OnInstall()
        {
            base.OnInstall();
            this.groundCheck = new ToggleAction(new Action(this.SubscribeGroundCheck), new Action(this.UnsubscribeGroundCheck));
            this.teleporterCheck = new ToggleAction(new Action(this.SubscribeTeleporterCheck), new Action(this.UnsubscribeTeleporterCheck));
            base.localUser.onBodyChanged += this.OnBodyChanged;
            SceneCatalog.onMostRecentSceneDefChanged += this.OnMostRecentSceneDefChanged;
        }

        public override void OnUninstall()
        {
            base.localUser.onBodyChanged -= this.OnBodyChanged;
            this.groundCheck.Dispose();
            this.teleporterCheck.Dispose();
            SceneCatalog.onMostRecentSceneDefChanged -= this.OnMostRecentSceneDefChanged;
            base.OnUninstall();
        }                   
    }    
}