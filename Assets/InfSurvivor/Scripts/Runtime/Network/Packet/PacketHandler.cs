using System;
using InfSurvivor.Runtime.Manager;
using Shared.Packet;
using Shared.Session;
using UnityEngine;

public class PacketHandler
{
    public static void S_ConnectedHandler(PacketSession session, IPacket packet)
    {
        S_Connected connectedPacket = (S_Connected)packet;
        if (connectedPacket.Ok)
        {
            Managers.Network.OnSuccessReceiveConnectPacket();
        }
    }

    public static void S_EnterGameHandler(PacketSession session, IPacket packet)
    {
        S_EnterGame enterGamePacket = (S_EnterGame)packet;
    }

    public static void S_MoveHandler(PacketSession session, IPacket packet)
    {
        S_Move movePacket = (S_Move)packet;
    }

    public static void S_PingHandler(PacketSession session, IPacket packet)
    {

    }

    public static void S_SpawnHandler(PacketSession session, IPacket packet)
    {
        S_Spawn spawnPacket = (S_Spawn)packet;
    }

    public static void S_DespawnHandler(PacketSession session, IPacket packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
    }


    public static void S_TimeSyncHandler(PacketSession session, IPacket packet)
    {
        Managers.Network.HandleSyncTime((S_TimeSync)packet);
    }
}
