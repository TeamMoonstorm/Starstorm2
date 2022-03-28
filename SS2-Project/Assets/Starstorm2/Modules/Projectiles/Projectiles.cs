using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Projectiles : ProjectileModuleBase
    {
        public static Projectiles Instance { get; set; }
        public override R2APISerializableContentPack SerializableContentPack { get; } = SS2Content.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing Projectiles.");
            GetProjectileBases();
        }

        protected override IEnumerable<ProjectileBase> GetProjectileBases()
        {
            base.GetProjectileBases()
                .ToList()
                .ForEach(projectile => AddProjectile(projectile));
            return null;
        }
    }
}
