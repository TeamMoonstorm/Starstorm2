using RoR2;
using RoR2.HudOverlay;
using RoR2.Skills;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UI;
using EntityStates;

namespace Moonstorm.Starstorm2.Components
{
    public class NemCaptainController : NetworkBehaviour, IOnTakeDamageServerReceiver, IOnDamageDealtServerReceiver
    {
        [Header("Cached Components")]
        public CharacterBody characterBody;
        public Animator characterAnimator;

        [Header("Drone Orders")]
        public SkillFamily deck;
        public GenericSkill hand1;
        public GenericSkill hand2;
        public GenericSkill hand3;
        public GenericSkill hand4;
        public SkillDef nullSkill;
        private List<int> drawnSkillIndicies = new List<int>();
        private bool initialDeck = true;

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
        public float stressOnOutOfOrders;

        [Header("Stress UI")]
        [SerializeField]
        public GameObject stressOverlayPrefab;

        [SerializeField]
        public string stressOverlayChildLocatorEntry;
        private ChildLocator stressOverlayInstanceChildlocator;
        private OverlayController stressOverlayController;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();
        private Text uiStressText;

        [Header("Hand UI")]
        [SerializeField]
        public GameObject cardOverlayPrefab;

        [SerializeField]
        public string cardOverlayChildLocatorEntry;
        private ChildLocator cardOverlayInstanceChildlocator;
        private OverlayController cardOverlayController;

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
            //add prefab & necessary hooks
            OverlayCreationParams overlayCreationParams = new OverlayCreationParams
            {
                prefab = stressOverlayPrefab,
                childLocatorEntry = stressOverlayChildLocatorEntry
            };
            stressOverlayController = HudOverlayManager.AddOverlay(gameObject, overlayCreationParams);
            stressOverlayController.onInstanceAdded += OnOverlayInstanceAdded;
            stressOverlayController.onInstanceRemove += OnOverlayInstanceRemoved;

            //check for a characterbody .. just in case
            if (characterBody != null)
            {
                //characterBody.OnInventoryChanged += OnInventoryChanged;
                if (NetworkServer.active)
                {
                    HealthComponent.onCharacterHealServer += OnCharacterHealServer;

                    //setup cards
                    InitializeCards();
                }
            }
        }

        private void InitializeCards()
        {
            //reset on start
            if (initialDeck)
            {
                drawnSkillIndicies.Clear();
                initialDeck = false;
                Debug.Log("did initial clear");
            }

            //give each hand an order
            //Debug.Log("random skill : " + GetRandomSkillDefFromDeck().skillNameToken);
            if (hand1.skillDef == nullSkill)
                hand1.UnsetSkillOverride(gameObject, hand1.skillDef, GenericSkill.SkillOverridePriority.Replacement);

            hand1.SetSkillOverride(gameObject, GetRandomSkillDefFromDeck(), GenericSkill.SkillOverridePriority.Replacement);
            //hand1.SetBaseSkill(GetRandomSkillDefFromDeck());
            //hand1.SetSkillInternal(GetRandomSkillDefFromDeck());
            Debug.Log("gave hand 1 skill : " + hand1.skillDef);
            hand2.SetSkillOverride(gameObject, GetRandomSkillDefFromDeck(), GenericSkill.SkillOverridePriority.Replacement);

            if (hand2.skillDef == nullSkill)
                hand2.UnsetSkillOverride(gameObject, hand2.skillDef, GenericSkill.SkillOverridePriority.Replacement);
            Debug.Log("gave hand 2 skill: " + hand2.skillDef);
            hand3.SetSkillOverride(gameObject, GetRandomSkillDefFromDeck(), GenericSkill.SkillOverridePriority.Replacement);

            if (hand3.skillDef == nullSkill)
                hand3.UnsetSkillOverride(gameObject, hand3.skillDef, GenericSkill.SkillOverridePriority.Replacement);
            Debug.Log("gave hand 3 skill: " + hand3.skillDef);
            hand4.SetSkillOverride(gameObject, GetRandomSkillDefFromDeck(), GenericSkill.SkillOverridePriority.Replacement);

            if (hand4.skillDef == nullSkill)
                hand4.UnsetSkillOverride(gameObject, hand4.skillDef, GenericSkill.SkillOverridePriority.Replacement);
            Debug.Log("gave hand 4 skill: " + hand4.skillDef);
        }

