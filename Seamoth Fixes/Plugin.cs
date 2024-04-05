using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TheVogels.SeamothFixMain
{
    [BepInPlugin(MyGuid, PluginName, VersionString)]
    public class SeamothFixMain : BaseUnityPlugin
    {
        private const string MyGuid = "com.thevogels.seamothfix";
        private const string PluginName = "Seamoth fix";
        private const string VersionString = "0.1.5";

        private static readonly Harmony Harmony = new Harmony(MyGuid);

        public static ManualLogSource Log;

        private void Awake()
        {
            Harmony.PatchAll();
            Logger.LogInfo(PluginName + " " + VersionString + " " + "loaded.");
            Log = Logger;
        }
    }
}
