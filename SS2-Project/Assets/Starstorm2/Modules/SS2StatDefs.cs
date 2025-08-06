using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using RoR2.Stats;
namespace SS2.Stats
{
    public class SS2StatDefs
    {
        [SystemInitializer]
        private static void Init()
        {
            // ?
        }   
        public static readonly StatDef engiTurretKills = StatDef.Register("engiTurretKillsAchievementProgress", StatRecordType.Sum, StatDataType.ULong, 0.0, null);

        public static readonly StatDef crocoPoisonedEnemies = StatDef.Register("crocoPoisonEnemiesAchievementProgress", StatRecordType.Sum, StatDataType.ULong, 0.0, null);
    }
}
