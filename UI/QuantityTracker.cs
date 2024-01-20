using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ItemSpawnerUnity
{
    public class QuantityTracker : MonoBehaviour
    {
        private static QuantityTracker instance;
        public static QuantityTracker Instance => instance;

        private const int QUANTITY_ONE = 1;
        private const int QUANTITY_FIVE = 5;
        private const int QUANTITY_TEN = 10;
        private const int QUANTITY_TWENTY_FIVE = 25;
        private const int QUANTITY_FIFTY = 50;
        private const int QUANTITY_ONE_HUNDRED = 100;

        private int quantity = QUANTITY_ONE;
        public int Quantity => quantity;

        [SerializeField] private Button oneButton;
        [SerializeField] private Button fiveButton;
        [SerializeField] private Button tenButton;
        [SerializeField] private Button twentyFiveButton;
        [SerializeField] private Button fiftyButton;
        [SerializeField] private Button oneHundredButton;
        [SerializeField] private TMP_Text quantityText;

        private void Start()
        {
            instance = this;
            oneButton.onClick.AddListener(() => UpdateQuantity(QUANTITY_ONE));
            fiveButton.onClick.AddListener(() => UpdateQuantity(QUANTITY_FIVE));
            tenButton.onClick.AddListener(() => UpdateQuantity(QUANTITY_TEN));
            twentyFiveButton.onClick.AddListener(() => UpdateQuantity(QUANTITY_TWENTY_FIVE));
            fiftyButton.onClick.AddListener(() => UpdateQuantity(QUANTITY_FIFTY));
            oneHundredButton.onClick.AddListener(() => UpdateQuantity(QUANTITY_ONE_HUNDRED));
        }

        public void UpdateQuantity(int qty)
        {
            quantity = qty;
            quantityText.text = $"QUANTITY TO SPAWN: {quantity}";
        }

        private void RemoveClickListeners()
        {
            Button[] buttons = new Button[]
{
                oneButton,
                fiveButton,
                tenButton,
                twentyFiveButton,
                fiftyButton,
                oneHundredButton
            };

            foreach (Button btn in buttons)
            {
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                }
            }
        }

        private void OnDestroy()
        {
            RemoveClickListeners();
            quantityText = null;
            instance = null;
            Destroy(this);
        }
    }
}
