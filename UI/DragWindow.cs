using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ItemSpawnerUnity
{
    public class DragWindow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private static DragWindow instance;
        public static DragWindow Instance => instance;

        [SerializeField] private RectTransform windowTransform;

        private bool isDragging = false;

        private Vector2 offset;

        private void Awake()
        {
            instance = this;
            Vector2 lastPanelLocation = ConfigurationManager.Instance.LastPanelLocation;
            if (lastPanelLocation == Vector2.zero) return;
            windowTransform.anchoredPosition = lastPanelLocation;
            Console.WriteLine("DragWindow.Awake() lastPanelLocation: " + lastPanelLocation);
        }

        public void UpdatePanelPosition()
        {
            ConfigurationManager.Instance.LastPanelLocation = windowTransform.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(windowTransform, eventData.position, eventData.pressEventCamera, out offset);

            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(windowTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 newPosition))
                {
                    newPosition -= offset;

                    windowTransform.anchoredPosition = newPosition;
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
        }

        private void OnDestroy()
        {
            isDragging = false;
            offset = Vector2.zero;
            windowTransform = null;
            instance = null;
            Destroy(this);
        }
    }
}
