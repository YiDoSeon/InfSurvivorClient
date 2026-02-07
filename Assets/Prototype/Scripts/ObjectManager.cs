using System.IO;
using InfSurvivor.Runtime.Manager;
using Shared.Packet;
using UnityEngine;

public class ObjectManager : IObjectService
{
    public PlayerController LocalPlayer { get; private set; }


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

        // TODO: 원격 클라이언트 생성 예외 처리

        GameObjectType objectType = GetObjectTypeById(info.ObjectId);

        if (localPlayer)
        {
            GameObject go = GameObject.Instantiate(playerPrefab);
            LocalPlayer = go.GetComponent<PlayerController>();
            LocalPlayer.Info = info;
        }
        else
        {

        }

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
}
