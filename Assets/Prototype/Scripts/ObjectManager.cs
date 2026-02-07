using System.IO;
using InfSurvivor.Runtime.Manager;
using UnityEngine;

public class ObjectManager : IObjectService
{
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

        GameObject player = localCharacterManifestBundle.LoadAsset<GameObject>("Player");
        player.transform.position = Vector3.zero;
        GameObject.Instantiate(player);

        localCharacterManifestBundle.Unload(false);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Registry()
    {
        Debug.Log("Registry");
        Managers.SetObjectService(() => new ObjectManager());
    }

}
