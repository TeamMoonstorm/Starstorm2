using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.LampBoss
{ 
    public class LampBossSpawn : GenericCharacterSpawnState
    {
        public static GameObject spawnVFX;
        public static string muzzleString;
        private Transform muzzle;
        private EffectData effectData;
        public override void OnEnter()
        {
            base.OnEnter();
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            EffectManager.SimpleEffect(spawnVFX, new Vector3(muzzle.position.x, muzzle.position.y + 10, muzzle.position.z), muzzle.rotation, true);
            //EffectManager.SimpleMuzzleFlash(spawnVFX, gameObject, muzzleString, true);
        }
    }
}
