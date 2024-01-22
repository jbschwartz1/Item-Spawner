using BepInEx;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System;
using UnityEngine.UI;

namespace ItemSpawnerUnity
{
    [BepInPlugin("com.item.spawner.unity.main", "Item Spawner Unity Main", "1.0.0")]
    public class ItemSpawnerUnityMain : BaseUnityPlugin
    {
        private static ItemSpawnerUnityMain instance;
        public static ItemSpawnerUnityMain Instance => instance;

        private const float MAX_DOTS_DISPLAY_TIME = 0.99f;
        private const float LOOKBACK_DELAY = 2.5f;
        private const float MAX_DISTANCE_TO_CASH_CIRCLE = 25f;
        private const int MAX_DOTS_BEFORE_CLEAR = 5;
        private const int LEFT_CLICK = 0;
        private const int ZERO = 0;
        private const int ONE = 1;
        private const int NEGATIVE_ONE = -1;

        public delegate int GetValueDelegate(out Component instance, out MethodInfo cashSetMethod);
        public delegate void SetValueAction(int value, Component instance, MethodInfo cashSetMethod);

        private ConfigurationManager configurationManager;
        private ItemSpawnerUnityUtilities Utilities => ItemSpawnerUnityUtilities.Instance;

        private MethodInfo showMouse;
        private MethodInfo setOverlayIndex;

        private object mouseInstance;

        private bool showPanel = false;
        public bool ShowPanel
        {
            get => showPanel;
            set => showPanel = value;
        }

        private readonly KeyCode[] NumberOfKeyCodes = {
            KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4,
            KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
            KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
            KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
        };
        private KeyCode UserPreferredKeyCodeOne => Utilities.GetKeyCode(configurationManager.UserPreferredKeyCodeOne);
        private KeyCode UserPreferredKeyCodeTwo => Utilities.GetKeyCode(configurationManager.UserPreferredKeyCodeTwo);

        private GameObject panelPrefab;
        private GameObject panelObject;
        private GameObject setActiveObject;
        private GameObject overlayManager;
        private GameObject playerPointer;
        public GameObject PlayerPointer => playerPointer;

        private Component inventoryComponent;
        private Component playerComponent;

        private Transform gameCanvas;
        private Transform cashCircle;
        public Transform GameCanvas => gameCanvas;

        private Text cashText;

        private bool lookingBack = false;
        private bool cashEditing = false;
        private bool generateDots = false;

        private void Awake()
        {
            configurationManager = ItemSpawnerUnityUtilities.Instance.GetComponentOfType("BepInEx_Manager", "ConfigurationManager") as ConfigurationManager;

            if (configurationManager == null)
            {
                Console.WriteLine("ConfigurationManager instance is null -- the mod will not load!");
                return;
            }

            var pluginPath = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, @"..\..\Bepinex\Plugins\AssetBundle\"));

            if (!Directory.Exists(pluginPath))
            {
                Console.WriteLine("AssetBundle directory not found -- the mod will not load!");
                return;
            }

            AssetBundle prefabs = AssetBundle.LoadFromFile(pluginPath + "ItemSpawnerAssets");

            if (prefabs == null)
            {
                Console.WriteLine("AssetBundle not found -- the mod will not load!");
                return;
            }

            panelPrefab = prefabs.LoadAsset("ItemSpawnCanvas") as GameObject;

            if (panelPrefab == null)
            {
                Console.WriteLine("ItemSpawnCanvas not found -- the mod will not load!");
                return;
            }

            instance = this;
        }

        private void Start()
        {
            if (instance == null)
            {
                Console.WriteLine("ItemSpawnerUnityMain instance is null -- the mod will not load!");
                return;
            }
            StartCoroutine(InitializeMouseController(LOOKBACK_DELAY));
        }

        private void Update()
        {
            if (configurationManager == null || panelPrefab == null) return;

            bool keyActivated = (Input.GetKeyUp(UserPreferredKeyCodeOne) && UserPreferredKeyCodeTwo == KeyCode.None) ||
                (Input.GetKey(UserPreferredKeyCodeOne) && Input.GetKeyUp(UserPreferredKeyCodeTwo));

            if (gameCanvas == null && !lookingBack)
            {
                Console.WriteLine("Looking back for gameCanvas...");
                StartCoroutine(DelayLookback(LOOKBACK_DELAY));
                return;
            }
            else
            {
                if (cashCircle == null) cashCircle = GameObject.Find("MasterUI/MainUI/HUDv2/Stats/MainStats/Cash/Circle").transform;
                if (cashText == null) cashText = GameObject.Find("MasterUI/MainUI/HUDv2/Stats/MainStats/Cash/Text").transform.GetComponent<Text>();
                if (playerPointer == null) playerPointer = GameObject.Find("Weed Event System/MouseCanvas/UIPointer_Player0");
            }

            if (keyActivated)
            {
                if (gameCanvas == null)
                {
                    showPanel = false;
                    if (panelObject != null) Destroy(panelObject);
                    panelObject = null;
                    Console.WriteLine("gameCanvas is null -- Destroying Created Panel!");
                    return;
                }
                showPanel = !showPanel;
                ManageKeyPress();
            }
        }

