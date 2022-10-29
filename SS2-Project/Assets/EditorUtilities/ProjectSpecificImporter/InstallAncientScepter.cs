using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderKit.Core;
using ThunderKit.Core.Config;
using ThunderKit.Integrations.Thunderstore;

namespace Moonstorm.Starstorm2.Editor
{
    internal class InstallAncientScepter : ThunderstorePackageInstaller
    {
        public override string DependencyId => "amogus_lovers-StandaloneAncientScepter";

        public override string ThunderstoreAddress => "https://thunderstore.io";

        public override int Priority => RiskOfThunder.RoR2Importer.Constants.Priority.InstallMLAPI - 500;
    }
}