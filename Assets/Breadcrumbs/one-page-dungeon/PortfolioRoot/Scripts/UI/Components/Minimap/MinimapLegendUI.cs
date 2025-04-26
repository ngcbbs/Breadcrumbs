using System.Collections;
using System.Collections.Generic;
using GamePortfolio.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GamePortfolio.UI.Components
{
    /// <summary>
    /// Provides a legend for the minimap showing what different icons and colors mean
    /// </summary>
    public class MinimapLegendUI : MonoBehaviour
    {
        [System.Serializable]
        public class LegendItem
        {
            public string label;
            public Sprite icon;
            public Color iconColor = Color.white;
        }
        
        [Header("Legend UI")]
        [SerializeField] private GameObject legendItemPrefab;
        [SerializeField] private Transform legendContainer;
        [SerializeField] private Button toggleButton;
        [SerializeField] private CanvasGroup legendCanvasGroup;
        [SerializeField] private float fadeDuration = 0.25f;
        
        [Header("Legend Items")]
        [SerializeField] private LegendItem[] legendItems;
        
        private bool isShowing = false;
        private Coroutine fadeCoroutine;
        
        private void Awake()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleLegend);
            }
            
            // Initially hide the legend
            if (legendCanvasGroup != null)
            {
                legendCanvasGroup.alpha = 0;
                legendCanvasGroup.blocksRaycasts = false;
                legendCanvasGroup.interactable = false;
            }
        }
        
        private void Start()
        {
            GenerateLegendItems();
        }
        
        /// <summary>
        /// Generate legend items from configuration
        /// </summary>
        private void GenerateLegendItems()
        {
            if (legendItemPrefab == null || legendContainer == null)
                return;
                
            // Clear existing items
            foreach (Transform child in legendContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create items
            foreach (LegendItem item in legendItems)
            {
                GameObject legendItemObj = Instantiate(legendItemPrefab, legendContainer);
                
                // Set icon
                Image iconImage = legendItemObj.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = item.icon;
                    iconImage.color = item.iconColor;
                }
                
                // Set label
                Text labelText = legendItemObj.transform.Find("Label")?.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.text = item.label;
                }
            }
        }
        
        /// <summary>
        /// Toggle legend visibility
        /// </summary>
        public void ToggleLegend()
        {
            isShowing = !isShowing;
            
            // Stop any existing fade
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            // Start new fade
            fadeCoroutine = StartCoroutine(FadeLegend(isShowing ? 1 : 0));
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound(isShowing ? "Open" : "Close");
            }
        }
        
        /// <summary>
        /// Coroutine to fade legend in/out
        /// </summary>
        private IEnumerator FadeLegend(float targetAlpha)
        {
            if (legendCanvasGroup == null)
                yield break;
                
            // Enable interaction if showing
            if (targetAlpha > 0)
            {
                legendCanvasGroup.blocksRaycasts = true;
                legendCanvasGroup.interactable = true;
            }
            
            float startAlpha = legendCanvasGroup.alpha;
            float time = 0;
            
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = time / fadeDuration;
                legendCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            
            legendCanvasGroup.alpha = targetAlpha;
            
            // Disable interaction if hidden
            if (targetAlpha == 0)
            {
                legendCanvasGroup.blocksRaycasts = false;
                legendCanvasGroup.interactable = false;
            }
            
            fadeCoroutine = null;
        }
        
        /// <summary>
        /// Add a new item to the legend at runtime
        /// </summary>
        public void AddLegendItem(string label, Sprite icon, Color color)
        {
            if (legendItemPrefab == null || legendContainer == null)
                return;
                
            GameObject legendItemObj = Instantiate(legendItemPrefab, legendContainer);
            
            // Set icon
            Image iconImage = legendItemObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.color = color;
            }
            
            // Set label
            Text labelText = legendItemObj.transform.Find("Label")?.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.text = label;
            }
        }
        
        /// <summary>
        /// Clear all legend items
        /// </summary>
        public void ClearLegend()
        {
            if (legendContainer == null)
                return;
                
            foreach (Transform child in legendContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}