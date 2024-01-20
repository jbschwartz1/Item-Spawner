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

        public void OnDestroy()
        {
            Console.WriteLine("ConfigurationManager.OnDestroy()");
            userPrefferredKeyCodeOne = null;
            userPrefferredKeyCodeTwo = null;
            lastPanelLocation = null;
            instance = null;
            Destroy(this);
        }
    }
}
