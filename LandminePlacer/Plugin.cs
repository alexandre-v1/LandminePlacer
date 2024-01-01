using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LethalLib.Modules;
using UnityEngine;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;

namespace LandminePlacer
{
    [BepInPlugin(PluginInfo.PluginGuid, PluginInfo.PluginName, PluginInfo.PluginVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static GameObject _landmine;
        public static GameObject Landmine => TryToGetLandMine();

        public Item placeableLandmineItem;
        internal static ManualLogSource Log;
        public static ConfigFile ConfigFile;
        
        private void Awake()
        {
            Log = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PluginGuid);
            ConfigFile = Config;
            global::LandminePlacer.Config.Load();
            PatchMod();
            InitLandminePlacer();
            
            Log.LogInfo($"Plugin {PluginInfo.PluginGuid} is loaded!");
        }

        private static void PatchMod()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

        private void InitLandminePlacer()
        {
            var assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), "landmine_placer");
            var assetBundle = AssetBundle.LoadFromFile(assetDir);
            
            placeableLandmineItem = assetBundle.LoadAsset<Item>("Assets/Mods/LandminePlacer/LandminePlacer.asset");
            if (placeableLandmineItem == null)
            {
                Log.LogError("Could not load LandminePlacer item");
                return;
            }
            var placeableLandminePrefab = placeableLandmineItem.spawnPrefab;
            if (placeableLandminePrefab == null)
            {
                Log.LogError("Could not load LandminePlacer prefab");
                return;
            }
            var physicsProp = placeableLandminePrefab.GetComponent<PhysicsProp>();
            if (physicsProp == null)
            {
                Log.LogError("PhysicsProp is missing on the landmine prefab.");
                return;
            }
            var placeableLandmineScript = placeableLandminePrefab.AddComponent<LandminePlacer>();
            if (placeableLandmineScript == null)
            {
                Log.LogError("Could not add LandminePlacer script to prefab");
                return;
            }
            if (physicsProp != null)
            {
                placeableLandmineScript.InitProperties(physicsProp);
                Destroy(physicsProp);
            }
            else
            {
                Log.LogError("PhysicsProp is missing on the landmine prefab.");
            }
            NetworkPrefabs.RegisterNetworkPrefab(placeableLandmineItem.spawnPrefab);
            
            var itemRarity = global::LandminePlacer.Config.LandminePlacerSpawnChance.Value;
            Items.RegisterItem(placeableLandmineItem);
            Items.RegisterScrap(placeableLandmineItem, itemRarity, Levels.LevelTypes.All);
            if (global::LandminePlacer.Config.LandminePlacerInShop.Value)
            {
                Items.RegisterShopItem(placeableLandmineItem, global::LandminePlacer.Config.LandminePlacerShopPrice.Value);
            }
        }
        
        
        
        private static GameObject TryToGetLandMine()
        {
            if (_landmine != null)
                return _landmine;
            var spawnableMapObjects = RoundManager.Instance.spawnableMapObjects;
            if (spawnableMapObjects.Length == 0)
                return null;
            RoundManager.Instance.spawnableMapObjects = spawnableMapObjects;
            foreach (var spawnableMapObject in spawnableMapObjects)
            {
                if (spawnableMapObject.prefabToSpawn.GetComponentInChildren<Landmine>() != null)
                {
                    _landmine = spawnableMapObject.prefabToSpawn;
                    return _landmine;
                }
                else
                {
                    Log.LogError( $"Could not find default landmine prefab");
                    return null;
                }
            }
            Log.LogError( $"Could not find default landmine prefab");
            return null;
        }
    }
}