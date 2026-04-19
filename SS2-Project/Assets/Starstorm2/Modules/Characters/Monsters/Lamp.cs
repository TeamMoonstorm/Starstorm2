using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Monsters
{
    public sealed class Lamp : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acLamp", SS2Bundle.Monsters);

        public static GameObject _masterPrefab;

        public static BodyIndex BodyIndex; // TYPE FIELDS!!!!!!!!!!!!!!!!!!
        public override void Initialize()
        {
            _masterPrefab = AssetCollection.FindAsset<GameObject>("LampMaster");
            RoR2Application.onLoad += () => BodyIndex = BodyCatalog.FindBodyIndex("LampBody");
        }

        [ConCommand(commandName = "spawn_lamp", flags = ConVarFlags.Cheat | ConVarFlags.ExecuteOnServer, helpText = "Spawns Lamp (Follower) enemies at the sender's position. Usage: spawn_lamp [count=3]")]
        public static void CCSpawnLamp(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            int count = args.TryGetArgInt(0) ?? 3;

            CharacterMaster master = args.GetSenderMaster();
            if (!master || !master.GetBody())
            {
                SS2Log.Error("spawn_lamp: No valid body found.");
                return;
            }

            if (!_masterPrefab)
            {
                SS2Log.Error("spawn_lamp: LampMaster prefab not loaded.");
                return;
            }

            Vector3 position = master.GetBody().footPosition;
            for (int i = 0; i < count; i++)
            {
                MasterSummon summon = new MasterSummon
                {
                    masterPrefab = _masterPrefab,
                    position = position + Vector3.up * 2f + Random.insideUnitSphere * 3f,
                    rotation = Quaternion.identity,
                    teamIndexOverride = TeamIndex.Monster
                };
                summon.Perform();
            }
            SS2Log.Info($"spawn_lamp: Spawned {count} Lamp(s).");
        }
    }
}
