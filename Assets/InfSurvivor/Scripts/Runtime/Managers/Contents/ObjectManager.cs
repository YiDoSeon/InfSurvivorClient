using Shared.Packet;
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
    }
}