        private void LateUpdate()
        {
            if (panelPrefab == null || cashCircle == null || cashText == null || cashEditing) return;

            if (showPanel && Input.GetMouseButtonDown(LEFT_CLICK) && playerPointer != null)
            {
                float cashCircleDistanceToCursor = Vector3.Distance(playerPointer.transform.position, cashCircle.position);
                if (cashCircleDistanceToCursor < MAX_DISTANCE_TO_CASH_CIRCLE)
                {
                    StartCoroutine(ReadInputCash(cashText));
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cashEditing = false;
                generateDots = false;
            }
        }

        private IEnumerator InitializeMouseController(float wait)
        {
            while (true)
            {
                Component mouseCursor = ItemSpawnerUnityUtilities.Instance.GetComponentOfType("MouseCanvas", "MouseController");
                if (mouseCursor != null)
                {
                    showMouse = mouseCursor.GetType().GetMethod("ShowCursor", BindingFlags.Public | BindingFlags.Instance);
                    CheckPanelInitilization();
                    Console.WriteLine("MouseController found!");
                    break;
                }
                yield return new WaitForSeconds(wait);
            }
        }

        private IEnumerator DelayLookback(float wait)
        {
            lookingBack = true;
            yield return new WaitForSeconds(wait);
            lookingBack = false;
            gameCanvas = gameCanvas == null ? FindObjectsOfType<Transform>().First((x) => x.transform.name == "MainUI") : gameCanvas;
            Console.WriteLine("gameCanvas found!");
        }

        private IEnumerator ReadInputGeneric(GetValueDelegate getOriginalValue, Action<string> setText, SetValueAction setValueAction, Text dots)
        {
            cashEditing = true;
            int originalValue = getOriginalValue(out Component instance, out MethodInfo cashSetMethod);
            string userInput = "";
            StartCoroutine(DisplayDots(dots));

            while (cashEditing)
            {
                foreach (KeyCode keyCode in NumberOfKeyCodes)
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        char digitChar = (char)('0' + (keyCode - (keyCode >= KeyCode.Keypad0 ? KeyCode.Keypad0 : KeyCode.Alpha0)));
                        userInput += digitChar.ToString();
                    }
                }

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape))
                    break;

                if (userInput.Length >= ONE)
                {
                    generateDots = false;
                    setText(userInput);
                }

