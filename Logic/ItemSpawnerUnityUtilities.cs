using BepInEx.Logging;
using System;
using UnityEngine;

namespace ItemSpawnerUnity
{
    public class ItemSpawnerUnityUtilities
    {
        private static ItemSpawnerUnityUtilities instance;

        public static ItemSpawnerUnityUtilities Instance
        {
            get
            {
                instance ??= new ItemSpawnerUnityUtilities();
                return instance;
            }
        }

        public Component GetComponentOfType(string rootObject, string rootComponent)
        {
            GameObject gameObject = GameObject.Find(rootObject);

            if (gameObject == null)
            {
                Console.WriteLine("GameObject " + rootObject + " not found");
                return null;
            }

            Component[] rootComponents = gameObject.GetComponents<Component>();

            foreach (Component component in rootComponents)
            {
                if (component.GetType().Name == rootComponent)
                {
                    return component;
                }
            }
            Console.WriteLine("Component " + rootComponent + " not found");
            return null;
        }

        public KeyCode GetKeyCode(string key)
        {
            try
            {
                return (KeyCode)Enum.Parse(typeof(KeyCode), key);
            }
            catch
            {
                Console.WriteLine("Key " + key + " not found");
                return KeyCode.None;
            }
        }

        public void DestroyInstance<T>(T instance) where T : class
        {
            if (instance != null) instance = null;
        }

        public bool CheckForNull<T>(T item, string errorMessage, ManualLogSource logger)
        {
            if (item == null)
            {
                Console.WriteLine(errorMessage);
                return true;
            }
            return false;
        }
    }
}
