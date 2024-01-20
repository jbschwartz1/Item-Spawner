using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerUnity
{
    public class SpawnItems : MonoBehaviour
    {
        private const int MIN_QUANTITY = 1;
        private const int MAX_QUANTITY = 100;

        [SerializeField] private Button spawnButton;
        [SerializeField] private TMP_InputField itemSearch;

        Component modMainComponent;
        
        MethodInfo spawnItemsMethod;

        private void Start()
        {
            spawnButton.onClick.AddListener(() => Spawn(QuantityTracker.Instance.Quantity, ContentImageManager.Instance.LastClickedButtonTag));
            modMainComponent = ItemSpawnerUnityUtilities.Instance.GetComponentOfType("BepInEx_Manager", "ItemSpawnModMain");
            if (modMainComponent == null) return;
            spawnItemsMethod = modMainComponent.GetType().GetMethod("SpawnItem", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void Spawn(int qty, string ID)
        {
            if (qty < MIN_QUANTITY || qty > MAX_QUANTITY || ID == string.Empty) return;
            spawnItemsMethod.Invoke(modMainComponent, new object[] { ID, qty });
        }

        private void OnDestroy()
        {
            spawnButton.onClick.RemoveAllListeners();
            spawnItemsMethod = null;
            modMainComponent = null;
            Destroy(this);
        }
    }
}
