using RoR2;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Buffs;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(SkillLocator))]
    public class PyroController : NetworkBehaviour
    {
        private void Awake()
        {
            PyroHeatComponent.heatGauge = Resources.Load<Texture2D>("texHeatgauge.png");
            PyroHeatComponent.heatGauge_Heated = Resources.Load<Texture2D>("texHeatgauge_heated.png");
            PyroHeatComponent.heatBar = Resources.Load<Texture2D>("texHeatbar.png");
            PyroHeatComponent.heatBar_Heated = Resources.Load<Texture2D>("texHeatbar_heated.png");
        }

    }
}