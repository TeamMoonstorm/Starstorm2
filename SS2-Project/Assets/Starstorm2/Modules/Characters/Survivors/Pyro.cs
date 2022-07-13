﻿using RoR2;
using UnityEngine;


namespace Moonstorm.Starstorm2.Survivors
{
  
    public sealed class Pyro : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("PyroBody");
        public override GameObject MasterPrefab { get; } = SS2Assets.Instance.MainAssetBundle.LoadAsset<GameObject>("PyroMonsterMaster");
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("SurvivorPyro");

        public override void ModifyPrefab()
        {
            base.ModifyPrefab();

            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }
    }
}
