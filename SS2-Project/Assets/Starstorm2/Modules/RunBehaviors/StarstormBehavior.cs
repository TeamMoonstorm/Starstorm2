using R2API;
using R2API.Utils;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Moonstorm.Starstorm2.Components
{
    public class StarstormBehavior : MonoBehaviour
    {
        public static StarstormBehavior instance { get; private set; }
        private EtherealBehavior ethInstance;
        public Run run;

        internal static void Init()
        {
 
        }

        private void Awake()
        {
            run = GetComponentInParent<Run>();
        }

        private void Start()
        {
            instance = this;
            ethInstance = GetComponent<EtherealBehavior>();
            if (ethInstance == null)
                SS2Log.Debug("Failed to find Ethereal run behavior!");

            //On.RoR2.SceneDirector.Start += SceneDirector_Start;
            //On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
            CharacterBody.onBodyStartGlobal += EliteSpawn;
        }


        private void OnDestroy()
        {
            //On.RoR2.SceneDirector.Start -= SceneDirector_Start;
            //On.RoR2.CombatDirector.Awake -= CombatDirector_Awake;
            CharacterBody.onBodyStartGlobal -= EliteSpawn;
        }

        public void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
        }

        public void EliteSpawn(CharacterBody body)
        {
            if (body.teamComponent?.teamIndex != TeamIndex.Player && !body.isChampion)
            {
                if (Util.CheckRoll(5f * ethInstance.etherealsCompleted))
                {
                    Debug.Log("spawning ethereal!");
                    MakeEthereal(body);
                }
            }
        }

        public void MakeEthereal(CharacterBody body)
        {
            //Debug.Log("starting");

            var inventory = body.inventory;

            //CombatDirector.instancesList[0].monsterCredit -= 50;
            //IDK LOL

            //Debug.Log("giving items");

            inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(40 + (40 * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.BoostCooldowns, 30);
            inventory.GiveItem(RoR2Content.Items.BoostDamage, (int)(10 + (10 * ethInstance.etherealsCompleted)));
            inventory.GiveItem(SS2Content.Items.EtherealItemAffix);

            //Debug.Log("modifying deaht rewards");

            DeathRewards rewards = body.GetComponent<DeathRewards>();
            if (rewards)
            {
                rewards.expReward *= (uint)(2 + (2 * ethInstance.etherealsCompleted));
                rewards.goldReward *= (uint)(2 + (2 * ethInstance.etherealsCompleted));
            }

            //Debug.Log("done!");
        }

        public void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            /*orig(self);
            Debug.Log("I'M WAKING UP");
            Debug.Log("TO ASH AND DUST");
            Debug.Log("CHECK THIS SHIT OUT:");
            Debug.Log(self.eliteBias + " : ELITE BIAS");
            Debug.Log(self.consecutiveCheapSkips + " : CONSEC CHEAP SKIPS");
            Debug.Log(self.moneyWaves + " : MONEY WAVES");
            Debug.Log(self.moneyWaveIntervals + " : MONEY WAVE INTERVALS");
            Debug.Log(self.creditMultiplier + " : CREDIT MULTIPLIER");
            Debug.Log("KTHXBAI");

            if (ethInstance != null)
            {

            }*/
        }
    }
}
