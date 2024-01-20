using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemSpawnerUnity
{
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform rectTransform;

        private GameObject tooltipPanel;

        private Text messageText;

        private Color panelColor = new(0.2f, 0.2f, 0.2f, 0.8f);
        private Color textColor = Color.yellow;

        private Vector2 panelPivot = new(0.5f, 0.5f);
        private Vector2 panelAnchorMin = new(0.5f, 0.505f);
        private Vector2 panelAnchorMax = new(0.5f, 0.555f);
        private Vector2 sizedelta = new(225f, 0);
        private Vector2 textAnchorMin = Vector2.zero;
        private Vector2 textAnchorMax = Vector2.one;

        private Vector3 tooltipOFfset = Vector3.up * 50f;

        public string tooltipText;

        private GameObject CreateMessagePanel()
        {
            GameObject panel = new("MessagePanel");
            panel.transform.SetParent(ItemSpawnerUnityMain.Instance.GameCanvas.gameObject.transform, false);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = panelColor;

            rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = panelAnchorMin;
            rectTransform.anchorMax = panelAnchorMax;
            rectTransform.pivot = panelPivot;
            rectTransform.sizeDelta = sizedelta;

            GameObject textObj = new("MessageText");
            textObj.transform.SetParent(panel.transform, false);

            messageText = textObj.AddComponent<Text>();
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            messageText.fontSize = 14;
            messageText.color = textColor;
            messageText.fontStyle = FontStyle.Bold;
            messageText.alignment = TextAnchor.MiddleCenter;
            messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            messageText.raycastTarget = false;
            messageText.text = tooltipText;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = textAnchorMin;
            textRect.anchorMax = textAnchorMax;

            return panel;
        }

        private void Start()
        {
            tooltipPanel = CreateMessagePanel();
            tooltipPanel.SetActive(false);
        }

        private void ShowTooltip()
        {
            if (!tooltipPanel.activeSelf)
            {
                rectTransform.position = ItemSpawnerUnityMain.Instance.PlayerPointer.transform.position + tooltipOFfset;
                tooltipPanel.SetActive(true);
            }
        }

        public void HideTooltip()
        {
            if (tooltipPanel.activeSelf)
            {
                tooltipPanel.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void OnDisable()
        {
            HideTooltip();
        }

        private void OnDestroy()
        {
            Destroy(tooltipPanel);
            Destroy(this);
        }
    }
}