using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.LampBoss
{ 
    public class LampBossSpawn : GenericCharacterSpawnState
    {
        public static GameObject spawnVFX;
        public static GameObject spawnVFXblue;
        public static string muzzleString;
        private Transform muzzle;
        private EffectData effectData;
        public override void OnEnter()
        {
            base.OnEnter();
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            bool isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";
            var effect = isBlue ? spawnVFXblue : spawnVFX;
            Util.PlaySound("WayfarerSpawn", gameObject);
            EffectManager.SimpleEffect(effect, new Vector3(muzzle.position.x, muzzle.position.y + 10, muzzle.position.z), muzzle.rotation, true);
        }
    }
}
