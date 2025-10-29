using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MSU;
using RoR2.ExpansionManagement;
namespace SS2
{
    public static class SS2Util
    {
        public static ExpansionDef DLC1;
        public static ExpansionDef DLC2;
        [SystemInitializer(typeof(ExpansionCatalog))]
        private static void Init()
        {
            DLC1 = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();
            DLC2 = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();
        }

        public static bool HasFearBuff(this CharacterBody body) // :/
        {
            return body.HasBuff(SS2Content.Buffs.BuffFear) || body.HasBuff(SS2Content.Buffs.BuffFearRed);
        }

        public static float IntegrateCurve(AnimationCurve curve, float startTime, float endTime, int steps)
        {
            return Integrate(curve.Evaluate, startTime, endTime, steps);
        }

        // Integrate function f(x) using the trapezoidal rule between x=x_low..x_high
        public static float Integrate(Func<float, float> f, float x_low, float x_high, int N_steps)
        {
            float h = (x_high - x_low) / N_steps;
            float res = (f(x_low) + f(x_high)) / 2;
            for (int i = 1; i < N_steps; i++)
            {
                res += f(x_low + i * h);
            }
            return h * res;
        }

        public static int GetItemCountForPlayers(ItemDef itemDef)
        {
            if (itemDef == null)
            {
                return 0;
            }
            int count = 0;
            foreach (PlayerCharacterMasterController playerCharacterMasterController in PlayerCharacterMasterController.instances)
            {
                if (playerCharacterMasterController.master)
                {
                    int itemCount = playerCharacterMasterController.master.inventory.GetItemCount(itemDef);
                    count += itemCount;
                }
            }
            return count;
        }
        public static int GetItemCountForTeam(ItemDef itemDef, TeamIndex teamIndex)
        {
            return GetItemCountForTeam(itemDef.itemIndex, teamIndex);
        }
        public static int GetItemCountForTeam(ItemIndex itemIndex, TeamIndex teamIndex)
        {
            int count = 0;
            foreach(TeamComponent member in TeamComponent.GetTeamMembers(teamIndex))
            {
                if(member && member.body && member.body.inventory)
                {
                    count += member.body.inventory.GetItemCount(itemIndex);
                }
            }
            return count;
        }
        public static bool DoesTodayLandWithinASpecificDaysWeek(int desiredDay, int desiredMonth)
        {
            var now = DateTime.Now;
            DateTime thisYearsSpecialWeek = new DateTime(now.Year, desiredMonth, desiredDay);
            int minDay = desiredDay;
            int maxDay = desiredDay;

            switch (thisYearsSpecialWeek.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    maxDay += 6;
                    break;
                case DayOfWeek.Tuesday:
                    maxDay += 5;
                    minDay -= 1;
                    break;
                case DayOfWeek.Wednesday:
                    maxDay += 4;
                    minDay -= 2;
                    break;
                case DayOfWeek.Thursday:
                    maxDay += 3;
                    minDay -= 3;
                    break;
                case DayOfWeek.Friday:
                    maxDay += 2;
                    minDay -= 4;
                    break;
                case DayOfWeek.Saturday:
                    maxDay += 1;
                    minDay -= 5;
                    break;
                case DayOfWeek.Sunday:
                    minDay -= 6;
                    break;
            }

            DateTime minDate = new DateTime(now.Year, desiredMonth, minDay);
            DateTime maxDate = new DateTime(now.Year, desiredMonth, maxDay);

            return now >= minDate && now <= maxDate;
        }
        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException(nameof(number), "insert value between 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            return string.Empty;
        }
        public static float AmbientLevelUncapped()
        {
            if (!Run.instance) return 0f;

            var run = Run.instance;
            float num = run.GetRunStopwatch();
            DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty);
            float num3 = (float)run.participatingPlayerCount * 0.3f;
            float num4 = 0.7f + num3;
            float num6 = Mathf.Pow((float)run.participatingPlayerCount, 0.2f);
            float num7 = 0.0506f * difficultyDef.scalingValue * num6;
            float num10 = (num4 + num7 * (num * 0.016666668f)) * Mathf.Pow(1.15f, (float)run.stageClearCount);
            return (num10 - num4) / 0.33f + 1f;
        }
        #region Misc
        public static T EnsureComponent<T>(this MonoBehaviour component) where T : MonoBehaviour
        {
            var c = component.GetComponent<T>();
            return c ? c : component.gameObject.AddComponent<T>();
        }

