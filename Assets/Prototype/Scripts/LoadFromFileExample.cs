using System.IO;
using UnityEngine;

public class LoadFromFileExample : MonoBehaviour
{
    private void Start()
    {
        AssetBundle localCharacterBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "local_character"));
        if (localCharacterBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }

        GameObject player = localCharacterBundle.LoadAsset<GameObject>("Player");
        player.transform.position = Vector3.zero;
        Instantiate(player);

        localCharacterBundle.Unload(false);
    }
}
