using System;
using UnityEngine;
using RoR2;
using System.Collections.Generic;
using JetBrains.Annotations;


namespace SS2.Components
{
    // Copy of Indicator but with added logic to only be visible to nemcrocos
    public class RadiationIndicator : MonoBehaviour
    {
        public RadiationIndicator(GameObject owner, GameObject visualizerPrefab)
        {
            this.owner = owner;
            _visualizerPrefab = visualizerPrefab;
        }

        public GameObject visualizerPrefab
        {
            get
            {
                return _visualizerPrefab;
            }
            set
            {
                bool flag = _visualizerPrefab == value;
                if (!flag)
                {
                    _visualizerPrefab = value;
                    bool flag2 = visualizerInstance;
                    if (flag2)
                    {
                        DestroyVisualizer();
                        InstantiateVisualizer();
                    }
                }
            }
        }

        public GameObject visualizerInstance { get; private set; }
        private Transform visualizerTransform { get; set; }

        public bool hasVisualizer
        {
            get
            {
                return visualizerInstance;
            }
        }

        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                if (_active != value)
                {
                    _active = value;
                    if (this.active)
                    {
                        RadiationIndicatorManager.AddIndicator(this);
                    }
                    else
                    {
                        RadiationIndicatorManager.RemoveIndicator(this);
                    }
                }
            }
        }

        private GameObject _visualizerPrefab;
        public readonly GameObject owner;
        public Transform targetTransform;
        protected List<Renderer> visualizerRenderers = new List<Renderer>();
        private bool _active = false;
        private bool visible = true;


        public void SetVisualizerInstantiated(bool newVisualizerInstantiated)
        {
            if (visualizerInstance != newVisualizerInstantiated)
            {
                if (newVisualizerInstantiated)
                {
                    InstantiateVisualizer();
                }
                else
                {
                    DestroyVisualizer();
                }
            }
        }

        private void InstantiateVisualizer()
        {
            visualizerInstance = UnityEngine.Object.Instantiate<GameObject>(visualizerPrefab);
            OnInstantiateVisualizer();
        }

        private void DestroyVisualizer()
        {
            OnDestroyVisualizer();
            UnityEngine.Object.Destroy(visualizerInstance);
            visualizerInstance = null;
        }

        protected void FindRenderers(Transform root)
        {
            visualizerRenderers.AddRange(root.GetComponentsInChildren<Renderer>(true));
        }

        private void FindInputBindingDisplayControllers(Transform root)
        {
            InputBindingDisplayController[] componentsInChildren = root.GetComponentsInChildren<InputBindingDisplayController>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                InputBindingDisplayController ibdc = componentsInChildren[i];
                InputBindingDisplayController ibdc2 = ibdc;
                ibdc2.oneFrameAfterTextUpdate = (Action)Delegate.Combine(ibdc2.oneFrameAfterTextUpdate, new Action(delegate ()
                {
                    FindRenderers(ibdc.transform);
                }));
            }
        }

        public void OnInstantiateVisualizer()
        {
            visualizerTransform = visualizerInstance.transform;
            FindRenderers(visualizerTransform);
            FindInputBindingDisplayControllers(visualizerTransform);
            SetVisibleInternal(visible);
        }

        public virtual void OnDestroyVisualizer()
        {
            visualizerTransform = null;
            visualizerRenderers.Clear();
        }

        public virtual void UpdateVisualizer()
        {
        }

        public virtual void PositionForUI(Camera sceneCamera, Camera uiCamera)
        {
            if (targetTransform)
            {
                Vector3 worldPosition = targetTransform.position;
                Vector3 screenPosition = sceneCamera.WorldToScreenPoint(worldPosition);
                screenPosition.z = ((screenPosition.z > 0f) ? 1f : -1f);
                Vector3 uiPosition = uiCamera.ScreenToWorldPoint(screenPosition);

                if (visualizerTransform != null)
                {
                    visualizerTransform.position = uiPosition;
                }
            }
        }

        public void SetVisible(bool newVisible)
        {
            newVisible &= targetTransform;
            if (visible != newVisible)
            {
                SetVisibleInternal(newVisible);
            }
        }

        private void SetVisibleInternal(bool newVisible)
        {
            visible = newVisible;
            foreach (Renderer renderer in visualizerRenderers)
            {
                if (renderer)
                {
                    renderer.enabled = newVisible;
                }
            }
        }

        
        private static class RadiationIndicatorManager
        {

            static RadiationIndicatorManager()
            {
                CameraRigController.onCameraTargetChanged += delegate (CameraRigController cameraRigController, GameObject target)
                {
                    RebuildVisualizerForAll();
                };
                UICamera.onUICameraPreRender += OnPreRenderUI;
                UICamera.onUICameraPostRender += OnPostRenderUI;
                RoR2Application.onUpdate += Update;
            }

            public static void AddIndicator([NotNull] RadiationIndicator indicator)
            {
                runningIndicators.Add(indicator);
                RebuildVisualizer(indicator);
            }
            public static void RemoveIndicator([NotNull] RadiationIndicator indicator)
            {
                indicator.SetVisualizerInstantiated(false);
                runningIndicators.Remove(indicator);
            }

            private static BodyIndex nemCroco;
            private static bool CanSeeRadiation(BodyIndex bodyIndex)
            {
                if (nemCroco == BodyIndex.None)
                {
                    nemCroco = BodyCatalog.FindBodyIndex("NemCrocoBody");
                }
                return bodyIndex == nemCroco;
            }
            private static void RebuildVisualizerForAll()
            {
                foreach (RadiationIndicator indicator in runningIndicators)
                {
                    RebuildVisualizer(indicator);
                }
            }

            private static void Update()
            {
                foreach (RadiationIndicator indicator in runningIndicators)
                {
                    if (indicator.hasVisualizer)
                    {
                        indicator.UpdateVisualizer();
                    }
                }
            }

            private static void RebuildVisualizer(RadiationIndicator indicator)
            {
                bool isVisibleToAtLeastOneCamera = false;
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    if (cameraRigController.target == indicator.owner && cameraRigController.targetBody && CanSeeRadiation(cameraRigController.targetBody.bodyIndex))
                    {
                        isVisibleToAtLeastOneCamera = true;
                        break;
                    }
                }
                indicator.SetVisualizerInstantiated(isVisibleToAtLeastOneCamera);
            }

            private static void OnPreRenderUI(UICamera uiCam)
            {
                GameObject cameraTarget = uiCam.cameraRigController.target;
                Camera sceneCam = uiCam.cameraRigController.sceneCam;
                foreach (RadiationIndicator indicator in runningIndicators)
                {
                    bool visible = cameraTarget == indicator.owner;
                    indicator.SetVisible(cameraTarget == indicator.owner && uiCam.cameraRigController.targetBody && CanSeeRadiation(uiCam.cameraRigController.targetBody.bodyIndex));
                    if (visible)
                    {
                        indicator.PositionForUI(sceneCam, uiCam.camera);
                    }
                }
            }
            private static void OnPostRenderUI(UICamera uiCamera)
            {
                foreach (RadiationIndicator indicator in runningIndicators)
                {
                    indicator.SetVisible(true);
                }
            }

            private static readonly List<RadiationIndicator> runningIndicators = new List<RadiationIndicator>();
        }

    }
}
