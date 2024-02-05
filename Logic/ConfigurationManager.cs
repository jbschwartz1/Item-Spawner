using BepInEx;
using BepInEx.Configuration;
using System;
using UnityEngine;

namespace ItemSpawnerUnity
{
    [BepInPlugin("com.item.spawner.unity.configuration", "Item Spawner Unity Configuration", "1.0.0")]
    public class ConfigurationManager : BaseUnityPlugin
    {
        private static ConfigurationManager instance;
        public static ConfigurationManager Instance => instance;

        private ConfigEntry<string> userPrefferredKeyCodeOne;
        private ConfigEntry<string> userPrefferredKeyCodeTwo;

        private ConfigEntry<Vector2> lastPanelLocation;

        private ConfigEntry<int> userPrefferredQuantityModifier;

        public string UserPreferredKeyCodeOne
        {
            get => userPrefferredKeyCodeOne.Value;
            set => userPrefferredKeyCodeOne.Value = value;
        }
        public string UserPreferredKeyCodeTwo
        {
            get => userPrefferredKeyCodeTwo.Value;
            set => userPrefferredKeyCodeTwo.Value = value;
        }
        public int UserPrefferredQuantityModifier
        {
            get => userPrefferredQuantityModifier.Value;
            set => userPrefferredQuantityModifier.Value = value;
        }

        public Vector2 LastPanelLocation
        {
            get
            {
                return lastPanelLocation.Value;
            }
            set
            {
                lastPanelLocation.Value = value;
            }
        }

        public void Awake()
        {
            instance = this;

            userPrefferredQuantityModifier = Config.Bind("Quantity Modifier",
             "QuantityModifier",
             1,
             "The Default Item Spawner allows you to spawn 1, 5, 10, 25, 50, and 100 items.\n" +
             "This modifier multiplies the default value by the multiplier.\n" +
             "For example, if the multiplier is 10, clicking 100 will spawn 1000 items.\n" +
             "This will be clamped to a range of 1 through 1000.");

            userPrefferredKeyCodeOne = Config.Bind("Key Bind One",
             "KeyBindOne",
             "LeftControl",
             "DO NOT MANUALLY CHANGE.");

            userPrefferredKeyCodeTwo = Config.Bind("Key Bind Two",
             "KeyBindTwo",
             "I",
             "DO NOT MANUALLY CHANGE.");

            lastPanelLocation = Config.Bind("Last Panel Location",
              "LastPanelLocation",
              Vector2.zero,
              "DO NOT MANUALLY CHANGE.");
        }

        public void ReloadFile()
        {
            Config.Reload();
        }

        public void OnDestroy()
        {
            userPrefferredKeyCodeOne = null;
            userPrefferredKeyCodeTwo = null;
            lastPanelLocation = null;
            instance = null;
            Destroy(this);
        }
    }
}
