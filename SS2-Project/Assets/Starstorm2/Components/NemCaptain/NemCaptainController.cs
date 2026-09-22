using RoR2;
using RoR2.HudOverlay;
using RoR2.Skills;
using RoR2.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using HG;
using TMPro;
using System.Linq;
using EntityStates;
namespace SS2.Components
{
    public class NemCaptainController : NetworkBehaviour, IOnTakeDamageServerReceiver, IOnDamageDealtServerReceiver, IOnKilledOtherServerReceiver
    {
        [Header("Cached Components")]
        public CharacterBody characterBody;
        public SkillLocator skillLocator;
        public Animator characterAnimator;
        public EntityStateMachine weaponStateMachine;


        [Header("Drone Orders")]
        public SkillDef[][] decks;
        public SkillDef[] deck1;
        public SkillDef[] deck2;
        public SkillDef[] deck3;
        private List<SkillDef> ordersStaticList
        {
            get
            {

                GenericSkill gs = skillLocator?.FindSkillByFamilyName("sfNemCaptainDeck");
                string skillName = gs?.skillDef?.skillName;
                if (skillName == null)
                {
                    deckFound = false;
                    return null;
                }
                switch (skillName)
                {
                    default :
                        deckFound = false;
                        return null;
                    case "NemCapDeckSimple" :
                        deckFound = true;
                        return deck1.ToList();
                    case "NemCapDeckFull" :
                        deckFound = true;
                        return deck2.ToList();
                    case "NemCapDeckBased" :
                        deckFound = true;
                        return deck3.ToList();
                }
            }
        }
        private Queue<SkillDef> ordersQueue = new Queue<SkillDef>();
        private SkillStateOverrideData TheSenatorOfAuthority;
        private bool isOverriding;

        private bool order1Set = false;
        private bool order2Set = false;
        private bool order3Set = false;

        private bool deckFound = false;
        [Header("Stress Values")]
        public float minStress;
        public float maxStress;
        public float bonusMaxStressPerStack;
        public float stressPerSecondInCombat;
        public float stressPerSecondOutOfCombat;
        public float stressPerSecondWhileOverstressed;
        public float stressPerSecondWhileTotalReset;
        public float stressPerSecondWhileRegenBuff;
        public float stressPerSecondWhileBarriered;
        public float stressGainedOnFullDamage;
        public float stressGainedOnOSP;
        public float stressGainedOnHeal;
        public float stressGainedOnCrit;
        public float stressGainedOnKill;
        public float stressGainedOnMinibossKill;
        public float stressGainedOnBossKill;
        public float stressGainedWhenSurrounded;
        public float stressGainedOnItem;
        public float stressOnOutOfOrders;

        [HideInInspector]
        public int bonusMaxStressStacks;
        /*public float surroundedThreshold;
        [HideInInspector]
        public static float enemyCheckInterval = 0.033333333f;
        private float enemyCheckStopwatch = 0f;
        private SphereSearch enemySearch;
        private List<HurtBox> hits;
        public float enemyRadius = 12f;*/

        [Header("Stress UI")]
        [SerializeField]
        public GameObject stressOverlayPrefab;

        [SerializeField]
        public string stressOverlayChildLocatorEntry;
        private ChildLocator stressOverlayInstanceChildlocator;
        private OverlayController stressOverlayController;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();
        private TextMeshProUGUI uiStressText;

        [Header("Hand UI")]
        [SerializeField]
        public GameObject cardOverlayPrefab;

        [SerializeField]
        public string cardOverlayChildLocatorEntry;
        private ChildLocator cardOverlayInstanceChildlocator;
        private OverlayController cardOverlayController;
        private Image[] freeOrderMarks;

        private NemCaptainSkillIcon ncsi1;
        private NemCaptainSkillIcon ncsi2;
        private NemCaptainSkillIcon ncsi3;

