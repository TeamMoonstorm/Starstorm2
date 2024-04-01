using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MSU;
namespace SS2
{
    public static class SS2Util
    {
        #region Misc
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
            PickupDropletController.CreatePickupDroplet(pickup, position, velocity);

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