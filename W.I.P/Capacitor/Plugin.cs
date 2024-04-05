using BepInEx;
using Nautilus.Assets;
using Nautilus.Crafting;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Assets.Gadgets;

namespace TheVogels.Capacitor
{
    [BepInPlugin(MyGuid, PluginName, VersionString)]
    [BepInDependency("com.snmodding.nautilus")]
    public class Plugin : BaseUnityPlugin
    {
        private const string MyGuid = "com.thevogels.capacitor";
        private const string PluginName = "Capacitor";
        private const string VersionString = "0.1.0";

        public static PrefabInfo SInfo { get; private set; }
        public static PrefabInfo BInfo { get; private set; }

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo(PluginName + " " + VersionString + " " + "loaded.");
            SInfo = PrefabInfo
            .WithTechType
            (
                "Capacitor",
                "Capacitor (Small battery wire configuration)",
                "A capacitor that is wired to go in a small battery slot."
            )
            .WithIcon(SpriteManager.Get(TechType.Battery));

            var BasePrefab = new CustomPrefab(SInfo);
            var BaseObj = new CloneTemplate(SInfo, TechType.Battery);
            BaseObj.ModifyPrefab += obj =>
            {
                var Bat = obj.GetComponent<Battery>();
                Bat._capacity = 1f;
                Bat._charge = 0f;
            };
            BasePrefab.SetGameObject(BaseObj);
            var recipe = new RecipeData
            (
                new Ingredient(TechType.Copper, 1),
                new Ingredient(TechType.Titanium, 1)
            );

            BasePrefab.SetRecipe(recipe)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithCraftingTime(0.3f);

            BasePrefab.SetEquipment(EquipmentType.BatteryCharger);

            BInfo = PrefabInfo
            .WithTechType
            (
                "CapacitorBig",
                "Capacitor (Big battery wire configuration)",
                "A capacitor that is wired to go in a power cell slot."
            )
            .WithIcon(SpriteManager.Get(TechType.Battery));

            var BaseBPrefab = new CustomPrefab(BInfo);
            var BaseBObj = new CloneTemplate(BInfo, TechType.PowerCell);
            BaseObj.ModifyPrefab += obj =>
            {
                var Bat = obj.GetComponent<Battery>();
                Bat._capacity = 1f;
                Bat._charge = 0f;
            };
            BaseBPrefab.SetGameObject(BaseObj);
            var recipeB = new RecipeData
            (
                new Ingredient(TechType.Copper, 1),
                new Ingredient(TechType.Titanium, 1)
            );

            BaseBPrefab.SetRecipe(recipeB)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithCraftingTime(0.3f);

            BaseBPrefab.SetEquipment(EquipmentType.BatteryCharger);

            //Converting capacitors
            var recipeBC = new RecipeData
            (
                new Ingredient(BasePrefab.Info.TechType, 1)
            );
            var recipeC = new RecipeData
            (
                new Ingredient(BaseBPrefab.Info.TechType, 1)
            );
            BaseBPrefab.AddGadget(new CraftingGadget(BaseBPrefab, recipeBC));
            BasePrefab.AddGadget(new CraftingGadget(BasePrefab, recipeC));
            BaseBPrefab.Register();
            BasePrefab.Register();
        }
    }
}