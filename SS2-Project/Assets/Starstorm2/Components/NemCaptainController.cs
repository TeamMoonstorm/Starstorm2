using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    public class NemCaptainController : NetworkBehaviour
    {
        [Header("Cached Components")]
        public CharacterBody characterBody;
        public Animator characterAnimator;

        [Header("Stress Values")]
        public float minStress;
        public float maxStress;
        public float stressPerSecondInCombat;
        public float stressPerSecondOutOfCombat;
        public float stressPerSecondWhileOverstressed;
        public float stressGainedOnFullDamage;
        public float stressGainedOnOSP;
        public float stressGainedOnHeal;
        public float stressGainedOnCrit;
        public float stressGainedOnKill;
        public float stressGainedOnItem;

        [Header("UI")]
        [SerializeField]
        public GameObject overlayPrefab;

        [SerializeField]
        public string overlayChildLocatorEntry;
        private ChildLocator overlayInstanceChildlocator;
        private OverlayController overlayController;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();
        private TextMeshProUGUI uiStressText;

        private int itemCount;

        [SyncVar(hook = "OnStressModified")]
        private float _stress;
        
        public float stress
        {
            get
            {
                return _stress;
            }
        }

        public float stressFraction
        {
            get
            {
                return stress / maxStress;
            }
        }

        public float stressPercentage
        {
            get
            {
                return stressFraction * 100f;
            }
        }

        public bool isFullStress
        {
            get
            {
                return stress >= maxStress;
            }
        }

        public bool isOverstressed
        {
            get
            {
                return characterBody && characterBody.HasBuff(SS2Content.Buffs.bdOverstress);
            }
        }

        private HealthComponent bodyHealthComponent
        {
            get
            {
                return characterBody.healthComponent;
            }
        }

        public float Network_stress
        {
            get
            {
                return _stress;
            }
            [param: In]
            set
            {
                if (NetworkServer.localClientActive && syncVarHookGuard)
                {
                    syncVarHookGuard = true;
                    OnStressModified(value);
                    syncVarHookGuard = false;
                }
                SetSyncVar<float>(value, ref _stress, 1U); //please work
            }
        }

        [Server]
        public void AddStress(float amount)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'Moonstorm.Starstorm2.Components.NemCaptainController::AddStress(System.Single)' called on client");
                return;
            }
            Network_stress = Mathf.Clamp(stress + amount, minStress, maxStress);
        }

        private void OnEnable()
        {
            //prefab stuff
            OverlayCreationParams overlayCreationParams = new OverlayCreationParams
            {
                prefab = overlayPrefab,
                childLocatorEntry = overlayChildLocatorEntry
            };
            overlayController = HudOverlayManager.AddOverlay(gameObject, overlayCreationParams);
            overlayController.onInstanceAdded += OnOverlayInstanceAdded;
            overlayController.onInstanceRemove += OnOverlayInstanceRemoved;

            if (characterBody)
            {
                //characterBody.OnInventoryChanged += OnInventoryChanged;
                if (NetworkServer.active)
                {
                    HealthComponent.onCharacterHealServer += OnCharacterHealServer;
                }
            }
        }

        private void OnDisable()
        {
            if (overlayController != null)
            {
                overlayController.onInstanceAdded -= OnOverlayInstanceAdded;
                overlayController.onInstanceRemove -= OnOverlayInstanceRemoved;
                fillUiList.Clear();
                HudOverlayManager.RemoveOverlay(overlayController);
            }
            if (characterBody)
            {
                //characterBody.onInventoryChanged -= OnInventoryChanged;
                if (NetworkServer.active)
                {
                    HealthComponent.onCharacterHealServer -= OnCharacterHealServer;
                }
            }
        }

        private void OnOverlayInstanceAdded(OverlayController controller, GameObject instance)
        {
            fillUiList.Add(instance.GetComponent<ImageFillController>());
            uiStressText = instance.GetComponent<TextMeshProUGUI>();
            uiStressText.font = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/animVoidSurvivorCorruptionUI.controller").WaitForCompletion().GetComponent<TextMeshProUGUI>().font;
            uiStressText.fontSharedMaterial = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/animVoidSurvivorCorruptionUI.controller").WaitForCompletion().GetComponent<TextMeshProUGUI>().fontSharedMaterial;
            uiStressText.fontMaterial = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/animVoidSurvivorCorruptionUI.controller").WaitForCompletion().GetComponent<TextMeshProUGUI>().fontMaterial;

            overlayInstanceChildlocator = instance.GetComponent<ChildLocator>();
        }

        private void OnOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {
            fillUiList.Remove(instance.GetComponent<ImageFillController>());
        }

        private void FixedUpdate()
        {
            float num;

            if (!isOverstressed)
                num = characterBody.outOfCombat ? stressPerSecondOutOfCombat : stressPerSecondInCombat;
            else
                num = stressPerSecondWhileOverstressed;

            if (NetworkServer.active && !characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility))
                AddStress(num * Time.fixedDeltaTime);

            UpdateUI();


            if (NetworkServer.active)
            {
                if (stress >= maxStress && !isOverstressed)
                {
                    characterBody.SetBuffCount(SS2Content.Buffs.bdOverstress.buffIndex, 1);
                }

                if (stress <= minStress && isOverstressed)
                {
                    characterBody.SetBuffCount(SS2Content.Buffs.bdOverstress.buffIndex, 0);
                }
            }
        }

        private void UpdateUI()
        {
            foreach (ImageFillController imageFillController in fillUiList)
            {
                imageFillController.SetTValue(stress / maxStress);
            }
            if (overlayInstanceChildlocator)
            {
                overlayInstanceChildlocator.FindChild("StressThreshold").rotation = Quaternion.Euler(0f, 0f, Mathf.InverseLerp(0f, maxStress, stress) * -360f);
                //overlayInstanceChildlocator.FindChild("MinStressShreshold");
            }
            if (uiStressText)
            {
                StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
                stringBuilder.AppendInt(Mathf.FloorToInt(stress), 1U, 3U).Append("%");
                uiStressText.SetText(stringBuilder);
                StringBuilderPool.ReturnStringBuilder(stringBuilder);
            }
        }

        private void OnCharacterHealServer(HealthComponent healthComponent, float amount, ProcChainMask procChainMask)
        {
            if (healthComponent == bodyHealthComponent)
            {
                float num = amount / bodyHealthComponent.fullCombinedHealth;
                AddStress(num * stressGainedOnHeal);
            }
        }

        public void OnDamageDealtServer(DamageReport damageReport)
        {
            if (damageReport.damageInfo.crit)
                AddStress(damageReport.damageInfo.procCoefficient * stressGainedOnCrit);
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            float num = damageReport.damageDealt / bodyHealthComponent.fullCombinedHealth;
            AddStress(num * stressGainedOnFullDamage);
        }

        private void OnInventoryChanged()
        {

        }

        private void OnStressModified(float newStress)
        {
            //ui stuff
            Network_stress = newStress;
        }

        public override void PreStartClient()
        {
        }

        private void UNetVersion()
        {
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(_stress);
                return true;
            }
            bool flag = false;
            if ((syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(_stress);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                _stress = reader.ReadSingle();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                OnStressModified(reader.ReadSingle());
            }
        }
    }
}
