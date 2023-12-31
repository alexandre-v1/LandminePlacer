using BepInEx.Configuration;

namespace LandminePlacer;

public static class Config
{
    public static ConfigEntry<bool> InfiniteLandmines { get; private set; }
    public static ConfigEntry<int> MaxLandmines { get; private set; }
    public static ConfigEntry<int> LandminePlacerSpawnChance { get; private set; }

    public static void Load()
    {
        var config = Plugin.ConfigFile;
        InfiniteLandmines = config.Bind(PluginInfo.PluginName, "InfiniteLandmines", false, "Infinite landmines, true = infinite, false = use MaxLandmines");
        MaxLandmines = config.Bind(PluginInfo.PluginName, "MaxLandmines", 1, "Max landmines per LandminePlacer, only used if InfiniteLandmines is false");
        LandminePlacerSpawnChance = config.Bind(PluginInfo.PluginName, "LandminePlacerSpawnChance", 40, "Chance of a LandminePlacer item spawning, higher = more common");
    }
}