        public static void DropShipCall(Transform origin, int tierWeight, uint teamLevel = 1, int amount = 1, ItemTier forcetier = 0, string theWorstCodeOfTheYear = null)
        {
            List<PickupIndex> dropList;
            int templevel = (int)teamLevel;
            float rarityMult = tierWeight * (MSUtil.InverseHyperbolicScaling(1, .1f, 6, templevel) / ((templevel + 14f)/ templevel)); //trust me this almost makes sense - at level 10, non white items are 1.3x more likely, and at 20, they're 2.5x more likely. additionally, reds are impossible at low enough levels
            //SS2Log.Debug(rarityMult);

            if (forcetier == ItemTier.Boss)
            {
                dropList = Run.instance.availableBossDropList;
            }
            else if (forcetier == ItemTier.Lunar)
            {
                dropList = Run.instance.availableLunarCombinedDropList;
            }
            else if (Util.CheckRoll(1 * rarityMult))
            {
                dropList = Run.instance.availableTier3DropList;
            }
            else if (Util.CheckRoll(20 * rarityMult))
            {
                dropList = Run.instance.availableTier2DropList;
            }
            else
            {
                dropList = Run.instance.availableTier1DropList;
            }


            int item = Run.instance.treasureRng.RangeInt(0, dropList.Count);

            if (amount > 1)
            {
                float angle = 360f / (float)amount;
                Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 15f + Vector3.forward * (4.75f + (.25f * amount)));

                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                for (int i = 0; i < amount; i++)
                {
                    PickupDropletController.CreatePickupDroplet(dropList[item], origin.position, vector);
                    vector = rotation * vector;
                }
                return;
            }

            if (dropList[item].itemIndex == SS2Content.Items.NkotasHeritage.itemIndex)
            {
                if (item != 0)
                {
                    item--;
                }
                else if (item != dropList.Count)
                {
                    item++;
                }
            }

