using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Projectile;
using RoR2;
using RoR2.Skills;
using System;
namespace Moonstorm.Starstorm2.Components
{
    public class TeleporterProjectile : MonoBehaviour
    {
        private ProjectileController controller;
        private GameObject owner;
        private ProjectileTeleporterOwnership ownership;
        void Start()
        {
            this.controller = base.GetComponent<ProjectileController>();
            this.owner = this.controller.owner;

            if(this.owner)
            {
                //this.ownership = this.owner.GetComponent<ProjectileTeleporterOwnership>(); 
                if(!this.ownership)
                {
                    this.ownership = this.owner.AddComponent<ProjectileTeleporterOwnership>();
                    this.ownership.teleporter = this;
                }
            }
        }
        public Vector3 GetSafeTeleportPosition()
        {
            //lol
            return base.transform.position;
        }
        public void OnTeleport()
        {
            //vfx
            //sound
            //teleporter anim??

            //Destroy(base.gameObject);
        }

        public class ProjectileTeleporterOwnership : MonoBehaviour
        {
            public TeleporterProjectile teleporter;
            private SkillDef teleportSkillDef;
            private CharacterBody body;
            private SkillLocator skillLocator;

            public static bool destroyOnFirstTeleport = false;
            private void Awake()
            {
                this.body = base.GetComponent<CharacterBody>();

                this.teleportSkillDef = SS2Assets.LoadAsset<SkillDef>("Cyborg2Teleport", SS2Bundle.Indev);
                if(this.body)
                {
                    this.skillLocator = body.skillLocator;
                    this.skillLocator.utility.SetSkillOverride(this, teleportSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }

            public void DoTeleport()
            {
                this.teleporter.OnTeleport();

                if(destroyOnFirstTeleport)
                    this.UnsetOverride();
            }

            public void UnsetOverride()
            {
                if(this.skillLocator)
                {
                    this.skillLocator.utility.UnsetSkillOverride(this, teleportSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                Destroy(this);
            }

            private void FixedUpdate()
            {
                if(!this.teleporter)
                {
                    this.UnsetOverride();
                }
            }
        }
    }
}

