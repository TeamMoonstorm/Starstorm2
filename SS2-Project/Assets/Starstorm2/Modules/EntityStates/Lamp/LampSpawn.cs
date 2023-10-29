﻿using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Lamp
{ 
    public class LampSpawn : GenericCharacterSpawnState
    {
        public static GameObject spawnVFX;
        //public static GameObject spawnVFXblue;
        public static string muzzleString;
        private Transform muzzle;
        private EffectData effectData;
        public override void OnEnter()
        {
            base.OnEnter();
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            //bool isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";
            //var effect = isBlue ? spawnVFXblue : spawnVFX;
            EffectManager.SimpleEffect(spawnVFX, new Vector3(muzzle.position.x, muzzle.position.y + 3, muzzle.position.z), muzzle.rotation, true);
        }
    }
}