        public SkillDef GetRandomSkillDefFromDeck()
        {
            //check if the entire deck is used
            if (drawnSkillIndicies.Count == deck.variants.Length)
            {
                Debug.Log("deck fully used; clearing");
                drawnSkillIndicies.Clear();
            }

            Debug.Log("starting to get random skill from deck");

            //loop until an unused order is found
            int randomVariantIndex;
            do
            {
                randomVariantIndex = Random.Range(0, deck.variants.Length);
            }
            while (drawnSkillIndicies.Contains(randomVariantIndex));

            //Debug.Log("randomSkillDef : " + randomSkillDef.name);

            //mark order as used
            drawnSkillIndicies.Add(randomVariantIndex);

            //Debug.Log("added skill to list : " + deck.variants[randomVariantIndex].skillDef.name);

            return deck.variants[randomVariantIndex].skillDef;
        }

        public void DiscardCardFromHand(int handIndex)
        {
            GenericSkill hand = GetHandByIndex(handIndex);
            if (hand != null)
            {
                //to-do: 'empty' skill
                Debug.Log("discarded hand");
                hand.UnsetSkillOverride(gameObject, hand.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                hand.SetSkillOverride(gameObject, nullSkill, GenericSkill.SkillOverridePriority.Loadout);
            }
        }

        public void DiscardCardsAndReplace()
        {
            DiscardCardFromHand(1);
            Debug.Log("discarded hand 1");
            DiscardCardFromHand(2);
            Debug.Log("discarded hand 2");
            DiscardCardFromHand(3);
            Debug.Log("discarded hand 3");
            DiscardCardFromHand(4);
            Debug.Log("discarded hand 4");

            //full reset
            InitializeCards();
        }

        //lol
        private GenericSkill GetHandByIndex(int handIndex)
        {
            switch (handIndex)
            {
                case 1:
                    return hand1;
                case 2:
                    return hand2;
                case 3:
                    return hand3;
                case 4:
                    return hand4;
                default:
                    return null;
            }
        }

        private void OnDisable()
        {
            if (stressOverlayController != null)
            {
                stressOverlayController.onInstanceAdded -= OnOverlayInstanceAdded;
                stressOverlayController.onInstanceRemove -= OnOverlayInstanceRemoved;
                fillUiList.Clear();
                HudOverlayManager.RemoveOverlay(stressOverlayController);
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
            uiStressText = instance.GetComponent<Text>();
            //uiStressText.font = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/animVoidSurvivorCorruptionUI.controller").WaitForCompletion().GetComponent<TextMeshProUGUI>().font;
            //uiStressText.fontSharedMaterial = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/animVoidSurvivorCorruptionUI.controller").WaitForCompletion().GetComponent<TextMeshProUGUI>().fontSharedMaterial;
            //uiStressText.fontMaterial = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/animVoidSurvivorCorruptionUI.controller").WaitForCompletion().GetComponent<TextMeshProUGUI>().fontMaterial;

            stressOverlayInstanceChildlocator = instance.GetComponent<ChildLocator>();
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

            //overstress toggle && check for empty hand
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

                if (hand1.skillDef == nullSkill && hand2.skillDef == nullSkill && hand3.skillDef == nullSkill && hand4.skillDef == nullSkill)
                {
                    InitializeCards();
                    if (!isOverstressed)
                    {
                        AddStress(stressOnOutOfOrders);
                    }
                }
            }
        }

        private void UpdateUI()
        {
            foreach (ImageFillController imageFillController in fillUiList)
            {
                imageFillController.SetTValue(stress / maxStress);
            }
            if (stressOverlayInstanceChildlocator)
            {
                stressOverlayInstanceChildlocator.FindChild("StressThreshold").rotation = Quaternion.Euler(0f, 0f, Mathf.InverseLerp(0f, maxStress, stress) * -360f);
                //overlayInstanceChildlocator.FindChild("MinStressShreshold");
            }
            if (uiStressText)
            {
                StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
                stringBuilder.AppendInt(Mathf.FloorToInt(stress), 1U, 3U).Append("%");
                uiStressText.text = stringBuilder.ToString();
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
            //probably ui stuff here later gulp
            Network_stress = newStress;
        }

        public override void PreStartClient()
        {
        }

        private void UNetVersion()
        {
        }

        //magic idk
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
