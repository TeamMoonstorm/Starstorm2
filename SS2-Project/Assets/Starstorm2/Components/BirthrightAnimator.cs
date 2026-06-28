using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using SS2.Items;
using Starstorm2.Components;
using UnityEngine;
using UnityEngine.Serialization;
using Object = System.Object;

namespace SS2.Components
{
    public class BirthrightAnimator : MonoBehaviour
    {
        public ItemDisplay birthrightIDR;
        public MeshRenderer scroll;
        private Material overlayMat;
        private CharacterModel characterModel;
        private float timer;
        private float closestBirthrightDistance = 1;
        private float newAlpha;
        private static readonly int propertyID = Shader.PropertyToID("_EmPower");

        private void OnEnable()
        {
            characterModel = GetComponentInParent<CharacterModel>();
            
            overlayMat = new Material(scroll.material);
            birthrightIDR.rendererInfos[0].defaultMaterial = overlayMat;
            overlayMat.SetFloat(propertyID, 0);
            
            SS2Log.Debug("birthright animator enable");
        }

        private void OnDisable()
        {
            Destroy(overlayMat);
        }

        private void Update()
        {
            if (PrimalBirthrightObjectiveToken.instanceList.Count == 0)
            {
                if (!Mathf.Approximately(newAlpha, 1))
                {
                    newAlpha = 1;
                    overlayMat.SetFloat(propertyID, newAlpha);
                }

                return;
            };
            
            timer += Time.fixedDeltaTime;
            if (timer > closestBirthrightDistance)
            {
                timer = 0;
                CharacterBody body = this.characterModel?.body;
                if (body)
                {
                    PlayRadar();
                }
                //SS2Log.Info($"################ distance from furthest birthright/75f {closestBirthrightDistance}");
            }
            
            newAlpha = ((closestBirthrightDistance - timer) / closestBirthrightDistance) * 3;
            overlayMat.SetFloat(propertyID, newAlpha);
            //SS2Log.Debug($"set overlay mat propid to {newAlpha}");
        }
        
        private void PlayRadar()
        {
            closestBirthrightDistance = float.MaxValue;
            foreach (PurchaseInteraction birthright in PrimalBirthrightObjectiveToken.instanceList.ToArray())
            {
                float distance = Vector3.Distance(characterModel.body.footPosition, birthright.transform.position);
                if (distance < closestBirthrightDistance)
                {
                    closestBirthrightDistance = distance;
                }
            }

            closestBirthrightDistance /= 75f;
            if (closestBirthrightDistance < 0.8f)
            {
                closestBirthrightDistance = 0.8f;
            }
        }
    }
}