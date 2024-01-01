using BepInEx.Configuration;

namespace LandminePlacer;

public static class Config
{
    public static ConfigEntry<bool> InfiniteLandmines { get; private set; }
    public static ConfigEntry<int> MaxLandmines { get; private set; }
    public static ConfigEntry<int> LandminePlacerSpawnChance { get; private set; }
    public static ConfigEntry<bool> LandminePlacerInShop { get; private set; }
    public static ConfigEntry<int> LandminePlacerShopPrice { get; private set; }

    public static void Load()
    {
        var config = Plugin.ConfigFile;
        InfiniteLandmines = config.Bind(PluginInfo.PluginName, "InfiniteLandmines", false, "Infinite landmines, true = infinite, false = use MaxLandmines");
        MaxLandmines = config.Bind(PluginInfo.PluginName, "MaxLandmines", 1, "Max landmines per LandminePlacer, only used if InfiniteLandmines is false");
        LandminePlacerSpawnChance = config.Bind(PluginInfo.PluginName, "LandminePlacerSpawnChance", 80, "Chance of a LandminePlacer item spawning, higher = more common");
        LandminePlacerInShop = config.Bind(PluginInfo.PluginName, "LandminePlacerInShop", false, "Whether or not the LandminePlacer should be in the shop, true = in shop, false = not in shop");
        LandminePlacerShopPrice = config.Bind(PluginInfo.PluginName, "LandminePlacerShopPrice", 70, "Price of the LandminePlacer in the shop, only used if LandminePlacerInShop is true");
    }
}