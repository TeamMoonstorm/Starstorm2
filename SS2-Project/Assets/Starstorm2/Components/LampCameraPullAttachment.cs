using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using RoR2;

namespace SS2
{
    // Iterate through all LampCameraPullControllers that want to pull me, pull camera/set AI target to the idk oldest? closest? one.
    public class LampCameraPullAttachment : NetworkBehaviour//, ICameraStateProvider
    {
        public static void AddPullToBody(GameObject bodyObject, LampCameraPullController controller, float duration)
        {
            var attachment = GetAttachmentForBody(bodyObject);
            if (!attachment)
            {
                attachment = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("LampCameraPullAttachment", SS2Bundle.Monsters)).GetComponent<LampCameraPullAttachment>();
                attachment.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(bodyObject);
            }
            attachment.AddPull(controller, duration);
        }
        public static LampCameraPullAttachment GetAttachmentForBody(GameObject bodyObject)
        {
            var list = InstanceTracker.GetInstancesList<LampCameraPullAttachment>();
            for (int i = 0; i < list.Count; i++)
            {
                var attachment = list[i];
                if (attachment.bodyAttachment.attachedBodyObject == bodyObject)
                {
                    return attachment;
                }
            }
            return null;
        }

        public PostProcessVolume ppVolume;
        public float ppDuration = 1f;
        public float ppMaxAngle = 180f;
        public AnimationCurve ppWeightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Weight over time
        public AnimationCurve ppAngleWeightCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Weight based on camera angle

        public float expireDuration = 1f;

        [SyncVar]
        public GameObject target;
        [SyncVar]
        public bool alive;

        //public float pullSpeed = 180f;
        //public float minFov = 60f;
        //public float maxFov = 80f;
        //public AnimationCurve pullCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        //public float angleForMaxPull = 30f;
        //public float enterCamDuration = 0.5f;
        //public float exitCamDuration = 0.5f;

        private NetworkedBodyAttachment bodyAttachment;
        private float expireTimer;
        
        private List<LampCameraPullController> lamps = new List<LampCameraPullController>();
        
        private void Awake()
        {
            bodyAttachment = GetComponent<NetworkedBodyAttachment>();
            
        }
        private void Start()
        {
            var cameraEffect = GetComponent<LocalCameraEffect>();
            cameraEffect.targetCharacter = bodyAttachment.attachedBodyObject;
        }
        private void OnEnable()
        {
            InstanceTracker.Add<LampCameraPullAttachment>(this);
        }
        private void OnDisable()
        {
            InstanceTracker.Remove<LampCameraPullAttachment>(this);
            if (ppVolume)
            {
                ppVolume.transform.parent.SetParent(null); // >:(
                ppVolume.gameObject.AddComponent<LampPostProcessDuration>();
            }
            //EndCamera();
        }
       
        public void AddPull(LampCameraPullController lamp, float duration)
        {
            expireTimer = duration;
            if (!lamps.Contains(lamp))
            {
                lamps.Add(lamp);
            }
        }
        
        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                ServerFixedUpdate();
            }
        }

        private void ServerFixedUpdate()
        {
            expireTimer -= Time.fixedDeltaTime;
            if (expireTimer <= 0)
            {
                Destroy(gameObject);
                return;
            }

            SelectLampServer();
            if (target && bodyAttachment.attachedBody && bodyAttachment.attachedBody.master && bodyAttachment.attachedBody.master.aiComponents.Length > 0)
            {
                bodyAttachment.attachedBody.master.aiComponents[0].currentEnemy.gameObject = target;
            }
            
        }
        private void SelectLampServer()
        {
            for (int i = 0; i < lamps.Count; i++)
            {
                if (lamps[i])
                {
                    target = lamps[i].gameObject;
                }
            }
        }

        private float ppStopwatch;
        private static bool log = false;

        private void Update()
        {
            if (!bodyAttachment.attachedBody)
            {
                return;
            }
            
            if (ppVolume && target)
            {
                ppStopwatch += Time.deltaTime;
                float time = Mathf.Clamp01(ppStopwatch / ppDuration);
                float weightFromTime = ppWeightCurve.Evaluate(time);

                Ray aimRay = bodyAttachment.attachedBody.inputBank.GetAimRay();
                Vector3 between = target.transform.position - aimRay.origin;
                float angle = Mathf.Clamp01(Vector3.Angle(between, aimRay.direction) / ppMaxAngle);
                float weightFromAngle = ppAngleWeightCurve.Evaluate(angle);

                ppVolume.weight = weightFromTime * weightFromAngle;
                //PostProcessVolume.DispatchVolumeSettingsChangedEvent(); // wheres the fucking method ????
                if (log)
                {
                    SS2Log.Debug($"Lamp pp  (time){weightFromTime} * (angle){weightFromAngle}");
                }
            }
        }
        public class LampPostProcessDuration : MonoBehaviour
        {
            private void Update()
            {
                stopwatch += Time.deltaTime;
                UpdatePostProccess();
            }
            private void Awake()
            {
                ppVolume = GetComponent<PostProcessVolume>();
                inWeight = ppVolume.weight;
                UpdatePostProccess();
            }
            private void UpdatePostProccess()
            {
                float num = Mathf.Clamp01(stopwatch / maxDuration);
                ppVolume.weight = inWeight * ppWeightCurve.Evaluate(num);
                if (num == 1f)
                {
                    Destroy(ppVolume.transform.parent.gameObject); // :P
                }
            }
            public float inWeight;
            public PostProcessVolume ppVolume;
            public static AnimationCurve ppWeightCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            public static float maxDuration = 1;
            private float stopwatch;
        }
    }
}
