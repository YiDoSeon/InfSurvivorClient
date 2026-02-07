using System.Collections.Generic;
using System.IO;
using InfSurvivor.Runtime.Manager;
using Shared.Packet;
using Shared.Session;
using UnityEngine;

public class ObjectManager : IObjectService
{
    public PlayerController LocalPlayer { get; private set; }
    private Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();
    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();

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

        if (localPlayer)
        {
            GameObject go = GameObject.Instantiate(playerPrefab);
            LocalPlayer = go.AddComponent<LocalPlayerController>();
            LocalPlayer.Info = info;
            LocalPlayer.InitPos(info.PosInfo);

            objects.Add(info.ObjectId, go);
            players.Add(info.ObjectId, LocalPlayer);
        }
        else
        {
            GameObject go = GameObject.Instantiate(playerPrefab);
            RemotePlayerController rpc = go.AddComponent<RemotePlayerController>();
            rpc.Info = info;
            rpc.InitPos(info.PosInfo);

            objects.Add(info.ObjectId, go);
            players.Add(info.ObjectId, rpc);
        }
    }

    public void Remove(int id)
    {
        if (LocalPlayer != null && LocalPlayer.Id == id)
        {
            return;
        }

        if (objects.ContainsKey(id) == false)
        {
            return;
        }

        GameObject go = FindById(id);
        if (go == null)
        {
            return;
        }

        objects.Remove(id);

        PlayerController pc = FindPlayerById(id);
        if (pc == null)
        {
            return;
        }
        players.Remove(id);

        // TODO: 리소스매니저에서 제거처리
        GameObject.Destroy(go);
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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Registry()
    {
        Managers.SetObjectService(() => new ObjectManager());
    }

    private GameObject playerPrefab;
    // 임시
    public void LoadPlayerResource()
    {
        AssetBundle standaloneWindows = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "StandaloneWindows"));
        AssetBundleManifest standaloneWindowsManifest = standaloneWindows.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        string[] dependencies = standaloneWindowsManifest.GetAllDependencies("local_character");

        foreach (string dep in dependencies)
        {
            Debug.Log(dep);
        }

        AssetBundle localCharacterManifestBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "local_character"));
        if (localCharacterManifestBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }

        playerPrefab = localCharacterManifestBundle.LoadAsset<GameObject>("Player");
        // GameObject player = localCharacterManifestBundle.LoadAsset<GameObject>("Player");
        // player.transform.position = Vector3.zero;
        // GameObject.Instantiate(player);

        localCharacterManifestBundle.Unload(false);
    }

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
}