        [Header("Drones & Drone Positions")]
        [SerializeField]
        public GameObject droneA;
        private Transform droneABaseTransform;
        [SerializeField]
        public GameObject droneB;
        private Transform droneBBaseTransform;
        [SerializeField]
        public GameObject droneC;
        private Transform droneCBaseTransform;
        [SerializeField]
        public GameObject droneAimRoot;
        private Transform droneAimRootTransform;

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

        public float totalMaxStress
        {
            get
            {
                return maxStress + (bonusMaxStressPerStack * bonusMaxStressStacks);
            }
        }

        public float stressFraction
        {
            get
            {
                if (isTotalReset)
                    return 0.0f;
                if (isOverstressed)
                    return 1.0f;
                return stress / totalMaxStress;
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
                return stress >= totalMaxStress;
            }
        }

        public bool isOverstressed
        {
            get
            {
                return characterBody && characterBody.HasBuff(SS2Content.Buffs.bdOverstress);
            }
        }

        public bool isTotalReset
        {
            get
            {
                return characterBody && characterBody.HasBuff(SS2Content.Buffs.bdTotalReset);
            }
        }

        public bool hasManaReductionBuff
        {
            get
            {
                return characterBody && characterBody.HasBuff(SS2Content.Buffs.bdNemCapManaReduction);
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
                if (NetworkServer.localClientActive && !syncVarHookGuard) //lol
                {
                    syncVarHookGuard = true;
                    OnStressModified(value);
                    syncVarHookGuard = false;
                }
                SetSyncVar<float>(value, ref _stress, 1U); //please work
            }
        }

        private int _freeOrders;
        private int freeOrders 
        { 
            get 
            {
                return _freeOrders;
            }
            set 
            { 
                if (value != _freeOrders)
                {
                    _freeOrders = Mathf.Clamp(value, 0, 3);
                    UpdateFreeOrderUI();
                    characterBody.SetBuffCount(SS2Content.Buffs.bdTacticalDecisionMaking.buffIndex, _freeOrders);
                }
            } 
        }

        public void UpdateFreeOrderUI()
        {
            for (int i = freeOrderMarks.Length; i > 0; i--)
            {
                if (freeOrders >= i)
                {
                    freeOrderMarks[i - 1].enabled = true;
                }
                else
                {
                    freeOrderMarks[i - 1].enabled = false;
                }
            }
        }

        public bool hasFreeOrders
        {
            get
            {
                if (freeOrders > 0)
                    return true;
                return false;
            }
        }

        public void GrantFreeOrders(int amount)
        {
            freeOrders += amount;
        }

        public void AddStressAndCycleNextOrder(float amount, GenericSkill activatorSkillSlot, bool ignoreFreeOrders = false)
        {
            CycleNextOrder(activatorSkillSlot, ignoreFreeOrders);
            AddOrderStress(amount, ignoreFreeOrders);
        }

        public void AddOrderStress(float amount, bool ignoreFreeOrders = false)
        {
            if (hasFreeOrders && !ignoreFreeOrders)
            {
                freeOrders--;
               /* if (NetworkServer.active && characterBody.HasBuff(SS2Content.Buffs.bdTacticalDecisionMaking))
                    characterBody.RemoveBuff(SS2Content.Buffs.bdTacticalDecisionMaking);*/
            }
            else
            {
                AddStress(amount);
            }
        }
        [Server]
        public void AddStress(float amount)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'SS2.Components.NemCaptainController::AddStress(System.Single)' called on client");
                return;
            }

            if (isTotalReset && amount > 0)
            {
                amount = 0;
            }

            //halve mana used if has reduction buff
            if (hasManaReductionBuff && amount > 0)
                amount /= 2;

