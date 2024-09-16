
using BepInEx;
using RoR2;
using RoR2.ConVar;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    /// <summary>
    /// The <see cref="OldEventDirector"/> is a Singleton class that's used for managing MSU's Event system.
    /// </summary>
    [RequireComponent(typeof(NetworkStateMachine))]
    public class OldEventDirector : MonoBehaviour
    {
        /// <summary>
        /// Returns the current instance of the EventDirector
        /// </summary>
        public static OldEventDirector Instance { get; private set; }
        /// <summary>
        /// The NetworkStateMachine tied to this Eventdirector
        /// </summary>
        public NetworkStateMachine NetworkStateMachine { get; private set; }

        private int stagesUntilNem;


        private float dogdshit;
        private bool fuckyo;
        [SystemInitializer]
        private static void SystemInit()
        {
            Run.onRunStartGlobal += (run) =>
            {
                if (NetworkServer.active && SS2.Events.EnableEvents.value)
                {
                    var go = Object.Instantiate(SS2Assets.LoadAsset<GameObject>("SS2EventDirector", SS2Bundle.Events), run.transform);
                    NetworkServer.Spawn(go);
                    var thisComponent = go.GetComponent<OldEventDirector>();
                    if (!thisComponent.isActiveAndEnabled)
                        thisComponent.enabled = true;

                }
            };
        }

        private void Awake()
        {
            if (NetworkServer.active)
            {
                NetworkStateMachine = GetComponent<NetworkStateMachine>();
            }
        }
        private void OnEnable()
        {
            if (!Instance)
            {
                Stage.onStageStartGlobal += TrySpawnNem;
                Instance = this;
                return;
            }
        }

        private void FixedUpdate()
        {
            if (fuckyo)
            {
                dogdshit += Time.fixedDeltaTime;
                if (dogdshit >= 4f)
                {
                    doit();
                    fuckyo = false;
                }
            }


        }
        private void TrySpawnNem(Stage obj)
        {
            if (TeleporterInteraction.instance && obj.sceneDef.sceneType == SceneType.Stage)
            {
                stagesUntilNem--;
                if (stagesUntilNem <= 0)
                {
                    foreach (var player in PlayerCharacterMasterController.instances)
                    {
                        if (player.master.inventory)
                        {
                            if (player.master.inventory.GetItemCount(SS2Content.Items.VoidRock) > 0)
                            {
                                stagesUntilNem = 3;
                                fuckyo = true;
                                return;
                            }
                        }
                    }

                }
            }
        }

        bool orange;
        bool red;
        private void doit()
        {
            if (red && orange) return;
            bool redguy = !red && (orange || UnityEngine.Random.Range(0f, 1f) > 0.5f);
            if (redguy)
            {
                red = true;
                NetworkStateMachine.stateMachines[0].SetState(new EntityStates.Events.NemCommandoBossState());
            }
            else
            {
                NetworkStateMachine.stateMachines[0].SetState(new EntityStates.Events.NemMercenaryBossState());
                orange = true;
            }
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Stage.onStageStartGlobal -= TrySpawnNem;
                Instance = null;
            }
        }
    }
}