                yield return null;
            }

            if (int.TryParse(userInput, out int newValue) && newValue <= int.MaxValue)
            {
                setText(newValue.ToString());
                setValueAction(newValue, instance, cashSetMethod);
            }
            else
            {
                setText(originalValue.ToString());
                setValueAction(originalValue, instance, cashSetMethod);
            }

            generateDots = generateDots != false ? false : generateDots;
            cashEditing = false;
        }

        private IEnumerator ReadInputCash(Text dots)
        {
            Component component = ItemSpawnerUnityUtilities.Instance.GetComponentOfType("BepInEx_Manager", "ItemSpawnModMain");
            if (component == null) return null;
            MethodInfo getCash = component.GetType().GetMethod("GetCash", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo setCash = component.GetType().GetMethod("SetCash", BindingFlags.Instance | BindingFlags.NonPublic);
            if (getCash == null || setCash == null) return null;
            return ReadInputGeneric(
                (out Component instance, out MethodInfo cashSetMethod) =>
                {
                    cashSetMethod = setCash;
                    instance = component;
                    object value = getCash;
                    if (value is int cashValue)
                        return cashValue;
                    return ZERO;
                },
                (s) => cashText.text = s,
                (value, instance, cashSetMethod) =>
                {
                    cashSetMethod.Invoke(instance, new object[] { value });
                },
                dots
            );
        }

        private IEnumerator DisplayDots(Text textObject)
        {
            string text = "";
            textObject.text = text;
            float timer = ZERO;
            int dots = ZERO;
            generateDots = true;
            while (generateDots)
            {
                if (dots >= MAX_DOTS_BEFORE_CLEAR && timer >= (MAX_DOTS_DISPLAY_TIME - Time.deltaTime))
                {
                    text = "";
                    dots = ZERO;
                }
                if (timer >= MAX_DOTS_DISPLAY_TIME)
                {
                    text += ".";
                    dots += ONE;
                    timer = ZERO;
                }
                textObject.text = text;
                timer += Time.deltaTime;
                yield return null;
            }
        }

        private void ManageKeyPress()
        {
            if (!InitializePlayerComponent()) return;
            if (!InitializeInventoryComponent()) return;
            if (!InitializeSetActiveObject()) return;
            if (!InitializeOverlayManager()) return;
            if (!SetOverlayIndex()) return;
            OpenPanel();
        }

        private bool InitializePlayerComponent()
        {
            if (playerComponent == null)
            {
                playerComponent = ItemSpawnerUnityUtilities.Instance.GetComponentOfType("MFPlayer", "UltimateCharacterLocomotion");
                if (playerComponent == null)
                {
                    Console.WriteLine("Player Component not found!");
                    return false;
                }
            }
            return true;
        }

        private bool InitializeInventoryComponent()
        {
            if (inventoryComponent == null)
            {
                inventoryComponent = ItemSpawnerUnityUtilities.Instance.GetComponentOfType("InventoryOverlays", "OverlayManager");
                if (inventoryComponent == null)
                {
                    Console.WriteLine("Inventory Component not found!");
                    return false;
                }
            }
            return true;
        }

        private bool InitializeSetActiveObject()
        {
            if (setActiveObject == null)
            {
                setActiveObject = playerComponent.gameObject;
                if (setActiveObject == null)
                {
                    Console.WriteLine("SetActiveObject not found!");
                    return false;
                }
            }
            return true;
        }

        private bool InitializeOverlayManager()
        {
            if (overlayManager == null)
            {
                overlayManager = inventoryComponent.gameObject;
                if (overlayManager == null)
                {
                    Console.WriteLine("OverlayManager not found!");
                    return false;
                }
            }
            return true;
        }

        private bool SetOverlayIndex()
        {
            if (setOverlayIndex == null)
            {
                setOverlayIndex = inventoryComponent.GetType().GetMethod("ShowOverlay", BindingFlags.NonPublic | BindingFlags.Instance);
                if (setOverlayIndex == null)
                {
                    Console.WriteLine("ShowOverlay not found!");
                    return false;
                }
            }
            return true;
        }

        public void OpenPanel()
        {
            CheckPanelInitilization();
            if (showPanel)
            {
                if (panelObject == null || panelObject.activeSelf) return;
                ChangeOverlayState(false);
                ShowMouse(true);
                Console.WriteLine("Opening Panel!");
            }
            else
            {
                if (panelObject == null || !panelObject.activeSelf) return;
                ChangeOverlayState(true);
                ShowMouse(false);
                UpdatePanelPositionAndKeybinds();
                Console.WriteLine("Closing Panel!");
            }

        }

        private void ChangeOverlayState(bool state)
        {
            if (!state) setOverlayIndex.Invoke(inventoryComponent, new object[] { NEGATIVE_ONE });
            overlayManager.SetActive(state);
            panelObject.SetActive(!state);
        }

        private void UpdatePanelPositionAndKeybinds()
        {
            KeybindManager.Instance.CapturingKeybind = false;
            KeybindManager.Instance.OptionsPanel.SetActive(false);
            DragWindow.Instance.UpdatePanelPosition();
        }

        private void CheckPanelInitilization()
        {
            if (panelObject != null) return;
            panelObject = Instantiate(panelPrefab);
            panelObject.transform.SetParent(gameCanvas);
            panelObject.SetActive(false);
            Console.WriteLine("Panel Created!");
        }

        public void ShowMouse(bool show)
        {
            if (showMouse == null)
            {
                Console.WriteLine("ShowMouse not found!");
                return;
            }
            mouseInstance = mouseInstance == null ?  Utilities.GetComponentOfType("MouseCanvas", "MouseController") : mouseInstance;
            if (PlayerPointer.activeSelf != show) showMouse.Invoke(mouseInstance, new object[] { "Menu", show });
            setActiveObject.SetActive(!show);
        }

        public void OnDestroy()
        {
            configurationManager = null;
            showMouse = null;
            panelPrefab = null;
            panelObject = null;
            setActiveObject = null;
            gameCanvas = null;
            ItemSpawnerUnityUtilities.Instance.DestroyInstance(ItemSpawnerUnityUtilities.Instance);
            instance = null;
            Destroy(this);
        }
    }
}