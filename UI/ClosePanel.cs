using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemSpawnerUnity
{
    public class ClosePanel : MonoBehaviour
    {
        public void Close()
        {
            KeybindManager.Instance.CapturingKeybind = false;
            KeybindManager.Instance.OptionsPanel.SetActive(false);
            ItemSpawnerUnityMain.Instance.ShowPanel = false;
            ItemSpawnerUnityMain.Instance.OpenPanel();
        }

        private void OnDestroy()
        {
            Destroy(this);
        }
    }
}
