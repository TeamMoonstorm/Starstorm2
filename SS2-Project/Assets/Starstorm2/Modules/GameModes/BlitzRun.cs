using RoR2;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2
{
    public class BlitzRun : Run
    {
        // private static readonly DateTime seedEpoch = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // [SyncVar]
        // private uint serverSeedCycle;

        // public uint NetworkserverSeedCycle
        // {
        //     get => serverSeedCycle;
        //     [param: In]
        //     set => SetSyncVar(value, ref serverSeedCycle, 2048u);
        // }

        // public static uint GetCurrentSeedCycle()
        // {
        //     return (uint)(DateTime.UtcNow - seedEpoch).Days;
        // }

        // public override ulong GenerateSeedForNewRun()
        // {
        //     return (ulong)GetCurrentSeedCycle() << 32;
        // }

        public override void Start()
        {
            base.Start();

            if (NetworkServer.active)
            {
                // NetworkserverSeedCycle = GetCurrentSeedCycle();
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "SS2_GAMEMODE_BLITZ_START"
                });
            }
        }

        public override void HandlePlayerFirstEntryAnimation(CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        {
        }

        // public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        // {
        //     bool flag = base.OnSerialize(writer, forceAll);
        //     if (forceAll)
        //     {
        //         writer.WritePackedUInt32(serverSeedCycle);
        //         return true;
        //     }
        //     bool wrote = false;
        //     if ((syncVarDirtyBits & 0x800u) != 0u)
        //     {
        //         if (!wrote)
        //         {
        //             writer.WritePackedUInt32(syncVarDirtyBits);
        //             wrote = true;
        //         }
        //         writer.WritePackedUInt32(serverSeedCycle);
        //     }
        //     if (!wrote)
        //     {
        //         writer.WritePackedUInt32(syncVarDirtyBits);
        //     }
        //     return wrote || flag;
        // }

        // public override void OnDeserialize(NetworkReader reader, bool initialState)
        // {
        //     base.OnDeserialize(reader, initialState);
        //     if (initialState)
        //     {
        //         serverSeedCycle = reader.ReadPackedUInt32();
        //         return;
        //     }
        //     int num = (int)reader.ReadPackedUInt32();
        //     if ((num & 0x800) != 0)
        //     {
        //         serverSeedCycle = reader.ReadPackedUInt32();
        //     }
        // }

        // public override void PreStartClient()
        // {
        //     base.PreStartClient();
        // }
    }
}
