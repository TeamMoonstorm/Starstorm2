using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using ProperSave;
using System.Runtime.CompilerServices;
namespace SS2
{
    public class MasterData : MonoBehaviour
    {
        public static List<MasterData> instances = new List<MasterData>();
        private static List<SaveData> saveData = new List<SaveData>();
        [SystemInitializer]
        private static void Init()
        {
            if (SS2Main.ProperSaveInstalled)
                ProperSaveInit();
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void ProperSaveInit() // british "people" when the goalie blocks a shot
        {
            ProperSave.SaveFile.OnGatherSaveData += OnGatherSaveData;
            ProperSave.Loading.OnLoadingStarted += OnLoadingStarted;
        }

        private static void OnLoadingStarted(SaveFile obj)
        {
        }

        private static void OnGatherSaveData(Dictionary<string, object> obj)
        {
        }
        private struct SaveData
        {
        }

        public uint shieldGateBonus { get; private set; }
        public uint maxHpOnKillBonus { get; private set; }
        public uint snakeEyesBonus { get; private set; }

        private CharacterMaster master;

        private void Awake()
        {
            master = base.GetComponent<CharacterMaster>();
        }

        private void OnEnable()
        {
            instances.Add(this);
        }
        private void OnDisable()
        {
            instances.Remove(this);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void LoadData()
        {

        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void OnSaveData()
        {

        }
    }
}
