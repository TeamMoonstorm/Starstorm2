using Assets.Starstorm2.Modules.EntityStates.Knight;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Knight 
{ 
    public class SpawnPassive : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var skillLoc = GetComponent<SkillLocator>();
            var passiveSkill = skillLoc.FindSkill("passive").ExecuteIfReady();

        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
