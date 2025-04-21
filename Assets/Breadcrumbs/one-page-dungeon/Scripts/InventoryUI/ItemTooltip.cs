using TMPro;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    public class ItemTooltip : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI itemNameText;
        [SerializeField]
        private TextMeshProUGUI rarityText;
        [SerializeField]
        private TextMeshProUGUI typeText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;
        [SerializeField]
        private TextMeshProUGUI statsText;

        // 툴팁 표시
        public void Show(ItemData item, Vector2 position) {
            // UI 업데이트
            itemNameText.text = item.itemName;
            itemNameText.color = item.GetRarityColor();

            rarityText.text = item.rarity.ToString();
            rarityText.color = item.GetRarityColor();

            typeText.text = item.itemType.ToString();

            descriptionText.text = item.description;

            // 스탯 정보 구성
            if (item.stats.Count > 0) {
                string statsString = "";
                foreach (var stat in item.stats) {
                    string prefix = stat.value > 0 ? "+" : "";
                    statsString += $"{prefix}{stat.value} {stat.type}\n";
                }

                statsText.gameObject.SetActive(true);
                statsText.text = statsString;
            } else {
                statsText.gameObject.SetActive(false);
            }

            // 위치 설정
            RectTransform rt = GetComponent<RectTransform>();
            rt.position = position;

            // 화면 밖으로 벗어나지 않도록 조정
            Vector2 size = rt.sizeDelta;
            Vector2 screenPos = position;

            // 오른쪽 경계 체크
            if (screenPos.x + size.x / 2 > Screen.width) {
                screenPos.x = Screen.width - size.x / 2;
            }
            // 왼쪽 경계 체크
            else if (screenPos.x - size.x / 2 < 0) {
                screenPos.x = size.x / 2;
            }

            // 아래쪽 경계 체크
            if (screenPos.y - size.y / 2 < 0) {
                screenPos.y = size.y / 2;
            }
            // 위쪽 경계 체크
            else if (screenPos.y + size.y / 2 > Screen.height) {
                screenPos.y = Screen.height - size.y / 2;
            }

            rt.position = screenPos;

            // 툴팁 표시
            gameObject.SetActive(true);
        }

        // 툴팁 숨기기
        public void Hide() {
            gameObject.SetActive(false);
        }
    }
}