using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

            // load core assetbundles
            await LoadCoreAssetBundles(MetaLocation);

            // load LoadingMenuScene
            Debug.Log("[OnLoad] Loading LoadingMenuScene...");
            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(bundles["loadingmenuscene"].GetAllScenePaths()[0], LoadSceneMode.Additive);
            sceneLoad.completed += async _ =>
            {
                LoadingInterface.UpdateLoadingScreen("Preparing...", "Waiting on script...", 0);

                // load rest of AssetBundles
                await LoadAllAssetBundles(MetaLocation, "Preparing...", "Loading assets...");

                // load MenuScene
                Debug.Log("[OnLoad] Loading MenuScene...");
                AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(bundles["menuscene"].GetAllScenePaths()[0], LoadSceneMode.Additive);
                await LoadingInterface.UpdateLoadingScreen("Preparing...", "Loading UI...", sceneLoad);

                Object.Destroy(GameObject.Find("LoadingMainUI"));
            };
        }

        public static async Task LoadCoreAssetBundles(string MetaLocation)
        {
            Debug.Log("[LoadCoreAssetBundles] Starting to load core AssetBundles..");
            foreach (string bundleName in JsonConvert.DeserializeObject<string[]>(File.ReadAllText(Path.Combine(MetaLocation, "Assets", "Bundles", "corebundles.json"))))
            {
                string bundlePath = Path.Combine(MetaLocation, "Assets", "Bundles",
                    $"{bundleName.ToLower()}.assetbundle");
                AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(bundlePath);
                bundle.completed += _ =>
                {
                    if (bundle == null)
                    {
                        throw new FileLoadException($"[LoadCoreAssetBundles] Failed to load core AssetBundle from {bundlePath}!");
                    }

                    string key = Path.GetFileNameWithoutExtension(bundlePath).ToLower();
                    if (!bundles.ContainsKey(key))
                    {
                        bundles.Add(key, bundle.assetBundle);
                        Debug.Log($"[LoadCoreAssetBundles] Loaded core AssetBundle '{key}' from {bundlePath}");
                    }
                    else
                    {
                        Debug.LogWarning($"[LoadCoreAssetBundles] Core AssetBundle with key '{key}' is already loaded. Skipping...");
                    }
                };
            }
        }

        public async static Task LoadAllAssetBundles(string MetaLocation, [CanBeNull] string title, [CanBeNull] string status)
        {
            Debug.Log("[LoadAllAssetBundles] Starting to load remaining AssetBundles...");
            string[] bundlePaths = Directory.GetFiles(Path.Combine(MetaLocation, "Assets", "Bundles"),
                "*.assetbundle");
            int length = bundlePaths.Length;
            int i = 0;
            foreach (string bundlePath in bundlePaths)
            {
                i++;
                AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(bundlePath);
                if (!string.IsNullOrEmpty(status))
                {
                    status += $" ({i}/{length})";
                }
                await LoadingInterface.UpdateLoadingScreen(title, status, bundle);

                string key = Path.GetFileNameWithoutExtension(bundlePath);
                if (bundle == null || !string.IsNullOrEmpty(key))
                {
                    Debug.LogWarning($"[LoadAllAssetBundles] Failed to load AssetBundle {i}/{length}, '{key}', from {bundlePath}!");
                    continue;
                }
                key = key.ToLower();

                if (!bundles.ContainsKey(key))
                {
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

    public class LoadingInterface
    {
        public static void SetActive(bool active)
        {
            GameObject LoadingMainUI = GameObject.Find("LoadingMainUI");
            LoadingMainUI.SetActive(active);
        }

        public static void UpdateLoadingScreen(string title)
        {
            TextMeshProUGUI text = GameObject.Find("LoadingMainUI/LoadingPanel/Title")
                .GetComponent<TextMeshProUGUI>();
            text.text = title;
        }
        public static void UpdateLoadingScreen([CanBeNull] string title, [CanBeNull] string status)
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
        public static void UpdateLoadingScreen(float proc)
        {
            Slider slider = GameObject.Find("LoadingMainUI/LoadingPanel/Slider").GetComponent<Slider>();
            slider.value = proc;
        }
        public static async Task UpdateLoadingScreen(AsyncOperation operation)
        {
            while (!operation.isDone)
            {
                UpdateLoadingScreen(operation.progress);
                await Task.Delay(50);
            }
            UpdateLoadingScreen(1.0f);
        }
        public static async Task UpdateLoadingScreen(AssetBundleCreateRequest operation)
        {
            while (!operation.isDone)
            {
                UpdateLoadingScreen(operation.progress);
                await Task.Delay(50);
            }
            UpdateLoadingScreen(1.0f);
        }

        public static async Task UpdateLoadingScreen(string title, AsyncOperation operation)
        {
            UpdateLoadingScreen(title);
            await UpdateLoadingScreen(operation);
        }
        public static async Task UpdateLoadingScreen([CanBeNull] string title, [CanBeNull] string status, AsyncOperation operation)
        {
            UpdateLoadingScreen(title, status);
            await UpdateLoadingScreen(operation);
        }
        public static void UpdateLoadingScreen([CanBeNull] string title, [CanBeNull] string status, float proc)
        {
            UpdateLoadingScreen(title, status);
            UpdateLoadingScreen(proc);
        }
        public static void UpdateLoadingScreen(string title, float proc)
        {
            UpdateLoadingScreen(title);
            UpdateLoadingScreen(proc);
        }
    }
}