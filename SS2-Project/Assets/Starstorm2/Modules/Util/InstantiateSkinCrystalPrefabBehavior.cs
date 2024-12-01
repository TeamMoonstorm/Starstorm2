using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class InstantiateSkinCrystalPrefabBehavior : InstantiatePrefabBehavior
    {
        private SkinCrystal skinCrystal;
        private GameObjectUnlockableFilter gameObjectUnlockableFilter;

        [HeaderAttribute("Skin Crystal Properties")]
        public string bodyString;
        public int skinUnlockID;
        public UnlockableDef forbiddenUnlockableDef;
        public string achievementName;
        public void InstantiatePrefab()
        {
            if (!networkedPrefab || NetworkServer.active)
            {
                Vector3 position = targetTransform ? targetTransform.position : Vector3.zero;
                Quaternion rotation = copyTargetRotation ? targetTransform.rotation : Quaternion.identity;
                Transform parent = parentToTarget ? targetTransform : null;
                GameObject crystalPrefab = Instantiate(prefab, position, rotation, parent);

                skinCrystal = crystalPrefab.GetComponent<SkinCrystal>();
                if (skinCrystal != null)
                {
                    skinCrystal.bodyString = bodyString;
                    skinCrystal.skinUnlockID = skinUnlockID;
                }

                gameObjectUnlockableFilter = crystalPrefab.GetComponent<GameObjectUnlockableFilter>();
                if (gameObjectUnlockableFilter != null)
                {
                    gameObjectUnlockableFilter.forbiddenUnlockableDef = forbiddenUnlockableDef;
                    gameObjectUnlockableFilter.AchievementToDisable = achievementName;
                }

                if (forceActivate)
                    crystalPrefab.SetActive(true);

                if (networkedPrefab)
                    NetworkServer.Spawn(gameObject);
            }
        }
    }
}
