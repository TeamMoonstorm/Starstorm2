using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2
{
    public static class SS2Util
    {
        #region Misc
        public static void DropShipCall(Transform origin, int tierWeight, uint teamLevel = 1, int amount = 1, ItemTier forcetier = 0, string theWorstCodeOfTheYear = null)
        {
            List<PickupIndex> dropList;
            float rarityscale = tierWeight * (float)(Math.Sqrt(teamLevel * 13) - 4); //I have absolutely no fucking idea what this is
            if (forcetier == ItemTier.Boss)
                dropList = Run.instance.availableLunarCombinedDropList;
            else if (forcetier == ItemTier.Lunar)
                dropList = Run.instance.availableLunarCombinedDropList;
            else if (Util.CheckRoll(0.5f * rarityscale - 1) || teamLevel >= 26 || forcetier == ItemTier.Tier3)
                dropList = Run.instance.availableTier3DropList;
            else if (Util.CheckRoll(4 * rarityscale) || teamLevel >= 13 || forcetier == ItemTier.Tier2)
                dropList = Run.instance.availableTier2DropList;
            else
                dropList = Run.instance.availableTier1DropList;
            int item = Run.instance.treasureRng.RangeInt(0, dropList.Count);

            if (amount > 1)
            {
                float angle = 360f / (float)amount;
                Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                for (int i = 0; i < amount; i++)
                {
                    if (theWorstCodeOfTheYear != null)
                        CreateVFXDroplet(dropList[item], origin.position, new Vector3(0, 0, 0), theWorstCodeOfTheYear);
                    else
                        PickupDropletController.CreatePickupDroplet(dropList[item], origin.position, vector);
                    vector = rotation * vector;
                }
                return;
            }

            if (theWorstCodeOfTheYear != null)
            {
                CreateVFXDroplet(dropList[item], origin.position, new Vector3(0, 0, 0), theWorstCodeOfTheYear);
                return;
            }
            PickupDropletController.CreatePickupDroplet(dropList[item], origin.position, new Vector3(0, 0, 0));
        }

        public static ItemDef NkotasRiggedItemDrop(int tierWeight, uint teamLevel = 1, int forcetier = 0)
        {
            List<PickupIndex> dropList;
            float rarityscale = tierWeight * (float)(Math.Sqrt(teamLevel * 13) - 4); //I have absolutely no fucking idea what this is
            if (Util.CheckRoll(0.5f * rarityscale - 1) || teamLevel >= 26 || (forcetier == 3 && forcetier != 0))
                dropList = Run.instance.availableTier3DropList;
            else if (Util.CheckRoll(4 * rarityscale) || teamLevel >= 13 || (forcetier == 2 && forcetier != 0))
                dropList = Run.instance.availableTier2DropList;
            else
                dropList = Run.instance.availableTier1DropList;
            int item = Run.instance.treasureRng.RangeInt(0, dropList.Count);
            return ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(dropList[item]).itemIndex); //This is retarded but so be it?
        }

        public static void CreateVFXDroplet(PickupIndex pickupIndex, Vector3 position, Vector3 velocity, string vfxPrefab)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(PickupDropletController.pickupDropletPrefab, position, Quaternion.identity);
            gameObject.GetComponent<PickupDropletController>().NetworkpickupIndex = pickupIndex;
            Rigidbody component = gameObject.GetComponent<Rigidbody>();
            component.velocity = velocity;
            component.AddTorque(UnityEngine.Random.Range(150f, 120f) * UnityEngine.Random.onUnitSphere);
            NetworkServer.Spawn(gameObject);
            EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>(vfxPrefab), new EffectData
            {
                origin = gameObject.transform.position,
                scale = 1f,
            }, true);
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