using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerUnity
{
    public class KeybindManager : MonoBehaviour
    {
        private static KeybindManager instance;
        public static KeybindManager Instance => instance;

        [SerializeField] private Button optionsButton;
        [SerializeField] private GameObject optionsPanel;
        public GameObject OptionsPanel
        {
            get => optionsPanel;
            set => optionsPanel = value;
        }
        [SerializeField] private TMP_Text currentKeybindText;
        [SerializeField] private TMP_InputField newKeybindOneInputField;
        [SerializeField] private TMP_InputField newKeybindTwoInputField;
        [SerializeField] private Button saveButton;

        private bool capturingKeybind = false;
        public bool CapturingKeybind
        {
            get => capturingKeybind;
            set => capturingKeybind = value;
        }

        private void Awake()
        {
            optionsButton.onClick.AddListener(OnOptionsButtonClick);
            saveButton.onClick.AddListener(OnSaveButtonClick);
            newKeybindOneInputField.onSelect.AddListener(OnInputFieldSelect);
            newKeybindTwoInputField.onSelect.AddListener(OnInputFieldSelect);
            newKeybindOneInputField.text = ConfigurationManager.Instance.UserPreferredKeyCodeOne;
            newKeybindTwoInputField.text = ConfigurationManager.Instance.UserPreferredKeyCodeTwo;
            currentKeybindText.text = $"CURRENT KEYBIND: {ConfigurationManager.Instance.UserPreferredKeyCodeOne} + {ConfigurationManager.Instance.UserPreferredKeyCodeTwo}";
            optionsPanel.SetActive(false);
            instance = this;
        }

        private void OnInputFieldSelect(string inputFieldText)
        {
            if (capturingKeybind) return;
            capturingKeybind = true;
            StartCoroutine(UpdateInputSelection());
        }

        private IEnumerator UpdateInputSelection()
        {
            string tempKeybindOne = newKeybindOneInputField.text;
            string tempKeybindTwo = newKeybindTwoInputField.text;

            while (capturingKeybind)
            {
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyUp(keyCode) && !IsNotMouseClick(keyCode))
                    {
                        if (newKeybindOneInputField.isFocused && newKeybindTwoInputField.text != $"{keyCode}")
                        {
                            capturingKeybind = false;
                            newKeybindOneInputField.text = $"{keyCode}";
                            newKeybindOneInputField.DeactivateInputField();
                            break;
                        }
                        else if (newKeybindTwoInputField.isFocused && newKeybindOneInputField.text != $"{keyCode}")
                        {
                            capturingKeybind = false;
                            newKeybindTwoInputField.text = $"{keyCode}";
                            newKeybindTwoInputField.DeactivateInputField();
                            break;
                        }
                        else
                        {
                            capturingKeybind = false;
                            newKeybindTwoInputField.text = tempKeybindTwo;
                            newKeybindOneInputField.text = tempKeybindOne;
                            newKeybindOneInputField.DeactivateInputField();
                            newKeybindTwoInputField.DeactivateInputField();
                            break;
                        }
                    }
                }
                yield return null;
            }
        }

        public void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (newKeybindOneInputField.isFocused)
                {
                    newKeybindOneInputField.DeactivateInputField();
                    capturingKeybind = false;
                }
                else if (newKeybindTwoInputField.isFocused)
                {
                    newKeybindTwoInputField.DeactivateInputField();
                    capturingKeybind = false;
                }
                optionsPanel.SetActive(false);
                ItemSpawnerUnityMain.Instance.ShowPanel = false;
                ItemSpawnerUnityMain.Instance.OpenPanel();
            }
        }

        private bool IsNotMouseClick(KeyCode keyCode)
        {
            KeyCode[] mosueClicks = new KeyCode[] { KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2, KeyCode.Mouse3,
            KeyCode.Mouse4, KeyCode.Mouse5, KeyCode.Mouse6, KeyCode.Backspace, KeyCode.Escape, KeyCode.LeftWindows, KeyCode.RightWindows,
            KeyCode.LeftApple, KeyCode.RightApple, KeyCode.LeftCommand, KeyCode.RightCommand, KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2,
            KeyCode.Alpha3, KeyCode.Alpha4
            };
            return mosueClicks.Contains(keyCode);
        }

        private void OnOptionsButtonClick()
        {
            optionsPanel.SetActive(!optionsPanel.activeSelf);
        }

        private void OnSaveButtonClick()
        {
            if (string.IsNullOrEmpty(newKeybindOneInputField.text)) return;

            if (Enum.TryParse(newKeybindOneInputField.text, out KeyCode keyCodeOne))
            {
                ConfigurationManager.Instance.UserPreferredKeyCodeOne = keyCodeOne.ToString();
            }
            else
            {
                Console.WriteLine("Invalid KeyCode for UserPreferredKeyCodeOne: " + newKeybindOneInputField.text);
                return;
            }

            if (string.IsNullOrEmpty(newKeybindTwoInputField.text))
            {
                ConfigurationManager.Instance.UserPreferredKeyCodeTwo = "None";
                currentKeybindText.text = $"CURRENT KEYBIND: {ConfigurationManager.Instance.UserPreferredKeyCodeOne}";
            }
            else
            {
                if (Enum.TryParse(newKeybindTwoInputField.text, out KeyCode keyCodeTwo))
                {
                    ConfigurationManager.Instance.UserPreferredKeyCodeTwo = keyCodeTwo.ToString();
                    currentKeybindText.text = $"CURRENT KEYBIND: {ConfigurationManager.Instance.UserPreferredKeyCodeOne} + " +
                        $"{ConfigurationManager.Instance.UserPreferredKeyCodeTwo}";
                }
                else
                {
                    Console.WriteLine("Invalid KeyCode for UserPreferredKeyCodeTwo: " + newKeybindTwoInputField.text);
                }
            }
        }

    }
}