            Network_stress = Mathf.Clamp(stress + amount, minStress, totalMaxStress);
        }

        private void OnStressModified(float newStress)
        {
            //probably ui stuff here later gulp

            

            if (newStress >= totalMaxStress && !isOverstressed) //I kinda wanna switch the overstress mechanic to an entitystatemachine ala void fiend/herald ngl because I'm not a fan of this. Buffs aren't fully networked and shouldn't be the deciding factor for logic outside of stats
            {
                weaponStateMachine.SetNextStateToMain();
                characterBody.AddBuff(SS2Content.Buffs.bdOverstress.buffIndex);
            }

            if (newStress <= minStress && isOverstressed)
            {
                //characterBody.SetBuffCount(SS2Content.Buffs.bdOverstress.buffIndex, 0);
                characterBody.RemoveBuff(SS2Content.Buffs.bdOverstress.buffIndex);
            }

            Network_stress = newStress;
        }

        private void OnEnable()
        {
            //add prefab & necessary hooks
            OverlayCreationParams stressOverlayCreationParams = new OverlayCreationParams
            {
                prefab = stressOverlayPrefab,
                childLocatorEntry = stressOverlayChildLocatorEntry
            };
            stressOverlayController = HudOverlayManager.AddOverlay(gameObject, stressOverlayCreationParams);
            stressOverlayController.onInstanceAdded += OnStressOverlayInstanceAdded;
            stressOverlayController.onInstanceRemove += OnStressOverlayInstanceRemoved;

            OverlayCreationParams cardOverlayCreationParams = new OverlayCreationParams
            {
                prefab = cardOverlayPrefab,
                childLocatorEntry = cardOverlayChildLocatorEntry
            };
            cardOverlayController = HudOverlayManager.AddOverlay(gameObject, cardOverlayCreationParams);
            cardOverlayController.onInstanceAdded += OnCardOverlayInstanceAdded;
            cardOverlayController.onInstanceRemove += OnCardOverlayInstanceRemoved;

            //what the fuck are we doing here?
            if (droneA != null)
                droneABaseTransform = droneA.transform;
            if (droneB != null)
                droneBBaseTransform = droneB.transform;
            if (droneC != null)
                droneCBaseTransform = droneC.transform;
            if (droneAimRoot != null)
                droneAimRootTransform = droneAimRoot.transform;

            //check for a characterbody .. just in case
            if (characterBody != null)
            {
                //characterBody.OnInventoryChanged += OnInventoryChanged;
                if (NetworkServer.active)
                {
                    HealthComponent.onCharacterHealServer += OnCharacterHealServer;

                    //setup cards
                    Debug.Log("Setup cards");
                    RefreshSkillStateOverrides();
                }
            }
        }
        /// <summary>
        /// Obviously, sets the Order Overrides. Can be called repeatedly to refresh, does not leak.
        /// </summary>
        /// <param name="handIndex"></param>
        /// <returns></returns>
        public void SetOrderOverrides()
        {
            //Debug.Log("SetOrderOverrides()");
            if (isOverriding)
                UnsetOrderOverrides();
            RefreshSkillStateOverrides();
            TheSenatorOfAuthority.OverrideSkills(skillLocator);
            characterBody.onSkillActivatedAuthority += OnSkillActivatedAuthority;
            isOverriding = true;
        }
        /// <summary>
        /// Obviously, unsets the Order Overrides. Can be called repeatedly as a failsafe, does not leak.
        /// </summary>
        /// <param name="handIndex"></param>
        /// <returns></returns>
        public void UnsetOrderOverrides()
        {
            //Debug.Log("UnsetOrderOverrides()");
            if (!isOverriding)
                return;
            TheSenatorOfAuthority.ClearOverrides();
            characterBody.onSkillActivatedAuthority -= OnSkillActivatedAuthority;
            isOverriding = false;
        }
        private void OnSkillActivatedAuthority(GenericSkill skill)
        {
            //Debug.Log("OnSkillAuthority()");
            if (skill.skillDef is OrderSkillDef skillDef && skillDef.autoHandleOrderQueue)
            {
                CycleNextOrder(skill);
            }
        }
        private void RefreshSkillStateOverrides()
        {
            //Debug.Log("RefreshOrderOverrides()");
            TheSenatorOfAuthority = new SkillStateOverrideData(characterBody)
            {
                primarySkillOverride = TheSenatorOfAuthority?.primarySkillOverride,
                utilitySkillOverride = TheSenatorOfAuthority?.utilitySkillOverride,
                specialSkillOverride = TheSenatorOfAuthority?.specialSkillOverride,

                simulateRestockForOverridenSkills = true,
                overrideFullReloadOnAssign = true
            };

            if (!order1Set)
            {
                order1Set = true;
                TheSenatorOfAuthority.primarySkillOverride = GetNextOrder();
                if (ncsi1)
                    ncsi1.UpdateSkillRef(TheSenatorOfAuthority.primarySkillOverride);
            }

            if (!order2Set)
            {
                order2Set = true;
                TheSenatorOfAuthority.utilitySkillOverride = GetNextOrder();
                if (ncsi2)
                    ncsi2.UpdateSkillRef(TheSenatorOfAuthority.utilitySkillOverride);
            }

            if (!order3Set)
            {
                order3Set = true;
                TheSenatorOfAuthority.specialSkillOverride = GetNextOrder();
                if (ncsi3)
                    ncsi3.UpdateSkillRef(TheSenatorOfAuthority.specialSkillOverride);
            }
        }
        private SkillDef GetNextOrder()
        {
            //Debug.Log("GetNextOrder()");
            SkillDef orderDef;
            if (ordersQueue.TryPeek(out _))
            {
                orderDef = ordersQueue.Dequeue();
            }
            else
            {
                ResetAndShuffleQueue();
                orderDef = ordersQueue.Dequeue();
            }

            return orderDef;
        }
        private void ResetAndShuffleQueue()
        {
            //Debug.Log("ResetShuffle()");
            ordersQueue.Clear();
            List<SkillDef> shuffledOrders = ordersStaticList;
            shuffledOrders.Shuffle();
            for (int i = 0; i < shuffledOrders.Count; i++)
                ordersQueue.Enqueue(shuffledOrders[i]);
        }
        /// <summary>
        /// Call the next order to the hand for the specified skillSlot. If called manually within an entityState (In which case the skillDef's autoHandleOrderQueue should be false), call this upon activatorSkillSlot.
        /// </summary>
        /// <param name="skill"></param>
        public void CycleNextOrder(GenericSkill skill, bool ignoreFreeOrders = false)
        {
            //Debug.Log("CycleNextOrder()");
            if (hasFreeOrders && !ignoreFreeOrders)
            {
                CycleAllOrders();
                return;
            }
            switch (skill)
            {
                case GenericSkill _ when skill == skillLocator.primary:
                    order1Set = false;
                    break;
                case GenericSkill _ when skill == skillLocator.utility:
                    order2Set = false;
                    break;
                case GenericSkill _ when skill == skillLocator.special:
                    order3Set = false;
                    break;
                default:
                    break;
            }
            SetOrderOverrides();
        }
        /// <summary>
        /// Call the next order to all slots in hand. In expected order of Primary, Utility, then Special.
        /// </summary>
        /// <param name="skill"></param>
        public void CycleAllOrders()
        {
            order1Set = false;
            order2Set = false;
            order3Set = false;
            SetOrderOverrides();
        }
        /// <summary>
        /// [deprecated] remove this reference Zebra
        /// </summary>
        /// <param name="handIndex"></param>
        /// <returns></returns>
        public void DiscardCardFromHand(int handIndex)
        {
            /*GenericSkill hand = GetHandByIndex(handIndex);
            if (hand != null)
            {
                //to-do: 'empty' skill
                Debug.Log("discarded hand");
                hand.UnsetSkillOverride(gameObject, hand.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                hand.SetSkillOverride(gameObject, nullSkill, GenericSkill.SkillOverridePriority.Loadout);
            }*/
        }
        /// <summary>
        /// [deprecated] remove this reference Zebra
        /// </summary>
        /// <param name="handIndex"></param>
        /// <returns></returns>
        public void DiscardCardsAndReplace()
        {
            /*DiscardCardFromHand(1);
            Debug.Log("discarded hand 1");
            DiscardCardFromHand(2);
            Debug.Log("discarded hand 2");
            DiscardCardFromHand(3);
            Debug.Log("discarded hand 3");
            DiscardCardFromHand(4);
            Debug.Log("discarded hand 4");

            //full reset
            InitializeCards();*/
        }

        //lol
        /// <summary>
        /// [deprecated] remove this reference Zebra
        /// </summary>
        /// <param name="handIndex"></param>
        /// <returns></returns>
        private GenericSkill GetHandByIndex(int handIndex)
        {
            return null;
            /*switch (handIndex)
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
            }*/
        }

        private void OnDisable()
        {
            if (stressOverlayController != null)
            {
                stressOverlayController.onInstanceAdded -= OnStressOverlayInstanceAdded;
                stressOverlayController.onInstanceRemove -= OnStressOverlayInstanceRemoved;
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

        private void OnStressOverlayInstanceAdded(OverlayController controller, GameObject instance)
        {
            ImageFillController uiFillController = instance.GetComponentInChildren<ImageFillController>();
            if (uiFillController != null)
            {
                Debug.Log("uiFillController found!");
            }
            else
            {
                Debug.Log("uiFillController not found FUCK");
            }
            fillUiList.Add(uiFillController);
            uiStressText = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (uiStressText != null)
            {
                Debug.Log("uiStressText found!");
            }
            else
            {
                Debug.Log("uiStressText not found FUCK");
            }
            freeOrderMarks = new Image[3];
            freeOrderMarks[0] = instance.transform.Find("FillRoot/FreeOrdersRoot/Mark1").gameObject.GetComponent<Image>();
            freeOrderMarks[1] = instance.transform.Find("FillRoot/FreeOrdersRoot/Mark2").gameObject.GetComponent<Image>();
            freeOrderMarks[2] = instance.transform.Find("FillRoot/FreeOrdersRoot/Mark3").gameObject.GetComponent<Image>();
            //uiStressText.font = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/animVoidSurvivorCorruptionUI.controller").WaitForCompletion().GetComponent<TextMeshProUGUI>().font;
            //uiStressText.fontSharedMaterial = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/animVoidSurvivorCorruptionUI.controller").WaitForCompletion().GetComponent<TextMeshProUGUI>().fontSharedMaterial;
            //uiStressText.fontMaterial = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/animVoidSurvivorCorruptionUI.controller").WaitForCompletion().GetComponent<TextMeshProUGUI>().fontMaterial;

            stressOverlayInstanceChildlocator = instance.GetComponent<ChildLocator>();
        }

        private void OnStressOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {
            fillUiList.Remove(instance.GetComponent<ImageFillController>());
        }

        private void OnCardOverlayInstanceAdded(OverlayController controller, GameObject instance)
        {
            cardOverlayInstanceChildlocator = instance.GetComponent<ChildLocator>();

            if (cardOverlayInstanceChildlocator == null)
            {
                Debug.Log("card overlay instance childlocator null; returning");
                return;
            }

            Transform icon1 = cardOverlayInstanceChildlocator.FindChild("Skill1");
            if (icon1 != null)
            {
                Debug.Log("setting skill 1");
                ncsi1 = icon1.GetComponent<NemCaptainSkillIcon>();
                if (ncsi1.targetSkill == null)
                {
                    ncsi1.targetSkill = TheSenatorOfAuthority.primarySkillOverride;
                }
                ncsi1.characterBody = characterBody;
            }

            Transform icon2 = cardOverlayInstanceChildlocator.FindChild("Skill2");
            if (icon2 != null)
            {
                Debug.Log("setting skill 2");
                ncsi2 = icon2.GetComponent<NemCaptainSkillIcon>();
                if (ncsi2.targetSkill == null)
                {
                    ncsi2.targetSkill = TheSenatorOfAuthority.utilitySkillOverride;
                }
                ncsi2.characterBody = characterBody;
            }

            Transform icon3 = cardOverlayInstanceChildlocator.FindChild("Skill3");
            if (icon3 != null)
            {
                Debug.Log("setting skill 3");
                ncsi3 = icon3.GetComponent<NemCaptainSkillIcon>();
                if (ncsi3.targetSkill == null)
                {
                    ncsi3.targetSkill = TheSenatorOfAuthority.specialSkillOverride;
                }
                ncsi3.characterBody = characterBody;
            }
        }

        private void OnCardOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {
            fillUiList.Remove(instance.GetComponent<ImageFillController>());
        }

        private void FixedUpdate()
        {
            if (!deckFound)
            {
                RefreshSkillStateOverrides();
            }
            if (isOverriding)
            {
                TheSenatorOfAuthority.StepRestock();
            }
            float num;

            num = characterBody.outOfCombat ? stressPerSecondOutOfCombat : stressPerSecondInCombat;


            if (characterBody.HasBuff(SS2Content.Buffs.bdNemCapManaRegen))
                num += stressPerSecondWhileRegenBuff;

            if (bodyHealthComponent.barrier > 0)
                num += stressPerSecondWhileBarriered;

            if (isOverstressed)
                num = stressPerSecondWhileOverstressed;

            if (isTotalReset)
                num = stressPerSecondWhileTotalReset;

            

            //add final stress per second amount; no stress if invincible
            if (NetworkServer.active && !(characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility) || characterBody.HasBuff(RoR2Content.Buffs.Immune)))
                AddStress(num * Time.fixedDeltaTime);

            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (ImageFillController imageFillController in fillUiList)
            {
                imageFillController.SetTValue(stress / totalMaxStress);
            }
            if (stressOverlayInstanceChildlocator)
            {
                Transform wazzok = stressOverlayInstanceChildlocator.FindChild("StressThreshold");
                if (wazzok) //dwarven engineering at its finest
                    wazzok.rotation = Quaternion.Euler(0f, 0f, Mathf.InverseLerp(0f, totalMaxStress, stress) * -360f);
                //overlayInstanceChildlocator.FindChild("MinStressThreshold");
            }
            if (uiStressText)
            {
                StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
                string gradient = ColorUtility.ToHtmlStringRGB(Color.Lerp(new Color(0.8f, 0.8f, 0.8f), new Color(0.6f, 0f, 0f), Mathf.Clamp01(stress / totalMaxStress)));
                stringBuilder.Append($"<color=#{gradient}>").AppendInt(Mathf.FloorToInt(stress), 1U, 3U).Append(" / ").AppendInt(Mathf.FloorToInt(totalMaxStress), 1U, 3U).Append("</color>");
                uiStressText.SetText(stringBuilder);
                HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            }
        }

        private void OnCharacterHealServer(HealthComponent healthComponent, float amount, ProcChainMask procChainMask)
        {
            if (healthComponent == bodyHealthComponent)
            {
                float lostHealth = bodyHealthComponent.fullHealth - bodyHealthComponent.health;
                if ( lostHealth <= 0 )
                {
                    return;
                }
                float num = Mathf.Min(amount, lostHealth) / bodyHealthComponent.fullCombinedHealth;
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

        public void OnKilledOtherServer(DamageReport damageReport)
        {
            if (damageReport.victimIsBoss || damageReport.victimIsChampion || damageReport.victimBody.hullClassification == HullClassification.BeetleQueen)
            {
                AddStress(stressGainedOnBossKill);
                return;
            }

            if (damageReport.victimIsElite || damageReport.victimBody.hullClassification == HullClassification.Golem)
            {
                AddStress(stressGainedOnMinibossKill);
                return;
            }

            AddStress(stressGainedOnKill);
        }

        private void OnInventoryChanged()
        {

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

    public static class ListExtension
    {
        internal static void Shuffle<T>(this List<T> list)
        {
            int count = list.Count;
            while (count > 1)
            {
                int swapWith = UnityEngine.Random.RandomRangeInt(0, count);
                count--;
                T value = list[count];
                list[count] = list[swapWith];
                list[swapWith] = value;
            }
        }
    }
}
