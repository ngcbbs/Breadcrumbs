using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Breadcrumbs.ItemSystem {
    public class ItemPickupNotification : MonoBehaviour {
        [SerializeField]
        private RectTransform notificationPanel;
        [SerializeField]
        private Image itemIconImage;
        [SerializeField]
        private TextMeshProUGUI itemNameText;
        [SerializeField]
        private TextMeshProUGUI quantityText;

        [Header("Animation Settings")]
        [SerializeField]
        private float showDuration = 2.0f;
        [SerializeField]
        private float fadeDuration = 0.5f;

        // 애니메이션 관련 변수
        private CanvasGroup canvasGroup;
        private float currentTimer = 0f;
        private Queue<(ItemData item, int quantity)> notificationQueue = new Queue<(ItemData item, int quantity)>();
        private bool isShowingNotification = false;

        private void Awake() {
            canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) {
                canvasGroup = notificationPanel.gameObject.AddComponent<CanvasGroup>();
            }

            // 기본 상태는 숨김
            canvasGroup.alpha = 0f;
            notificationPanel.gameObject.SetActive(false);
        }

        // PlayerInventory에서 호출할 아이템 획득 알림 메서드
        public void ShowItemPickupNotification(ItemData item, int quantity) {
            // 아이템 획득 알림 큐에 추가
            notificationQueue.Enqueue((item, quantity));

            // 현재 표시 중이 아니면 다음 알림 표시
            if (!isShowingNotification) {
                ShowNextNotification();
            }
        }

        // 다음 알림 표시
        private void ShowNextNotification() {
            if (notificationQueue.Count > 0) {
                var notification = notificationQueue.Dequeue();
                DisplayNotification(notification.item, notification.quantity);
            }
        }

        // 알림 표시 내부 메서드
        private void DisplayNotification(ItemData item, int quantity) {
            // 알림 UI 설정
            itemIconImage.sprite = item.icon;
            itemIconImage.color = Color.white;

            itemNameText.text = item.itemName;
            itemNameText.color = item.GetRarityColor();

            quantityText.text = quantity > 1 ? $"x{quantity}" : "";

            // 알림 표시
            notificationPanel.gameObject.SetActive(true);
            isShowingNotification = true;
            currentTimer = showDuration;

            // 페이드 인 효과
            StartCoroutine(FadeIn());
        }

        private void Update() {
            if (isShowingNotification) {
                currentTimer -= Time.deltaTime;

                if (currentTimer <= 0f) {
                    // 표시 시간 종료, 페이드 아웃
                    StartCoroutine(FadeOut());
                }
            }
        }

        // 페이드 인 효과
        private IEnumerator FadeIn() {
            canvasGroup.alpha = 0f;

            float timer = 0f;
            while (timer < fadeDuration) {
                timer += Time.deltaTime;
                canvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        // 페이드 아웃 효과
        private IEnumerator FadeOut() {
            canvasGroup.alpha = 1f;

            float timer = 0f;
            while (timer < fadeDuration) {
                timer += Time.deltaTime;
                canvasGroup.alpha = 1f - (timer / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            notificationPanel.gameObject.SetActive(false);
            isShowingNotification = false;

            // 다음 알림 표시
            ShowNextNotification();
        }
    }
}