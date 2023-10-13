using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Projectile;
using RoR2;
using RoR2.Skills;
using System;
namespace Moonstorm.Starstorm2.Components
{
    public class ProjectileRemoteDetonator : MonoBehaviour
    {
        private ProjectileController controller;
        private ProjectileExplosion explosion;
        private GameObject owner;
        private RemoteDetonatorOwnership ownership;
        void Start()
        {
            this.controller = base.GetComponent<ProjectileController>();
            this.owner = this.controller.owner;
            this.explosion = base.GetComponent<ProjectileExplosion>();

            if (this.owner)
            {
                //this.ownership = this.owner.GetComponent<ProjectileTeleporterOwnership>(); 
                if (!this.ownership)
                {
                    this.ownership = this.owner.AddComponent<RemoteDetonatorOwnership>();
                    this.ownership.projectile = this;
                }
            }
        }
        public Vector3 GetSafeTeleportPosition()
        {
            //lol
            return base.transform.position;
        }
        public void OnDetonate()
        {
            if(this.explosion)
            {
                this.explosion.Detonate();
            }
        }

        public class RemoteDetonatorOwnership : MonoBehaviour
        {
            public ProjectileRemoteDetonator projectile;
            private SkillDef detonateSkillDef;
            private CharacterBody body;
            private SkillLocator skillLocator;
            private void Awake()
            {
                this.body = base.GetComponent<CharacterBody>();

                this.detonateSkillDef = SS2Assets.LoadAsset<SkillDef>("Cyborg2Detonate", SS2Bundle.Indev);
                if (this.body)
                {
                    this.skillLocator = body.skillLocator;
                    this.skillLocator.special.SetSkillOverride(this, detonateSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }

            public void Detonate()
            {
                this.projectile.OnDetonate();
                this.UnsetOverride();
            }

            public void UnsetOverride()
            {
                if (this.skillLocator)
                {
                    this.skillLocator.special.UnsetSkillOverride(this, detonateSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                Destroy(this);
            }

            private void FixedUpdate()
            {
                if (!this.projectile)
                {
                    this.UnsetOverride();
                }
            }
        }
    }
}

