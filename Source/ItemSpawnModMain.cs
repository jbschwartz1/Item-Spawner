using BepInEx;
using BepInEx.Bootstrap;
using almost;
using UnityEngine;
using ItemSpawnerUnity;
using System;
using System.Collections.Generic;
using BTuple = System.Tuple<string, string>;
using System.Linq;
using SickscoreGames.HUDNavigationSystem;

namespace ItemSpawnerMod.Source
{
    [BepInDependency(PluginInfo.CONFIGURATION_MANAGER, BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ItemSpawnModMain : BaseUnityPlugin
    {
        public const int ONE_AND_A_HALF_BILLION = 1_500_000_000;
        public const int ONE_DOLLAR = 1;
        Dictionary<BTuple, Sprite> tempDictionary;
        List<BI> allInGameItems;
        private readonly string[] bannedCategories = [ "Grow Area", "Shop Area", "Shop Sign", "Shop Walls",
            "Grow Room Walls", "Shop Floors", "Grow Room Floors", "Door Bell", "Curtains", "Exterior Walls",
            "Storage Area", "Lab Area", "Field Area", "Smart Field" ];
        private Dictionary<string, Action<BI>> categoryActions;
        private ContentImageManager ContentImageManager => ContentImageManager.Instance;
        private void Awake()
        {
            tempDictionary = [];
            allInGameItems = [];
            Chainloader.ManagerObject.hideFlags = HideFlags.HideAndDontSave;
        }
        private void InitializeCategoryActions()
        {
            categoryActions = new Dictionary<string, Action<BI>>
            {
                { "Sativa", item => AddToCategory(item, ContentImageManager.weedCategory, null, null) },
                { "Indica", item => AddToCategory(item, ContentImageManager.weedCategory, null, null) },
                { "Hybrid", item => AddToCategory(item, ContentImageManager.weedCategory, null, null) },
                { "Danja", item => AddToCategory(item, ContentImageManager.weedCategory, null, null) },
                { "Concentrates", item => AddToCategory(item, tempDictionary, null, null) },
                { "Bongs", item => AddToCategory(item, ContentImageManager.merchCategory, null, null) },
                { "Allen Bongs", item => AddToCategory(item, ContentImageManager.merchCategory, null, null) },
                { "Allen Counters", item => AddToCategory(item, ContentImageManager.furnitureCategory, null, null) },
                { "Allen Setups", item => AddToCategory(item, ContentImageManager.componentCategory, null, null) },
                { "Allen Storage", item => AddToCategory(item, ContentImageManager.equipmentCategory, null, null) },
                { "Allen Tech", item => AddToCategory(item, ContentImageManager.equipmentCategory, null, null) },
                { "Artifacts", item => AddToCategory(item, ContentImageManager.toolCategory, null, null) },
                { "Automation", item => AddToCategory(item, ContentImageManager.toolCategory, null, null) },
                { "Balistic", item => AddToCategory(item, ContentImageManager.toolCategory, null, null) },
                { "Boosters", item => AddToCategory(item, ContentImageManager.suppliesCategory, null, null) },
                { "Collectible", item => AddToCategory(item, ContentImageManager.toolCategory, null, null) },
                { "Counters", item => AddToCategory(item, ContentImageManager.furnitureCategory, ContentImageManager.equipmentCategory, "Counter,Wait Spot") },
                { "Decorative", item => AddToCategory(item, ContentImageManager.furnitureCategory, null, null) },
                { "Displays", item => AddToCategory(item, ContentImageManager.furnitureCategory, null, null) },
                { "Extractors", item => AddToCategory(item, ContentImageManager.equipmentCategory, null, null) },
                { "Freedom N Shit", item => AddToCategory(item, ContentImageManager.toolCategory, null, null) },
                { "Healers", item => AddToCategory(item, ContentImageManager.suppliesCategory, null, null) },
                { "Melee", item => AddToCategory(item, ContentImageManager.toolCategory, null, null) },
                { "Merch", item => AddToCategory(item, ContentImageManager.merchCategory, null, null) },
                { "Paint", item => AddToCategory(item, ContentImageManager.suppliesCategory, null, null) },
                { "Pots", item => AddToCategory(item, ContentImageManager.componentCategory, null, null) },
                { "Power Shits", item => AddToCategory(item, ContentImageManager.toolCategory, null, null) },
                { "Production", item => AddToCategory(item, ContentImageManager.suppliesCategory, null, null) },
                { "Setups", item => AddToCategory(item, ContentImageManager.componentCategory, null, null) },
                { "Storage", item => AddToCategory(item, ContentImageManager.equipmentCategory, null, null) },
                { "Vapes", item => AddToCategory(item, ContentImageManager.merchCategory, null, null) },
                { "Delivery", item => AddToCategory(item, ContentImageManager.toolCategory, null, null) },
                { "Ventilation", item => AddToCategory(item, ContentImageManager.componentCategory, null, null) },
                { "Water", item => AddToCategory(item, ContentImageManager.componentCategory, null, null) },
                { "Workstations", item => AddToCategory(item, ContentImageManager.equipmentCategory, ContentImageManager.equipmentCategory, "Smart TV,Poster") },
                { "Dildos", item => AddToCategory(item, ContentImageManager.merchCategory, null, null) },
                { "Joints", item => AddToCategory(item, ContentImageManager.merchCategory, tempDictionary, "Papers,Pack") },
                { "Lights", item => AddToCategory(item, ContentImageManager.furnitureCategory, ContentImageManager.componentCategory, "Floor,Wall,Garden,Post,Orb,Flood") },
                { "Breeders", item => AddToCategory(item, ContentImageManager.equipmentCategory, ContentImageManager.suppliesCategory, "Semi-Auto Crossbreeding Pod,Manual xBreeding Pod,Auto Crossbreeding Pod") },
                { "Trash N Shit", item => AddToCategory(item, null,  ContentImageManager.toolCategory, "Trash") },
                { "Tools", item => AddToCategory(item, null, ContentImageManager.toolCategory, "Suite,RealFakeStudio,Lab Key") }
            };
        }
        private void UpdateImageDictionary()
        {
            try
            {
                InitializeCategoryActions();

                DB instance = AlmostSingleton<DB>.Instance;
                if (instance == null) return;

                allInGameItems = instance.dataBase.GetAllItems();

                foreach (BI item in allInGameItems)
                {
                    bool isBanned = item.GetType().Name == string.Empty || item.preview == null ||
                        item.category == null || bannedCategories.Contains(item.category.CategoryName);

                    if (isBanned) continue;

                    if (categoryActions.ContainsKey(item.category.CategoryName))
                    {
                        categoryActions[item.category.CategoryName](item);
                        AddToDictionary(item.name, item.ItemName, item.preview, CategoryImageManager.Instance.imageDictionary);
                    }
                    else
                    {
                        Logger.LogError("Category " + item.category.CategoryName + " not found for item " + item.ItemName);
                    }
                }
                AddJointsAtEndOfProcessing();
            }
            catch (Exception e)
            {
                Logger.LogFatal("Error in UpdateImageDictionary: " + e.Message);
            }
        }
        private void AddToCategory(BI item, Dictionary<BTuple, Sprite> categoryDictionary, Dictionary<BTuple, Sprite> backupDictionary, string bannedWords)
        {
            try
            {
                if (bannedWords == null)
                {
                    AddToDictionary(item.name, item.ItemName, item.preview, categoryDictionary);
                }
                else
                {
                    string[] bannedWordsArray = bannedWords.Split(',');
                    bool isBanned = false;
                    foreach (string word in bannedWordsArray)
                    {
                        if (item.ItemName.Contains(word))
                        {
                            isBanned = true;
                            break;
                        }
                    }
                    if (isBanned)
                    {
                        AddToDictionary(item.name, item.ItemName, item.preview, categoryDictionary);
                    }
                    else
                    {
                        AddToDictionary(item.name, item.ItemName, item.preview, backupDictionary);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogFatal("Error in AddToCategory: " + e.Message);
            }
        }
        private void AddJointsAtEndOfProcessing()
        {
            if (tempDictionary == null || tempDictionary.Count < 1) return;
            /* The duplicateJoints are Joints that show up in the tempDictionary twice. EG Joint 0 is Crappy OG Kush Doobie, but
             also has Joint 0_Indica 1 is also Crappy OG Kush Doobie. XmasJoint is XmasJoint_Sativa CC for Candy Cane Blunt. */
            /*Crappy OG Kush Doobie, Purple Haze Joint, Hindu Kush Blunt, Blueberry Cross Joint, Sour Diesel Crackwoods Blunt,
             Romulan Dolla Joint and Candy Cane Blunt (respectively to Joint 0, 1, 2, etc.. */
            string[] duplicateJoints = [ "Joint 0", "Joint 1", "Joint 2", "Joint 3", "Joint 4", "Joint 5", "XmasJoint" ];
            foreach (KeyValuePair<BTuple, Sprite> entry in tempDictionary)
            {
                if (duplicateJoints.Contains(entry.Key.Item1)) continue;
                AddToDictionary(entry.Key.Item1, entry.Key.Item2, entry.Value, ContentImageManager.weedCategory);
            }
        }
        private void AddToDictionary(string itemID, string itemName, Sprite preview, Dictionary<BTuple, Sprite> dictionary)
        {
            if (itemID == null || itemName == null || preview == null || dictionary == null) return;
            var key = new BTuple(itemID, itemName);
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, preview);
            }
        }
        private void SpawnItem(string itemID, int quantity)
        {
            ConfigurationManager.Instance.ReloadFile();
            int quantityModifier = Mathf.Clamp(ConfigurationManager.Instance.UserPrefferredQuantityModifier, 1, 1000);
            if (allInGameItems == null || allInGameItems.Count < 1) return;
            BI item = allInGameItems.Find(x => x.ID.ToLower().Equals(itemID.ToLower()));
            if (item == null) return;
            Main instance = AlmostSingleton<Main>.Instance;
            if (instance == null) return;
            instance.Inventory.Add(item, 100, quantity * quantityModifier);
        }
        private void SetCash (int cash)
        {
            if (cash < ONE_DOLLAR || cash > ONE_AND_A_HALF_BILLION) return;
            Main instance = AlmostSingleton<Main>.Instance;
            if (instance == null) return;
            instance.Cash = cash;
        }
        private int GetCash()
        {
            Main instance = AlmostSingleton<Main>.Instance;
            if (instance == null) return 0;
            return instance.Cash;
        }
        private void OnDestroy()
        {
            Chainloader.ManagerObject.hideFlags = HideFlags.None;
            tempDictionary.Clear();
            tempDictionary = null;
            allInGameItems.Clear();
            allInGameItems = null;
            categoryActions.Clear();
            categoryActions = null;
            Destroy(this);
        }
    }
}