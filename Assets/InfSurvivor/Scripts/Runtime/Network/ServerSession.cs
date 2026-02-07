using System;
using System.Net;
using InfSurvivor.Runtime.Manager;
using Shared.Session;
using UnityEngine;

namespace InfSurvivor.Runtime.Network
{
    public class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            PacketManager.Instance.CustomPacketInterceptor = (session, packet, id) =>
            {
                PacketQueue.Instance.Push(id, packet);
            };
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {

        }
        
        public void Close()
        {
            Disconnect();
        }
    }
}
