using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace EntityStates.NemBandit
{
    public class OctocamoTest : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            //FindModelChild("CamoBody").GetComponent<SkinnedMeshRenderer>().material = Resources.Load<GameObject>("Prefabs/CharacterBodies/LunarGolemBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;
            Debug.Log("skill activated");
            //characterBody.GetComponentInChildren<CharacterModel>().baseRendererInfos[1].defaultMaterial = Resources.Load<GameObject>("Prefabs/CharacterBodies/LunarGolemBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            //Material octoMaterial = FindModelChild("CamoBody").GetComponent<SkinnedMeshRenderer>().material;
            //SkinnedMeshRenderer smr = FindModelChild("CamoBody").GetComponent<SkinnedMeshRenderer>();
            CharacterModel cm = characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            Debug.Log(cm);
            //Debug.Log(octoMaterial);


            RaycastHit hit;

            if (Physics.Raycast(characterBody.transform.position, Vector3.down, out hit, 4f))
            {
                //Debug.Log("Found object!");
                //Debug.Log(hit.collider.name);

                Material newMaterial = hit.collider.GetComponentInParent<MeshRenderer>().material;
                //Debug.Log(newMaterial);

                //octoMaterial = newMaterial;

                cm.baseRendererInfos[1].defaultMaterial = newMaterial;
                //smr.materials = newMaterial;
            }

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= 0.2f && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
