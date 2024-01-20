using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerUnity
{
    public class ContentImageManager : MonoBehaviour
    {
        private static ContentImageManager instance;
        public static ContentImageManager Instance => instance;

        [SerializeField] private RectTransform contentRect;
        [SerializeField] private Transform mainCanvas;
        [SerializeField] private TMP_Text itemText;
        [SerializeField] private TMP_InputField itemSearch;

        private const int UPDATE_QUANTITY = 1;
        private const int UPPER_LIMIT = -170;
        private const int BOTTOM_LIMIT = -90;
        private const int NUM_OF_IMAGES = 14;
        private const int NUM_OF_IMAGES_PER_ROW = 7;
        private const int ROW_HEIGHT = 150;
        private const int ROWS_BEFORE_SCROLL = 2;
        private const int ROWS_BEFORE_SCROLL_OFFSET_0 = 0;
        private const int ROWS_BEFORE_SCROLL_OFFSET_3 = 3;

        public Dictionary<Tuple<string, string>, Sprite> weedCategory;
        public Dictionary<Tuple<string, string>, Sprite> suppliesCategory;
        public Dictionary<Tuple<string, string>, Sprite> equipmentCategory;
        public Dictionary<Tuple<string, string>, Sprite> furnitureCategory;
        public Dictionary<Tuple<string, string>, Sprite> componentCategory;
        public Dictionary<Tuple<string, string>, Sprite> merchCategory;
        public Dictionary<Tuple<string, string>, Sprite> toolCategory;
        public Dictionary<Tuple<string, string>, Sprite> imageDictionary;
        public Dictionary<Tuple<string, string>, Sprite> filteredDictionary;

        private string lastClickedButtonName = "Select an Item...";
        private string lastClickedButtonTag = "Sativa420";
        public string LastClickedButtonTag => lastClickedButtonTag;

        private void Awake()
        {
            instance = this;

            weedCategory = new Dictionary<Tuple<string, string>, Sprite>();
            suppliesCategory = new Dictionary<Tuple<string, string>, Sprite>();
            equipmentCategory = new Dictionary<Tuple<string, string>, Sprite>();
            furnitureCategory = new Dictionary<Tuple<string, string>, Sprite>();
            componentCategory = new Dictionary<Tuple<string, string>, Sprite>();
            merchCategory = new Dictionary<Tuple<string, string>, Sprite>();
            toolCategory = new Dictionary<Tuple<string, string>, Sprite>();
            imageDictionary = new Dictionary<Tuple<string, string>, Sprite>();
            filteredDictionary = new Dictionary<Tuple<string, string>, Sprite>();
        }

        private void Start()
        {
            itemText.text = $"Item: {lastClickedButtonName}";
            itemSearch.onValueChanged.AddListener(delegate { OnSearch(); });
            UpdatePanel("WeedImage");
        }

        private void OnSearch()
        {
            if (itemSearch.text == string.Empty)
            {
                UpdatePanel(CategoryImageManager.Instance.LastCategoryClicked);
                return;
            } 
            
            filteredDictionary = imageDictionary.Where(x => x.Key.Item2.ToLower().Contains(itemSearch.text.ToLower())).ToDictionary(x => x.Key, x => x.Value);

            ClearPanel();

            CreateImageObjects(filteredDictionary);

            ResetScrollPosition();
        }

        public void UpdatePanel(string category)
        {
            ClearPanel();

            SetImageDictionary(category);

            CreateImageObjects(imageDictionary);

            ResetScrollPosition();
        }

        private void ResetScrollPosition()
        {
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, UPPER_LIMIT);
        }

        private void ClearPanel()
        {
            foreach (Transform child in contentRect.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void SetImageDictionary(string category)
        {
            imageDictionary = null;
            imageDictionary = new Dictionary<Tuple<string, string>, Sprite>();

            switch (category)
            {
                case "WeedImage":
                    imageDictionary = weedCategory;
                    break;
                case "SuppliesImage":
                    imageDictionary = suppliesCategory;
                    break;
                case "EquipmentImage":
                    imageDictionary = equipmentCategory;
                    break;
                case "FurnitureImage":
                    imageDictionary = furnitureCategory;
                    break;
                case "ComponentImage":
                    imageDictionary = componentCategory;
                    break;
                case "MerchImage":
                    imageDictionary = merchCategory;
                    break;
                case "ToolImage":
                    imageDictionary = toolCategory;
                    break;
            }
        }

        private void CreateImageObjects(Dictionary<Tuple<string, string>, Sprite> dictionary)
        {
            foreach (KeyValuePair<Tuple<string, string>, Sprite> pair in dictionary)
            {
                GameObject imageObject = new();
                imageObject.AddComponent<Image>().sprite = pair.Value;
                imageObject.AddComponent<Button>().onClick.AddListener(delegate { OnClick(imageObject, pair.Key.Item1); });
                imageObject.transform.SetParent(contentRect);
                imageObject.transform.localScale = Vector3.one;
                imageObject.name = pair.Key.Item2;
                Tooltip tooltip = imageObject.AddComponent<Tooltip>();
                tooltip.tooltipText = pair.Key.Item2;
            }
        }


        public void OnClick(GameObject imageObject, string itemID)
        {
            QuantityTracker.Instance.UpdateQuantity(UPDATE_QUANTITY);
            lastClickedButtonName = imageObject.name;
            lastClickedButtonTag = itemID;
            itemText.text = $"Item: {lastClickedButtonName}";
        }

        private void LateUpdate()
        {
            LimitScrollViewWindow();
        }

        private void LimitScrollViewWindow()
        {


            float upperLimit = UPPER_LIMIT;
            int numOfImages = contentRect.GetComponentsInChildren<Image>().Count();
            float bottomLimit;

            if (numOfImages <= NUM_OF_IMAGES)
            {
                bottomLimit = upperLimit;
            }
            else
            {
                int completeRows = numOfImages / NUM_OF_IMAGES_PER_ROW;
                int remainder = numOfImages % NUM_OF_IMAGES_PER_ROW;

                if (completeRows > ROWS_BEFORE_SCROLL)
                {
                    if (remainder == ROWS_BEFORE_SCROLL_OFFSET_0)
                    {
                        bottomLimit = BOTTOM_LIMIT + (ROW_HEIGHT * (completeRows - ROWS_BEFORE_SCROLL_OFFSET_3));
                    }
                    else
                    {
                        bottomLimit = BOTTOM_LIMIT + (ROW_HEIGHT * (completeRows - ROWS_BEFORE_SCROLL));
                    }
                }
                else
                {
                    bottomLimit = BOTTOM_LIMIT;
                }
            }

            if (contentRect.anchoredPosition.y < upperLimit)
            {
                contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, upperLimit);
            }

            if (contentRect.anchoredPosition.y > bottomLimit)
            {
                contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, bottomLimit);
            }
        }

        private void OnDestroy()
        {
            Console.WriteLine("ContentImageManager.OnDestroy()");
            weedCategory = null;
            suppliesCategory = null;
            equipmentCategory = null;
            furnitureCategory = null;
            componentCategory = null;
            merchCategory = null;
            toolCategory = null;
            imageDictionary = null;
            instance = null;
            Destroy(this);
        }
    }
}