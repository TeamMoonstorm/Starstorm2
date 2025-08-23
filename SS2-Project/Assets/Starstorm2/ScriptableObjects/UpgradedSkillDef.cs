using RoR2.Skills;
using UnityEngine;

namespace SS2
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/UpgradedSkillDef")]
    public class UpgradedSkillDef : SkillDef
    {
        //a skill override entitystate, for example, can look at assigned skills and check the catalog for a skill that the assigned skill can upgrade to
        //doing it this way instead of the reverse to have a skill upgraded from a vanilla skill, but this way we can have an upgraded skill for any skill without them having to be a certain skilldef, or assigned on a component or some such
        public SkillDef upgradedFrom;
    }
}
