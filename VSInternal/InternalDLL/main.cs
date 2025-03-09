using System;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace ProjectMultiground
{
    public class Entrypoint
    {
        static Dictionary<string, AssetBundle> bundles = new();

        public static async void OnLoad(string MetaLocation)
        {
            Debug.Log("[OnLoad] Injected!");

            try
            {
                // load core assetbundles
                await LoadCoreAssetBundles(MetaLocation);

                // load LoadingMenuScene
                Debug.Log("[OnLoad] Loading LoadingMenuScene...");
                if (!bundles.ContainsKey("loadingmenuscene"))
                {
                    throw new RowNotInTableException("[OnLoad] Failed to find loadingmenuscene bundle in bundles!");
                }

                string[] scenePaths = bundles["loadingmenuscene"].GetAllScenePaths();
                if (scenePaths.Length < 1)
                {
                    throw new IndexOutOfRangeException("[OnLoad] No scenes found in loadingmenuscene bundle!");
                }

                Debug.Log($"[OnLoad] Loading scene from path: {scenePaths[0]}");
                AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(scenePaths[0], LoadSceneMode.Additive);
                sceneLoad.completed += operation => { Debug.Log("[OnLoad] LoadingMenuScene load completed!"); };
                await LoadingInterface.AwaitOperation(sceneLoad);

                LoadingInterface.UpdateLoadingScreen("Preparing...", "Waiting on script...", 0);

                // load rest of AssetBundles
                await LoadAllAssetBundles(MetaLocation, "Preparing...", "Loading assets...");

                // load MenuScene
                Debug.Log("[OnLoad] Loading MenuScene...");
                if (!bundles.ContainsKey("menuscene"))
                {
                    throw new RowNotInTableException("[OnLoad] Failed to find menuscene bundle!");
                }

                scenePaths = bundles["menuscene"].GetAllScenePaths();
                if (scenePaths.Length < 1)
                {
                    throw new IndexOutOfRangeException("[OnLoad] No scenes found in menuscene bundle!");
                }
                AsyncOperation menuSceneLoad = SceneManager.LoadSceneAsync(scenePaths[0], LoadSceneMode.Additive);
                await LoadingInterface.UpdateLoadingScreen("Preparing...", "Loading UI...", menuSceneLoad);

                Object.Destroy(GameObject.Find("LoadingMainUI"));
            }
            catch (Exception ex)
            {
                throw new Exception($"[OnLoad] Exception occurred: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static async Task LoadCoreAssetBundles(string MetaLocation)
        {
            Debug.Log("[LoadCoreAssetBundles] Starting to load core AssetBundles..");
            foreach (string bundleName in JsonConvert.DeserializeObject<string[]>(File.ReadAllText(Path.Combine(MetaLocation, "Assets", "Bundles", "corebundles.json"))))
            {
                string bundlePath = Path.Combine(MetaLocation, "Assets", "Bundles",
                    $"{bundleName.ToLower()}.assetbundle");

                Debug.Log($"[LoadCoreAssetBundles] Loading core AssetBundle from {bundlePath}");

                string key = Path.GetFileNameWithoutExtension(bundlePath).ToLower();
                if (!bundles.ContainsKey(key))
                {
                    AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
                    await LoadingInterface.AwaitOperation(bundleRequest);

                    AssetBundle loadedBundle = bundleRequest.assetBundle;
                    if (loadedBundle == null)
                    {
                        throw new FileLoadException($"[LoadCoreAssetBundles] Failed to load core AssetBundle from {bundlePath}!");
                    }

                    bundles.Add(key, loadedBundle);
                    Debug.Log($"[LoadCoreAssetBundles] Loaded core AssetBundle '{key}' from {bundlePath}");
                }
                else
                {
                    Debug.LogWarning($"[LoadCoreAssetBundles] Core AssetBundle with key '{key}' is already loaded. Skipping...");
                }

            }
        }

        public static async Task LoadAllAssetBundles(string MetaLocation, [CanBeNull] string title, [CanBeNull] string status)
        {
            Debug.Log("[LoadAllAssetBundles] Starting to load remaining AssetBundles...");
            string[] bundlePaths = Directory.GetFiles(Path.Combine(MetaLocation, "Assets", "Bundles"),
                "*.assetbundle");
            int length = bundlePaths.Length;
            int i = 0;
            foreach (string bundlePath in bundlePaths)
            {
                i++;
                Debug.Log($"[LoadCoreAssetBundles] Loading core AssetBundle from {bundlePath}");

                string key = Path.GetFileNameWithoutExtension(bundlePath).ToLower();
                if (!bundles.ContainsKey(key))
                {
                    AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(bundlePath);
                    string newStatus = status;
                    if (!string.IsNullOrEmpty(status))
                    {
                        newStatus += $" ({i}/{length})";
                    }
                    await LoadingInterface.UpdateLoadingScreen(title, newStatus, bundle);

                    if (bundle == null)
                    {
                        Debug.LogWarning($"[LoadAllAssetBundles] Failed to load AssetBundle {i}/{length}, '{key}', from {bundlePath}!");
                        continue;
                    }

                    bundles.Add(key, bundle.assetBundle);
                    Debug.Log($"[LoadAllAssetBundles] Loaded AssetBundle {i}/{length}, '{key}', from {bundlePath}");
                }
                else
                {
                    Debug.LogWarning($"[LoadAllAssetBundles] AssetBundle {i}/{length}, with key '{key}', is already loaded. (it might be a core AssetBundle)");
                }
            }
        }
    }

    internal class LoadingInterface
    {
        internal static void SetActive(bool active)
        {
            GameObject LoadingMainUI = GameObject.Find("LoadingMainUI");
            LoadingMainUI.SetActive(active);
        }

        internal static async Task AwaitOperation(AssetBundleCreateRequest operation)
        {
            while (!operation.isDone)
            {
                await Task.Delay(50);
            }
        }
        internal static async Task AwaitOperation(AsyncOperation operation)
        {
            while (!operation.isDone)
            {
                await Task.Delay(50);
            }
        }

        internal static void UpdateLoadingScreen(string title)
        {
            TextMeshProUGUI text = GameObject.Find("LoadingMainUI/LoadingPanel/Title")
                .GetComponent<TextMeshProUGUI>();
            text.text = title;
        }
        internal static void UpdateLoadingScreen([CanBeNull] string title, [CanBeNull] string status)
        {
            if (!string.IsNullOrEmpty(title))
            {
                TextMeshProUGUI text = GameObject.Find("LoadingMainUI/LoadingPanel/Title")
                    .GetComponent<TextMeshProUGUI>();
                text.text = title;
            }
            if (!string.IsNullOrEmpty(status))
            {
                TextMeshProUGUI text = GameObject.Find("LoadingMainUI/LoadingPanel/Status")
                    .GetComponent<TextMeshProUGUI>();
                text.text = status;
            }
        }
        internal static void UpdateLoadingScreen(float proc)
        {
            Slider slider = GameObject.Find("LoadingMainUI/LoadingPanel/Slider").GetComponent<Slider>();
            slider.value = proc;
        }
        internal static async Task UpdateLoadingScreen(AsyncOperation operation)
        {
            while (!operation.isDone)
            {
                UpdateLoadingScreen(operation.progress);
                await Task.Delay(50);
            }
            UpdateLoadingScreen(1.0f);
        }
        internal static async Task UpdateLoadingScreen(AssetBundleCreateRequest operation)
        {
            while (!operation.isDone)
            {
                UpdateLoadingScreen(operation.progress);
                await Task.Delay(50);
            }
            UpdateLoadingScreen(1.0f);
        }

        internal static async Task UpdateLoadingScreen(string title, AsyncOperation operation)
        {
            UpdateLoadingScreen(title);
            await UpdateLoadingScreen(operation);
        }
        internal static async Task UpdateLoadingScreen([CanBeNull] string title, [CanBeNull] string status, AsyncOperation operation)
        {
            UpdateLoadingScreen(title, status);
            await UpdateLoadingScreen(operation);
        }
        internal static void UpdateLoadingScreen([CanBeNull] string title, [CanBeNull] string status, float proc)
        {
            UpdateLoadingScreen(title, status);
            UpdateLoadingScreen(proc);
        }
        internal static void UpdateLoadingScreen(string title, float proc)
        {
            UpdateLoadingScreen(title);
            UpdateLoadingScreen(proc);
        }
    }
}