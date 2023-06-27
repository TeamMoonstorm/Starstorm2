using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Projectile;
using RoR2;
using RoR2.Skills;
using System;
namespace Moonstorm.Starstorm2.Components
{
    public class ProjectileTeleporter : MonoBehaviour
    {
        public static SkillDef teleportSkillDef;

        private ProjectileController controller;
        private GameObject owner;
        private ProjectileTeleporterOwnership ownership;
        void Start()
        {
            this.controller = base.GetComponent<ProjectileController>();
            this.owner = this.controller.owner;

            if(this.owner)
            {
                this.ownership = this.owner.GetComponent<ProjectileTeleporterOwnership>();
                if(!this.ownership)
                {
                    this.ownership = this.owner.AddComponent<ProjectileTeleporterOwnership>();
                }
            }
        }
        public Vector3 GetSafeTeleportPosition()
        {
            //lol
            return base.transform.position;
        }

        public class ProjectileTeleporterOwnership : MonoBehaviour
        {
            public ProjectileTeleporter teleporter;
            private CharacterBody body;
            private SkillLocator skillLocator;
            private void Awake()
            {
                this.body = base.GetComponent<CharacterBody>();
                if(this.body)
                {
                    this.skillLocator = body.skillLocator;
                    this.skillLocator.utility.SetSkillOverride(this, ProjectileTeleporter.teleportSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
            public void UnsetOverride()
            {
                if(this.skillLocator)
                {
                    this.skillLocator.utility.UnsetSkillOverride(this, ProjectileTeleporter.teleportSkillDef, GenericSkill.SkillOverridePriority.Contextual);
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

