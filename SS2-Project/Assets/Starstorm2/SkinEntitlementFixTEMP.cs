using RoR2;
using RoR2.ExpansionManagement;
using RoR2.EntitlementManagement;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
namespace SS2
{
    internal static class SkinEntitlementFixTEMP
    {
        [SystemInitializer(typeof(UnlockableCatalog))]
        public static void Init()
        {
            var bodyPrefabs = SS2Content.SS2ContentPack.bodyPrefabs;
            for (int i = 0; i < bodyPrefabs.Count; i++)
            {
                var skins = BodyCatalog.GetBodySkins(BodyCatalog.FindBodyIndex(bodyPrefabs[i]));
                if(skins != null)
                {
                    for (int j = 0; j < skins.Length; j++)
                    {
                        if (skins[j].unlockableDef)
                            skins[j].unlockableDef.requiredExpansion = null;
                    }
                }
                
            }

        }
    }
}
