using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace InfSurvivor.Runtime.Manager
{
    public class ResourceManager
    {
        private AssetBundleManifest manifest;
        private Dictionary<string, AssetBundle> bundleCache = new Dictionary<string, AssetBundle>();
        private Dictionary<string, UnityEngine.Object> assetCache = new Dictionary<string, UnityEngine.Object>();

        public GameObject Instantiate(string bundleName, string assetName)
        {
            GameObject go = LoadObject<GameObject>(bundleName, assetName);
            Debug.Assert(go != null, $"bundle [{bundleName}]이 없거나 asset [{assetName}]이 없습니다");
            return GameObject.Instantiate(go);
        }

        public void Destroy(GameObject go)
        {
            GameObject.Destroy(go);
        }

        #region Asset Bundle

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

        #endregion
    }
}
