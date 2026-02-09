using System.Collections.Generic;
using System.IO;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using Shared.Session;
using UnityEngine;

public class ObjectManager : IObjectService
{
    public PlayerController LocalPlayer { get; private set; }
    private Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();
    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();

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
                GameObject go = GameObject.Instantiate(playerPrefab);
                LocalPlayer = go.AddComponent<LocalPlayerController>();
                LocalPlayer.Info = info;
                LocalPlayer.InitPos(info.PosInfo);

                objects.Add(info.ObjectId, go);
                players.Add(info.ObjectId, LocalPlayer);
                Camera.main.transform.parent = go.transform;
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
        else if (objectType == GameObjectType.Monster)
        {
            GameObject monsterPrefab = LoadObject<GameObject>("prefabs/monster", "Slime");
            GameObject go = GameObject.Instantiate(monsterPrefab);
            go.name = info.Name;
            go.transform.position = info.PosInfo.Pos.ToUnityVector2();
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
    
    #region 리소스 로드 및 캐싱 (TODO: ResourceManager 이관)
    private AssetBundleManifest manifest;
    private Dictionary<string, AssetBundle> bundleCache = new Dictionary<string, AssetBundle>();
    private Dictionary<string, UnityEngine.Object> assetCache = new Dictionary<string, UnityEngine.Object>();
    private GameObject playerPrefab;

    public T LoadObject<T>(string bundleName, string assetName) where T : UnityEngine.Object
    {
        LoadManifest();
        string assetKey = $"{bundleName}_{assetName}";
        if (assetCache.TryGetValue(assetKey, out UnityEngine.Object cachedAsset))
        {
            return assetCache[assetKey] as T;
        }

        AssetBundle bundle = LoadAssetBundle(bundleName);
        if (bundle == null)
        {
            return null;
        }

        T asset = bundle.LoadAsset<T>(assetName);
        if (asset != null)
        {
            assetCache.Add(assetKey, asset);
        }

        return asset;
    }

    private AssetBundle LoadAssetBundle(string bundleName)
    {
        if (bundleCache.TryGetValue(bundleName, out AssetBundle bundle))
        {
            return bundle;
        }

        string[] dependencies = manifest.GetAllDependencies(bundleName);
        foreach (var dep in dependencies)
        {
            if (bundleCache.TryGetValue(dep, out AssetBundle depBundle) == false)
            {
                depBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, dep));
                bundleCache.Add(dep, depBundle);
            }
        }

        bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleName));
        bundleCache.Add(bundleName, bundle);
        return bundle;
    }

    private void LoadManifest()
    {
        if (manifest != null)
        {
            return;
        }
        AssetBundle baseBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "StandaloneWindows"));
        manifest = baseBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

    }
    // 임시
    public void LoadPlayerResource()
    {
        playerPrefab = LoadObject<GameObject>("prefabs/player", "Player");
    }

    #endregion
}
