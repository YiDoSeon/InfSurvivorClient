using System.Collections.Generic;
using InfSurvivor.Runtime.Controller;
using Shared.Packet;
using Shared.Session;
using UnityEngine;

namespace InfSurvivor.Runtime.Manager
{
    public class ObjectManager
    {
        public LocalPlayerController LocalPlayer { get; private set; }
        private Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();
        private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
        private Dictionary<int, EnemyController> enemies = new Dictionary<int, EnemyController>();

        public void AddObjects(List<ObjectInfo> objects)
        {
            foreach (ObjectInfo info in objects)
            {
                Add(info, false);
            }
        }

        public void RemoveObjects(List<int> ids)
        {
            foreach (int id in ids)
            {
                Remove(id);
            }
        }

        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public static int GetObjectIndexById(int id)
        {
            return id & 0x00FFFFFF;
        }

        public void Add(ObjectInfo info, bool localPlayer = false)
        {
            if (LocalPlayer != null && LocalPlayer.Id == info.ObjectId)
            {
                return;
            }

            if (objects.ContainsKey(info.ObjectId))
            {
                return;
            }

            GameObjectType objectType = GetObjectTypeById(info.ObjectId);

            if (objectType == GameObjectType.Player)
            {
                if (localPlayer)
                {
                    GameObject go = Managers.Resource.Instantiate("prefabs/player", "Player");
                    go.name = info.Name;

                    LocalPlayer = go.AddComponent<LocalPlayerController>();
                    LocalPlayer.Info = info;
                    LocalPlayer.InitPos(info.PosInfo);

                    objects.Add(info.ObjectId, go);
                    players.Add(info.ObjectId, LocalPlayer);
                    Camera.main.transform.parent = go.transform;
                    Vector3 cameraLocalPos = Camera.main.transform.localPosition;
                    Camera.main.transform.localPosition = new Vector3(0f, 0f, cameraLocalPos.z);
                }
                else
                {
                    GameObject go = Managers.Resource.Instantiate("prefabs/player", "Player");
                    go.name = info.Name;

                    RemotePlayerController rpc = go.AddComponent<RemotePlayerController>();
                    rpc.Info = info;
                    rpc.InitPos(info.PosInfo);

                    objects.Add(info.ObjectId, go);
                    players.Add(info.ObjectId, rpc);
                }
            }
            else if (objectType == GameObjectType.Monster)
            {
                GameObject go = Managers.Resource.Instantiate("prefabs/monster", "Slime");
                go.name = info.Name;

                EnemyController enemy = go.AddComponent<EnemyController>();
                enemy.Info = info;
                enemy.InitPos(info.PosInfo);

                objects.Add(info.ObjectId, go);
                enemies.Add(info.ObjectId, enemy);
            }
        }

        public void Remove(int id)
        {
            if (LocalPlayer != null && LocalPlayer.Id == id)
            {
                return;
            }

            GameObjectType objectType = GetObjectTypeById(id);

            switch (objectType)
            {
                case GameObjectType.Player:
                    players.Remove(id);
                    break;
                case GameObjectType.Monster:
                    enemies.Remove(id);
                    break;
            }

            if (objects.Remove(id, out GameObject go))
            {
                Managers.Resource.Destroy(go);
            }
        }

        public void OnMoveHandler(PacketSession session, S_Move movePacket)
        {
            PlayerController pc = FindPlayerById(movePacket.ObjectId);
            if (pc == null)
            {
                return;
            }

            pc.OnUpdateMoveState(movePacket);
        }

        public void OnMeleeAttackHandler(PacketSession session, S_MeleeAttack meleeAttackPacket)
        {
            if (LocalPlayer == null)
            {
                return;
            }

            LocalPlayer.OnMeleeAttackConfirm(meleeAttackPacket);
        }

        public GameObject FindById(int id)
        {
            objects.TryGetValue(id, out GameObject go);
            return go;
        }

        public PlayerController FindPlayerById(int id)
        {
            players.TryGetValue(id, out PlayerController player);
            return player;
        }

        public EnemyController FindEnemyById(int id)
        {
            enemies.TryGetValue(id, out EnemyController enemy);
            return enemy;
        }
    }
}
