using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Breadcrumbs.ItemSystem {
    public class DragItemImage : MonoBehaviour {
        [SerializeField]
        private Image itemIconImage;
        [SerializeField]
        private TextMeshProUGUI quantityText;

        private static DragItemImage instance;
        private RectTransform rectTransform;

        private void Awake() {
            instance = this;
            rectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        private void Update() {
            // 드래그 중일 때만 위치 업데이트
            if (gameObject.activeSelf) {
                rectTransform.position = Input.mousePosition;
            }
        }

        // 드래그 이미지 표시
        public static void Show(ItemData item, int quantity) {
            if (instance == null || item == null) return;

            instance.itemIconImage.sprite = item.icon;
            instance.quantityText.gameObject.SetActive(quantity > 1);
            instance.quantityText.text = quantity > 1 ? quantity.ToString() : "";

            instance.gameObject.SetActive(true);
        }

        // 드래그 이미지 숨기기
        public static void Hide() {
            if (instance == null) return;

            instance.gameObject.SetActive(false);
        }
    }
}