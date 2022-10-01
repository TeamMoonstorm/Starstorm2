using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(SkillLocator))]
    public class PyroController : NetworkBehaviour
    {
        private void Awake()
        {
            HeatComponent.heatGauge = LegacyResourcesAPI.Load<Texture2D>("texHeatgauge.png");
            HeatComponent.heatGauge_Heated = LegacyResourcesAPI.Load<Texture2D>("texHeatgauge_heated.png");
            HeatComponent.heatBar = LegacyResourcesAPI.Load<Texture2D>("texHeatbar.png");
            HeatComponent.heatBar_Heated = LegacyResourcesAPI.Load<Texture2D>("texHeatbar_heated.png");
        }

    }
}