using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using R2API;
namespace SS2.Monsters
{
    public sealed class Lamp : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acLamp", SS2Bundle.Monsters);

        public static GameObject _masterPrefab;

        public static BodyIndex BodyIndex; // TYPE FIELDS!!!!!!!!!!!!!!!!!!

        public static Color barrierColor = new Color(252f / 255f, 198f / 255f, 3f / 255f);
        public static DamageColorIndex barrierDamageColor;
        public override void Initialize()
        {
            _masterPrefab = AssetCollection.FindAsset<GameObject>("LampMaster");
            RoR2Application.onLoad += () => BodyIndex = BodyCatalog.FindBodyIndex("LampBody");

            barrierDamageColor = ColorsAPI.RegisterDamageColor(barrierColor);
        }
    }
}
