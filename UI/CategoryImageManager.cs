using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerUnity
{
    public class CategoryImageManager : MonoBehaviour
    {
        private static CategoryImageManager instance;
        public static CategoryImageManager Instance => instance;

        [SerializeField] private Image weedImage;
        [SerializeField] private Image suppliesImage;
        [SerializeField] private Image equipmentImage;
        [SerializeField] private Image furnitureImage;
        [SerializeField] private Image componentImage;
        [SerializeField] private Image merchImage;
        [SerializeField] private Image toolImage;
        [SerializeField] private TMP_InputField itemSearch;

        private string lastCategoryClicked = "WeedImage";
        public string LastCategoryClicked => lastCategoryClicked;

        private const int UPDATE_QUANTITY = 1;

        public Dictionary<Tuple<string, string>, Sprite> imageDictionary;

        private void Awake()
        {
            instance = this;

            imageDictionary = new Dictionary<Tuple<string, string>, Sprite>();
        }

        private void Start()
        {
            UpdatePanel();
        }

        public void UpdatePanel()
        {
            try
            {
                Component component = ItemSpawnerUnityUtilities.Instance.GetComponentOfType("BepInEx_Manager", "ItemSpawnModMain");

                if (component == null)
                {
                    Console.WriteLine("CategoryImageManager.UpdatePanel() component is null");
                    return;
                }

                MethodInfo methodInfo = component.GetType().GetMethod("UpdateImageDictionary", BindingFlags.Instance | BindingFlags.NonPublic);

                if (methodInfo == null)
                {
                    Console.WriteLine("CategoryImageManager.UpdatePanel() methodInfo is null");
                    return;
                }

                methodInfo.Invoke(component, null);

                SetImageSprite(weedImage, "The 420 Special");
                SetImageSprite(suppliesImage, "Anti Mold Hardcore");
                SetImageSprite(furnitureImage, "Cutout Street Sign");
                SetImageSprite(equipmentImage, "Retro PC");
                SetImageSprite(componentImage, "Steel Pot");
                SetImageSprite(merchImage, "Allen Abductor");
                SetImageSprite(toolImage, "Big Glocc");

                SetupTooltips();

                SetupClickListeners();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in CategoryImageManager.UpdatePanel(): {e.Message}");
            }
        }

        private void SetupTooltips()
        {
            SetupTooltipForImage(weedImage, "Weed and Seed");
            SetupTooltipForImage(suppliesImage, "Supplies");
            SetupTooltipForImage(furnitureImage, "Furniture");
            SetupTooltipForImage(equipmentImage, "Equipment");
            SetupTooltipForImage(componentImage, "Components");
            SetupTooltipForImage(merchImage, "Merchandise");
            SetupTooltipForImage(toolImage, "Random Tools");
        }

        private void SetupTooltipForImage(Image targetImage, string tooltipText)
        {
            if (targetImage == null)
            {
                Console.WriteLine("CategoryImageManager.SetupTooltipForImage() targetImage is null");
                return;
            }
            Tooltip tooltip = targetImage.gameObject.AddComponent<Tooltip>();
            tooltip.tooltipText = tooltipText;
        }

        private void SetImageSprite(Image targetImage, string imageName)
        {
            Sprite categorySprite = imageDictionary.FirstOrDefault((x) => x.Key.Item2 == imageName).Value;
            if (categorySprite == null)
            {
                Console.WriteLine($"CategoryImageManager.SetImageSprite() could not find sprite for {imageName}");
                return;
            }
            targetImage.sprite = categorySprite;
        }

        private void ImageClickHandler(string imageName)
        {
            lastCategoryClicked = imageName;
            
            QuantityTracker quantityTracker = QuantityTracker.Instance;
            if (quantityTracker == null)
            {
                Console.WriteLine("QuantityTracker is null");
                return;
            }
            quantityTracker.UpdateQuantity(UPDATE_QUANTITY);

            ContentImageManager contentImageManager = ContentImageManager.Instance;
            if (contentImageManager == null)
            {
                Console.WriteLine("ContentImageManager is null");
                return;
            }
            contentImageManager.UpdatePanel(imageName);

            itemSearch.text = string.Empty;
        }

        private void SetupClickListeners()
        {
            Image[] images = new Image[]
            {
                weedImage,
                suppliesImage,
                equipmentImage,
                furnitureImage,
                componentImage,
                merchImage,
                toolImage
            };

            foreach (Image image in images)
            {
                if (image == null)
                {
                    Console.WriteLine("Image is null");
                    continue;
                }
                if (!image.TryGetComponent<Button>(out var button))
                {
                    Console.WriteLine("Image does not have a button");
                    continue;
                }
                button.onClick.AddListener(() => ImageClickHandler(image.name));
            }
        }

        private void RemoveClickListeners()
        {
            Image[] images = new Image[]
            {
                weedImage,
                suppliesImage,
                equipmentImage,
                furnitureImage,
                componentImage,
                merchImage,
                toolImage
            };

            foreach (Image image in images)
            {
                if (image == null)
                {
                    Console.WriteLine("Image is null");
                    continue;
                }
                if (!image.TryGetComponent<Button>(out var button))
                {
                    Console.WriteLine("Image does not have a button");
                    continue;
                }
                button.onClick.RemoveAllListeners();
            }
        }

        public void AddImage(string key, Sprite image)
        {
            if (imageDictionary.ContainsKey(new Tuple<string, string>(key, key)))
            {
                Console.WriteLine($"ImageDictionary already contains key {key}");
                return;
            }
            imageDictionary.Add(new Tuple<string, string>(key, key), image);
        }

        public void OnDestroy()
        {
            RemoveClickListeners();
            imageDictionary.Clear();
            imageDictionary = null;
            itemSearch = null;
            lastCategoryClicked = null;
            instance = null;
            Destroy(this);
        }
    }
}