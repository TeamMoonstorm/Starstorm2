using RoR2.Skills;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Components
{
    public class UpgradedSkillCache : MonoBehaviour
    {
        private Dictionary<SkillDef, SkillDef> baseSkillToUpgradeMap;

        [SerializeField]
        private UpgradedSkillDef[] upgradedSkillDefs;

        void Awake()
        {
            baseSkillToUpgradeMap = new Dictionary<SkillDef, SkillDef>();

            foreach(SkillDef skill in SkillCatalog.allSkillDefs)
            {
                for(int i = 0; i < upgradedSkillDefs.Length; i++)
                {
                    if(upgradedSkillDefs[i].upgradedFrom == skill)
                    {
                        baseSkillToUpgradeMap[skill] = upgradedSkillDefs[i];
                    }
                }
            }
        }

        public SkillDef GetUpgradedSkillDef(SkillDef skillDef)
        {
            if (baseSkillToUpgradeMap.ContainsKey(skillDef))
            {
               return baseSkillToUpgradeMap[skillDef];
            }
            return null;
        }

        public static UpgradedSkillDef FindUpgradedSkillDef(SkillDef skilldef)
        {
            foreach (SkillDef catalogSkill in SkillCatalog.allSkillDefs)
            {
                if (catalogSkill is UpgradedSkillDef upgradedSkillDef)
                {
                    if (upgradedSkillDef.upgradedFrom == skilldef)
                    {
                        return upgradedSkillDef;
                    }
                }
            }
            return null;
        }
    }
}
