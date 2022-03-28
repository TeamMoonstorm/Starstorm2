using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public class DamageTypes : DamageTypeModuleBase
    {
        public static DamageTypes Instance { get; private set; }

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing DamageTypes.");
            GetDamageTypeBases();
        }

        protected override IEnumerable<DamageTypeBase> GetDamageTypeBases()
        {
            base.GetDamageTypeBases()
                .ToList()
                .ForEach(dType => AddDamageType(dType));
            return null;
        }
    }
}