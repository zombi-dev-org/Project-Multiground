
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectMultiground
{
    public class Entrypoint
    {
        public static Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
        public static void OnLoad(string MetaLocation)
        {
            // load AssetBundles
            foreach (string bundlePath in Directory.GetFiles(Path.Combine(MetaLocation, "Assets", "Bundles"), "*.assetbundle"))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle == null)
                {
                    Debug.Log($"Failed to load AssetBundle from {bundlePath}!");
                    continue;
                }

                string key = Path.GetFileNameWithoutExtension(bundlePath).ToLower();
                if (!bundles.ContainsKey(key))
                {
                    bundles.Add(key, bundle);
                    Debug.Log($"Loaded AssetBundle '{key}' from {bundlePath}");
                }
                else
                {
                    Debug.LogWarning($"AssetBundle with key '{key}' is already loaded.");
                }
            }

            // load MainScene
            string[] scenePath = bundles["mainscene"].GetAllScenePaths();
            SceneManager.LoadSceneAsync(scenePath[0], LoadSceneMode.Additive);
        }
    }
}
