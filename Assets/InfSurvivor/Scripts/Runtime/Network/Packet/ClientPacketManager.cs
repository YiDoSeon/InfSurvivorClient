
using System;
using System.Collections.Generic;
using MessagePack;
using Shared.Session;
using Shared.Packet;

public class PacketManager
{
    #region Singleton
    private static PacketManager instance = new PacketManager();
    public static PacketManager Instance => instance;
    #endregion

    public PacketManager()
    {
        Register();
    }

    #region delegate
    public delegate void RawPacketHandler(PacketSession session, ArraySegment<byte> buffer, PacketId id);
    public delegate void PacketProcessor(PacketSession session, IPacket packet);
    public delegate void PacketInterceptor(PacketSession session, IPacket packet, PacketId id);
    #endregion

    private Dictionary<PacketId, RawPacketHandler> rawPacketHandlers = new Dictionary<PacketId, RawPacketHandler>();
    private Dictionary<PacketId, PacketProcessor> packetProcessors = new();

    public PacketInterceptor CustomPacketInterceptor { get; set; }

    public void Register()
    {
        
        rawPacketHandlers.Add(PacketId.S_CONNECTED, MakePacket<S_Connected>);
        packetProcessors.Add(PacketId.S_CONNECTED, PacketHandler.S_ConnectedHandler);
        rawPacketHandlers.Add(PacketId.S_PING, MakePacket<S_Ping>);
        packetProcessors.Add(PacketId.S_PING, PacketHandler.S_PingHandler);
        rawPacketHandlers.Add(PacketId.S_ENTER_GAME, MakePacket<S_EnterGame>);
        packetProcessors.Add(PacketId.S_ENTER_GAME, PacketHandler.S_EnterGameHandler);
        rawPacketHandlers.Add(PacketId.S_SPAWN, MakePacket<S_Spawn>);
        packetProcessors.Add(PacketId.S_SPAWN, PacketHandler.S_SpawnHandler);
        rawPacketHandlers.Add(PacketId.S_DESPAWN, MakePacket<S_Despawn>);
        packetProcessors.Add(PacketId.S_DESPAWN, PacketHandler.S_DespawnHandler);
        rawPacketHandlers.Add(PacketId.S_MOVE, MakePacket<S_Move>);
        packetProcessors.Add(PacketId.S_MOVE, PacketHandler.S_MoveHandler);
        rawPacketHandlers.Add(PacketId.S_TIME_SYNC, MakePacket<S_TimeSync>);
        packetProcessors.Add(PacketId.S_TIME_SYNC, PacketHandler.S_TimeSyncHandler);
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        PacketId id = (PacketId)BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        if (rawPacketHandlers.TryGetValue(id, out RawPacketHandler action))
        {
            action.Invoke(session, buffer, id);
        }
    }

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, PacketId id)
        where T : IPacket
    {
        T pkt = MessagePackSerializer.Deserialize<T>
            (
                new ReadOnlyMemory<byte>
                (
                    buffer.Array,
                    buffer.Offset + 4,
                    buffer.Count - 4
                )
            );

        if (CustomPacketInterceptor != null)
        {
            CustomPacketInterceptor.Invoke(session, pkt, id);
        }
        else
        {
            GetPacketProcessor(id)?.Invoke(session, pkt);
        }
    }

    public PacketProcessor GetPacketProcessor(PacketId id)
    {
        packetProcessors.TryGetValue(id, out PacketProcessor action);
        return action;
    }
}