            if (theWorstCodeOfTheYear != null)
            {
                PickupDropletController.CreatePickupDroplet(dropList[item], origin.position, new Vector3(0, 15, 0));
                return;
            }
            PickupDropletController.CreatePickupDroplet(dropList[item], origin.position, new Vector3(0, 15, 0));
        }

        public static void RemoveDotStacks(CharacterBody victim, DotController.DotIndex TargetedIndex, int NumberOfStacksToRemove)
        {
            DotController VictimController = DotController.FindDotController(victim.gameObject);
            if (!VictimController)
            {
                return;
            }

            for (int i = VictimController.dotStackList.Count - 1; i >= 0; i--)
            {
                DotController.DotStack dotStack = VictimController.dotStackList[i];
                if (dotStack.dotIndex == TargetedIndex)
                {
                    VictimController.RemoveDotStackAtServer(i);
                    NumberOfStacksToRemove--;

                    if (NumberOfStacksToRemove <= 0)
                    {
                        return;
                    }
                }
            }
        }

        public static bool CheckIsValidInteractable(IInteractable interactable, GameObject interactableObject)
        {
            var procFilter = interactableObject.GetComponent<InteractionProcFilter>();
            MonoBehaviour interactableAsMonobehavior = (MonoBehaviour)interactable;

            if ((bool)procFilter)
            {
                return procFilter.shouldAllowOnInteractionBeginProc;
            }
            if ((bool)interactableAsMonobehavior.GetComponent<GenericPickupController>())
            {
                return false;
            }
            if ((bool)interactableAsMonobehavior.GetComponent<VehicleSeat>())
            {
                return false;
            }
            if ((bool)interactableAsMonobehavior.GetComponent<NetworkUIPromptController>())
            {
                return false;
            }
            return true;
        }

        public static ItemDef NkotasRiggedItemDrop(int tierWeight, uint teamLevel = 1, int forcetier = 0)
        {
            List<PickupIndex> dropList;
            float rarityscale = tierWeight * (float)(Math.Sqrt(teamLevel * 13) - 4); //I have absolutely no fucking idea what this is // me neither
            if (Util.CheckRoll(0.5f * rarityscale - 1)  || (forcetier == 3 && forcetier != 0))
                dropList = Run.instance.availableTier3DropList;
            else if (Util.CheckRoll(4 * rarityscale) || (forcetier == 2 && forcetier != 0))
                dropList = Run.instance.availableTier2DropList;
            else
                dropList = Run.instance.availableTier1DropList;
            int item = Run.instance.treasureRng.RangeInt(0, dropList.Count);
            return ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(dropList[item]).itemIndex);
        }

        public static void CreateVFXDroplet(PickupIndex pickupIndex, Vector3 position, Vector3 velocity, string vfxPrefab)
        {
            var pickup = new GenericPickupController.CreatePickupInfo();

            pickup.prefabOverride = PickupCatalog.GetPickupDef(pickupIndex).dropletDisplayPrefab;

            pickup.pickupIndex = pickupIndex;
            PickupDropletController.CreatePickupDroplet(pickupIndex, position, velocity);

            EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>(vfxPrefab, SS2Bundle.All), new EffectData
            {
                rootObject = pickup.prefabOverride,
                origin = position,
                scale = 1f,
            }, true);
        }

        public static void RefreshOldestBuffStack(CharacterBody charBody, BuffDef buff, float duration)
        {
            float timer = 9999;
            int pos = -1;
            var buffs = charBody.timedBuffs;

            for (int i = 0; i < buffs.Count; ++i)
            {
                if (buffs[i].buffIndex == buff.buffIndex)
                {
                    if(buffs[i].timer < timer)
                    {
                        timer = buffs[i].timer;
                        pos = i;
                    }
                }
            }

            if(pos != -1 && pos < buffs.Count)
            {
                //if(buffs[pos].buffIndex == buff.buffIndex) //maybe this is important but i diont think so, this shouldn't take too long, so realistically the buff should still exist right? if not just uncomment this
                //{
                    buffs[pos].timer = duration;
                    //SS2Log.Info("refershed buff with def " + buff.buffIndex + ", was in slot " + pos + " out of " + buffs.Count);
                //}
            }
        }

        public static void RefreshAllBuffStacks(CharacterBody charBody, BuffDef buff, float duration)
        {
            var buffs = charBody.timedBuffs;

            foreach( CharacterBody.TimedBuff current in buffs)
            {
                if(current.buffIndex == buff.buffIndex)
                {
                    current.timer = duration;
                }
            }
        }


        [ConCommand(commandName = "one_of_each", flags = ConVarFlags.Cheat, helpText = "Grants one of each item. Format: {itemCount} {itemTier} {itemTag}")]
        public static void CmdGrantOneOfEachItem(ConCommandArgs args)
        {
            CharacterMaster master = args.GetSenderMaster();

            int itemCount = 1;
            if(args.Count > 0) int.TryParse(args[0], out itemCount);

            ItemTier argTier = (ItemTier)(-1);
            if (args.Count > 1) ItemTier.TryParse(args[1], out argTier);

            ItemTag argTag = ItemTag.Any;
            if (args.Count > 2) ItemTag.TryParse(args[2], out argTag);

            for (ItemIndex itemIndex = 0; itemIndex < (ItemIndex)ItemCatalog.allItemDefs.Length; itemIndex++)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                bool shouldGive = true;
                if (argTier != (ItemTier)(-1)) shouldGive &= itemDef.tier == argTier;
                if (argTag != ItemTag.Any) shouldGive &= itemDef.ContainsTag(argTag);
                if (!shouldGive) continue;
                try
                {
                        master.inventory.GiveItem(itemDef, itemCount);
                }
                catch (Exception e)
                {
                    SS2Log.Warning("Failed to grant ItemIndex " + itemIndex + ", " + Language.GetString(itemDef.nameToken));
                    SS2Log.Error(e);
                    continue; //????????????????????????
                }
            }
        }

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            var fields = type.GetFields();
            foreach (var field in fields) field.SetValue(copy, field.GetValue(original));
            return copy as T;
        }


        public static IEnumerator BroadcastChat(string token)
        {
            yield return new WaitForSeconds(1);
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage() { baseToken = token });
            yield break;
        }

        internal static string ScepterDescription(string desc)
        {
            return "\n<color=#d299ff>SCEPTER: " + desc + "</color>";
        }

        #endregion Misc
    }

    internal static class ArrayHelper
    {
        public static T[] Append<T>(ref T[] array, List<T> list)
        {
            var orig = array.Length;
            var added = list.Count;
            Array.Resize(ref array, orig + added);
            list.CopyTo(array, orig);
            return array;
        }

        public static Func<T[], T[]> AppendDel<T>(List<T> list) => (r) => Append(ref r, list);
    }
}