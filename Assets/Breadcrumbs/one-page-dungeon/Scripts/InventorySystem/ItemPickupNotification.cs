using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Breadcrumbs.Core;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.UI {
    /// <summary>
    /// 아이템 획득 알림 시스템 - 이벤트 기반으로 개선
    /// </summary>
    public class ItemPickupNotification : MonoBehaviour {
        [SerializeField]
        private RectTransform notificationPanel;
        [SerializeField]
        private Image itemIconImage;
        [SerializeField]
        private TextMeshProUGUI itemNameText;
        [SerializeField]
        private TextMeshProUGUI quantityText;

        [Header("애니메이션 설정")]
        [SerializeField]
        private float showDuration = 2.0f;
        [SerializeField]
        private float fadeDuration = 0.5f;

        // 애니메이션 관련 변수
        private CanvasGroup canvasGroup;
        private float currentTimer = 0f;
        private Queue<(ItemData item, int quantity)> notificationQueue =
            new Queue<(ItemData item, int quantity)>();
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

        private void Start() {
            // 이벤트 리스너 등록
            RegisterEvents();
        }

        private void OnDestroy() {
            // 이벤트 리스너 해제
            UnregisterEvents();
        }

        /// <summary>
        /// 이벤트 리스너 등록
        /// </summary>
        private void RegisterEvents() {
            // 아이템 획득 이벤트 구독
            EventManager.Subscribe("Item.Pickup", OnItemPickup);
        }

        /// <summary>
        /// 이벤트 리스너 해제
        /// </summary>
        private void UnregisterEvents() {
            EventManager.Unsubscribe("Item.Pickup", OnItemPickup);
        }

        /// <summary>
        /// 아이템 획득 이벤트 처리
        /// </summary>
        private void OnItemPickup(object data) {
            if (data is ItemPickupEventData eventData) {
                ShowItemPickupNotification(eventData.Item, eventData.Quantity);
            }
        }

        /// <summary>
        /// 아이템 획득 알림 표시
        /// </summary>
        public void ShowItemPickupNotification(ItemData item, int quantity) {
            // 아이템 획득 알림 큐에 추가
            notificationQueue.Enqueue((item, quantity));

            // 현재 표시 중이 아니면 다음 알림 표시
            if (!isShowingNotification) {
                ShowNextNotification();
            }
        }

        /// <summary>
        /// 다음 알림 표시
        /// </summary>
        private void ShowNextNotification() {
            if (notificationQueue.Count > 0) {
                var notification = notificationQueue.Dequeue();
                DisplayNotification(notification.item, notification.quantity);
            }
        }

        /// <summary>
        /// 알림 표시 내부 메서드
        /// </summary>
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

        /// <summary>
        /// 페이드 인 효과
        /// </summary>
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

        /// <summary>
        /// 페이드 아웃 효과
        /// </summary>
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