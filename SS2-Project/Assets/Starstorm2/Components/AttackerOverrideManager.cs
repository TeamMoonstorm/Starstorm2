using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
namespace Moonstorm.Starstorm2
{
    // overwrites damageInfo.attacker with a specified gameobject
    // not really tested. could be some unforeseen abuse cases
    // could be useful for chirr, instead of copying inventory. or anything else. idk
    public static class AttackerOverrideManager 
    {
        public static Dictionary<GameObject, GameObject> attackerToOverride = new Dictionary<GameObject, GameObject>(); // cringe but idc
        [SystemInitializer]
        private static void Init()
        {
            On.RoR2.HealthComponent.TakeDamage += OverrideTakeDamage;
            On.RoR2.GlobalEventManager.OnHitEnemy += OverrideOnHitEnemy;
            On.RoR2.GlobalEventManager.OnHitAll += OverrideOnHitAll;
        }

        public static void AddOverride(GameObject self, GameObject overrider)
        {
            attackerToOverride.Add(self, overrider);
        }
        public static void RemoveOverride(GameObject self)
        {
            attackerToOverride.Remove(self);
        }

        private static void OverrideOnHitAll(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, UnityEngine.GameObject hitObject)
        {
            if(attackerToOverride.Count > 0 && damageInfo.attacker && attackerToOverride.TryGetValue(damageInfo.attacker, out GameObject newAttacker))
            {
                if(newAttacker)
                    damageInfo.attacker = newAttacker;
                else
                {
                    SS2Log.Error("AttackerOverrideManager: " + damageInfo.attacker + " has an override but newAttacker is null!");
                }
            }
            orig(self, damageInfo, hitObject);
        }

        private static void OverrideOnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, UnityEngine.GameObject victim)
        {
            if (attackerToOverride.Count > 0 && damageInfo.attacker && attackerToOverride.TryGetValue(damageInfo.attacker, out GameObject newAttacker))
            {
                if (newAttacker)
                    damageInfo.attacker = newAttacker;
                else
                {
                    SS2Log.Error("AttackerOverrideManager: " + damageInfo.attacker + " has an override but newAttacker is null!");
                }
            }
            orig(self, damageInfo, victim);
        }

        private static void OverrideTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (attackerToOverride.Count > 0 && damageInfo.attacker && attackerToOverride.TryGetValue(damageInfo.attacker, out GameObject newAttacker))
            {
                if (newAttacker)
                    damageInfo.attacker = newAttacker;
                else
                {
                    SS2Log.Error("AttackerOverrideManager: " + damageInfo.attacker + " has an override but newAttacker is null!");
                }
            }
            orig(self, damageInfo);
        }
    }
}
