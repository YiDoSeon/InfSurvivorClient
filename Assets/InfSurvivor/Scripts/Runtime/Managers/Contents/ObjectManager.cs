using Shared.Packet;
using Shared.Session;
using UnityEngine;

namespace InfSurvivor.Runtime.Manager
{
    public class ObjectManager
    {
    }

    public interface IObjectService
    {
        public void Add(ObjectInfo info, bool localPlayer = false);
        public void LoadPlayerResource();
        public void OnMoveHandler(PacketSession session, S_Move movePacket);
    }
}
