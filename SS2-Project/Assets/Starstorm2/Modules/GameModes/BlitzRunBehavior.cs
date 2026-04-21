using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2
{
    public class BlitzRunBehavior : MonoBehaviour
    {
        private static BlitzRunBehavior instance;

        private void OnEnable()
        {
            instance = this;
            Run.onRunStartGlobal += OnRunStart;
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin += OnTeleporterInteractionBegin;
            On.RoR2.SceneDirector.PopulateScene += OnPopulateScene;
            ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
        }

        private void OnDisable()
        {
            Run.onRunStartGlobal -= OnRunStart;
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin -= OnTeleporterInteractionBegin;
            On.RoR2.SceneDirector.PopulateScene -= OnPopulateScene;
            ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
            instance = null;
        }

        private void OnRunStart(Run run)
        {
            if (run.gameObject != gameObject)
                return;

            if (NetworkServer.active)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "SS2_GAMEMODE_BLITZ_START"
                });
            }
        }

        private void OnTeleporterInteractionBegin(
            On.RoR2.TeleporterInteraction.IdleState.orig_OnInteractionBegin orig,
            EntityStates.BaseState self, Interactor activator)
        {
            if (instance)
                return;

            orig(self, activator);
        }

        private void OnPopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);

            if (!instance || !self.teleporterInstance)
                return;

            if (!self.teleporterInstance.TryGetComponent<InteractionProcFilter>(out var filter))
                filter = self.teleporterInstance.AddComponent<InteractionProcFilter>();

            filter.shouldAllowOnInteractionBeginProc = false;
        }

        private static void OnCollectObjectiveSources(
            CharacterMaster master,
            List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
        {
            if (!instance)
                return;

            objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
            {
                master = master,
                objectiveType = typeof(BlitzSurviveObjectiveTracker),
                source = instance
            });
        }

        public class BlitzSurviveObjectiveTracker : ObjectivePanelController.ObjectiveTracker
        {
            protected override string GenerateString()
            {
                return Language.GetString("SS2_GAMEMODE_BLITZ_OBJECTIVE");
            }

            protected override bool IsDirty()
            {
                return false;
            }
        }

        // TODO: Dont delete
        // On.RoR2.Run.HandlePlayerFirstEntryAnimation += OnHandlePlayerFirstEntryAnimation;
        // private void OnHandlePlayerFirstEntryAnimation(
        //     On.RoR2.Run.orig_HandlePlayerFirstEntryAnimation orig,
        //     Run self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        // {
        //     if (self.gameObject == gameObject)
        //     {
        //         // No survivor pod — players teleport in directly
        //         body.SetBodyStateToPreferredInitialState();
        //         return;
        //     }
        //     orig(self, body, spawnPosition, spawnRotation);
        // }
    }
}
