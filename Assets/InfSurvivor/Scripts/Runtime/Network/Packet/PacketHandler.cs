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
            Debug.Log("서버와 연결되었습니다.");
            Managers.Network.OnSuccessReceiveConnectPacket();
        }
    }

    public static void S_TimeSyncHandler(PacketSession session, IPacket packet)
    {
        Managers.Network.HandleSyncTime((S_TimeSync)packet);
    }
    
    public static void S_PingHandler(PacketSession session, IPacket packet)
    {

    }

    public static void S_EnterGameHandler(PacketSession session, IPacket packet)
    {
        S_EnterGame enterGamePacket = (S_EnterGame)packet;
        Managers.Object.Add(enterGamePacket.Player, true);
    }


    public static void S_SpawnHandler(PacketSession session, IPacket packet)
    {
        S_Spawn spawnPacket = (S_Spawn)packet;
        Managers.Object.AddObjects(spawnPacket.Objects);
    }

    public static void S_DespawnHandler(PacketSession session, IPacket packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        Managers.Object.RemoveObjects(despawnPacket.ObjectIds);
    }

    public static void S_MoveHandler(PacketSession session, IPacket packet)
    {
        S_Move movePacket = (S_Move)packet;
        Managers.Object.OnMoveHandler(session, movePacket);
    }
}
