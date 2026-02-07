using System.Collections.Generic;
using Shared.Packet;

namespace InfSurvivor.Runtime.Network
{
    public class PacketMessage
    {
        public PacketId Id { get; set; }
        public IPacket Packet { get; set; }
    }
    public class PacketQueue
    {
        private static PacketQueue instance = new PacketQueue();
        public static PacketQueue Instance => instance;

        private Queue<PacketMessage> packetQueue = new Queue<PacketMessage>();
        private object _lock = new object();

        public void Push(PacketId id, IPacket packet)
        {
            lock (_lock)
            {
                packetQueue.Enqueue(new PacketMessage()
                {
                    Id = id,
                    Packet = packet
                });
            }
        }

        public PacketMessage Pop()
        {
            lock (_lock)
            {
                if (packetQueue.Count == 0)
                {
                    return null;
                }

                return packetQueue.Dequeue();
            }
        }

        public List<PacketMessage> PopAll()
        {
            List<PacketMessage> list = new List<PacketMessage>();

            lock (_lock)
            {
                while (packetQueue.Count > 0)
                {
                    list.Add(packetQueue.Dequeue());
                }
            }
            return list;
        }
    }
}
