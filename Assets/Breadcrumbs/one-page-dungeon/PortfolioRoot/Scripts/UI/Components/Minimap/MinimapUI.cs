using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GamePortfolio.Core;
using GamePortfolio.Dungeon.Generation;
using GamePortfolio.UI.HUD;

namespace GamePortfolio.UI.Components
{
    /// <summary>
    /// Enhanced minimap UI component with interactive features
    /// </summary>
    public class MinimapUI : MonoBehaviour, IDragHandler, IPointerClickHandler
    {
        [Header("Minimap UI Components")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private RectTransform minimapContainer;
        [SerializeField] private RectTransform minimapIconsContainer;
        [SerializeField] private RectTransform playerIconRect;
        [SerializeField] private Image playerIcon;
        [SerializeField] private Button zoomInButton;
        [SerializeField] private Button zoomOutButton;
        [SerializeField] private Button toggleModeButton;
        [SerializeField] private Button toggleRotationButton;
        [SerializeField] private Button fullMapButton;
        [SerializeField] private Button addMarkerButton;
        [SerializeField] private Button clearMarkersButton;
        [SerializeField] private Slider transparencySlider;
        [SerializeField] private CanvasGroup minimapCanvasGroup;
        [SerializeField] private GameObject markerPanel;
        [SerializeField] private GameObject markerButtonPrefab;
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private Text tooltipText;
        
        [Header("Minimap Settings")]
        [SerializeField] private float defaultZoomLevel = 1.0f;
        [SerializeField] private float maxZoomLevel = 3.0f;
        [SerializeField] private float minZoomLevel = 0.5f;
        [SerializeField] private float zoomStep = 0.25f;
        [SerializeField] private bool rotateWithPlayer = false;
        [SerializeField] private bool centerOnPlayer = true;
        [SerializeField] private float updateInterval = 0.1f;
        [SerializeField] private Vector2 minimapSize = new Vector2(256, 256);
        [SerializeField] private KeyCode toggleMinimapKey = KeyCode.M;
        [SerializeField] private KeyCode fullMapKey = KeyCode.Tab;
        
        [Header("Marker Icons")]
        [SerializeField] private Sprite[] markerIcons;
        [SerializeField] private Color[] markerColors;
        [SerializeField] private string[] markerTooltips;
        
        // References
        private Transform playerTransform;
        private GamePortfolio.UI.HUD.MinimapSystem minimapSystem;
        private RectTransform rectTransform;
        private GameObject activeMarkerPanel;
        
        // State
        private float currentZoomLevel;
        private bool isDragging = false;
        private Vector2 dragOffset = Vector2.zero;
        private bool showFullMap = false;
        private bool minimapVisible = true;
        private List<GameObject> customMarkers = new List<GameObject>();
        private Dictionary<GameObject, string> markerTooltipMap = new Dictionary<GameObject, string>();
        private MarkerType selectedMarkerType = MarkerType.Point;
        private Coroutine minimapUpdateCoroutine;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            
            // Find player if not assigned
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
            
            // Find or create minimap system
            minimapSystem = FindObjectOfType<GamePortfolio.UI.HUD.MinimapSystem>();
            if (minimapSystem == null)
            {
                Debug.LogError("MinimapSystem not found. MinimapUI requires a MinimapSystem component.");
            }
            
            // Set initial zoom level
            currentZoomLevel = defaultZoomLevel;
            
            // Setup UI buttons
            SetupUIButtons();
            
            // Hide marker panel initially
            if (markerPanel != null)
            {
                markerPanel.SetActive(false);
            }
            
            // Hide tooltip initially
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
        
        private void Start()
        {
            // Start minimap update coroutine
            minimapUpdateCoroutine = StartCoroutine(UpdateMinimapRoutine());
        }
        
        private void OnDestroy()
        {
            if (minimapUpdateCoroutine != null)
            {
                StopCoroutine(minimapUpdateCoroutine);
            }
        }
        
        private void Update()
        {
            // Toggle minimap with key
            if (Input.GetKeyDown(toggleMinimapKey))
            {
                ToggleMinimapVisibility();
            }
            
            // Toggle full map with key
            if (Input.GetKeyDown(fullMapKey))
            {
                ToggleFullMap();
            }
            
            // Update player icon rotation if needed
            if (playerTransform != null && playerIconRect != null && !rotateWithPlayer)
            {
                playerIconRect.rotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.y);
            }
        }
        
        /// <summary>
        /// Setup all UI buttons and controls
        /// </summary>
        private void SetupUIButtons()
        {
            if (zoomInButton != null)
            {
                zoomInButton.onClick.AddListener(ZoomIn);
            }
            
            if (zoomOutButton != null)
            {
                zoomOutButton.onClick.AddListener(ZoomOut);
            }
            
            if (toggleModeButton != null)
            {
                toggleModeButton.onClick.AddListener(ToggleMinimapMode);
            }
            
            if (toggleRotationButton != null)
            {
                toggleRotationButton.onClick.AddListener(ToggleRotation);
            }
            
            if (fullMapButton != null)
            {
                fullMapButton.onClick.AddListener(ToggleFullMap);
            }
            
            if (addMarkerButton != null)
            {
                addMarkerButton.onClick.AddListener(ToggleMarkerPanel);
            }
            
            if (clearMarkersButton != null)
            {
                clearMarkersButton.onClick.AddListener(ClearAllMarkers);
            }
            
            if (transparencySlider != null)
            {
                transparencySlider.onValueChanged.AddListener(SetMinimapTransparency);
                
                // Set initial transparency
                if (minimapCanvasGroup != null)
                {
                    minimapCanvasGroup.alpha = transparencySlider.value;
                }
            }
        }
        
        /// <summary>
        /// Coroutine to update minimap at fixed intervals
        /// </summary>
        private IEnumerator UpdateMinimapRoutine()
        {
            while (true)
            {
                // Update minimap when visible
                if (minimapVisible)
                {
                    UpdateMinimapPosition();
                }
                
                yield return new WaitForSeconds(updateInterval);
            }
        }
        
        /// <summary>
        /// Update minimap position based on player position
        /// </summary>
        private void UpdateMinimapPosition()
        {
            if (playerTransform != null && minimapImage != null && centerOnPlayer)
            {
                // Use the minimap system's conversion methods
                if (minimapSystem != null)
                {
                    // Center minimap on player if needed
                    minimapSystem.CenterOnPosition(playerTransform.position);
                }
                
                // Rotate minimap container if rotating with player
                if (rotateWithPlayer && minimapContainer != null)
                {
                    minimapContainer.rotation = Quaternion.Euler(0, 0, playerTransform.eulerAngles.y);
                    
                    // Keep player icon facing up
                    if (playerIconRect != null)
                    {
                        playerIconRect.rotation = Quaternion.identity;
                    }
                }
                else if (minimapContainer != null)
                {
                    // Reset rotation when not rotating with player
                    minimapContainer.rotation = Quaternion.identity;
                }
            }
        }
        
        /// <summary>
        /// Handle drag operation on minimap
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (centerOnPlayer || !minimapVisible)
                return;
                
            // Calculate drag delta in minimap space
            Vector2 delta = eventData.delta;
            
            // Update drag offset
            dragOffset -= delta;
            
            // Apply offset to minimap
            if (minimapSystem != null)
            {
                // TODO: Implement offset in minimap system
            }
        }
        
        /// <summary>
        /// Handle click on minimap
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!minimapVisible)
                return;
                
            // Check if adding marker
            if (markerPanel != null && markerPanel.activeSelf)
            {
                // Convert screen position to world position
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    minimapImage.rectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
                
                // Add marker at position
                AddMarkerAtPosition(localPoint, selectedMarkerType);
                
                // Hide marker panel
                markerPanel.SetActive(false);
                return;
            }
            
            // Handle right-click to create ping
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Convert screen position to world position
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    minimapImage.rectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
                
                // Create temporary ping at this position
                CreatePing(localPoint);
            }
        }
        
        /// <summary>
        /// Toggle minimap visibility
        /// </summary>
        public void ToggleMinimapVisibility()
        {
            minimapVisible = !minimapVisible;
            gameObject.SetActive(minimapVisible);
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Toggle");
            }
        }
        
        /// <summary>
        /// Zoom in on minimap
        /// </summary>
        public void ZoomIn()
        {
            if (minimapSystem == null)
                return;
                
            currentZoomLevel = Mathf.Clamp(currentZoomLevel + zoomStep, minZoomLevel, maxZoomLevel);
            minimapSystem.SetZoomLevel(currentZoomLevel);
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }
        
        /// <summary>
        /// Zoom out on minimap
        /// </summary>
        public void ZoomOut()
        {
            if (minimapSystem == null)
                return;
                
            currentZoomLevel = Mathf.Clamp(currentZoomLevel - zoomStep, minZoomLevel, maxZoomLevel);
            minimapSystem.SetZoomLevel(currentZoomLevel);
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }
        
        /// <summary>
        /// Toggle between player-centered and free-look modes
        /// </summary>
        public void ToggleMinimapMode()
        {
            centerOnPlayer = !centerOnPlayer;
            dragOffset = Vector2.zero;
            
            // Update minimap system
            if (minimapSystem != null)
            {
                minimapSystem.ToggleMinimapMode();
            }
            
            // Update button appearance based on mode
            if (toggleModeButton != null)
            {
                Image buttonImage = toggleModeButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = centerOnPlayer ? Color.green : Color.white;
                }
            }
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Toggle");
            }
        }
        
        /// <summary>
        /// Toggle rotation mode
        /// </summary>
        public void ToggleRotation()
        {
            rotateWithPlayer = !rotateWithPlayer;
            
            // Update minimap system
            if (minimapSystem != null)
            {
                minimapSystem.ToggleRotation();
            }
            
            // Update button appearance based on mode
            if (toggleRotationButton != null)
            {
                Image buttonImage = toggleRotationButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = rotateWithPlayer ? Color.green : Color.white;
                }
            }
            
            // Reset container rotation when disabling rotation
            if (!rotateWithPlayer && minimapContainer != null)
            {
                minimapContainer.rotation = Quaternion.identity;
            }
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Toggle");
            }
        }
        
        /// <summary>
        /// Toggle full map view
        /// </summary>
        public void ToggleFullMap()
        {
            if (minimapSystem == null)
                return;
                
            showFullMap = !showFullMap;
            minimapSystem.ToggleFullMap();
            
            // Update button appearance
            if (fullMapButton != null)
            {
                Image buttonImage = fullMapButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = showFullMap ? Color.green : Color.white;
                }
            }
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Map");
            }
        }
        
        /// <summary>
        /// Set minimap transparency
        /// </summary>
        public void SetMinimapTransparency(float transparency)
        {
            if (minimapCanvasGroup != null)
            {
                minimapCanvasGroup.alpha = transparency;
            }
        }
        
        /// <summary>
        /// Toggle marker selection panel
        /// </summary>
        public void ToggleMarkerPanel()
        {
            if (markerPanel == null)
                return;
                
            bool isActive = !markerPanel.activeSelf;
            markerPanel.SetActive(isActive);
            
            // Generate marker buttons if showing panel
            if (isActive && markerPanel.transform.childCount == 0)
            {
                GenerateMarkerButtons();
            }
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound(isActive ? "Open" : "Close");
            }
        }
        
        /// <summary>
        /// Generate marker type buttons
        /// </summary>
        private void GenerateMarkerButtons()
        {
            if (markerButtonPrefab == null || markerPanel == null)
                return;
                
            // Clear existing buttons
            foreach (Transform child in markerPanel.transform)
            {
                Destroy(child.gameObject);
            }
            
            // Create a button for each marker type
            for (int i = 0; i < System.Enum.GetValues(typeof(MarkerType)).Length; i++)
            {
                MarkerType markerType = (MarkerType)i;
                
                GameObject buttonObj = Instantiate(markerButtonPrefab, markerPanel.transform);
                Button button = buttonObj.GetComponent<Button>();
                
                // Set icon if available
                if (i < markerIcons.Length)
                {
                    Image buttonImage = buttonObj.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.sprite = markerIcons[i];
                    }
                }
                
                // Set tooltip
                string tooltip = i < markerTooltips.Length ? markerTooltips[i] : markerType.ToString();
                
                // Add button click event
                int index = i; // Capture for lambda
                button.onClick.AddListener(() => {
                    selectedMarkerType = (MarkerType)index;
                    markerPanel.SetActive(false);
                });
                
                // Add hover events for tooltip
                EventTrigger eventTrigger = buttonObj.AddComponent<EventTrigger>();
                
                // Pointer enter event
                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((data) => { ShowTooltip(tooltip, ((PointerEventData)data).position); });
                eventTrigger.triggers.Add(enterEntry);
                
                // Pointer exit event
                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) => { HideTooltip(); });
                eventTrigger.triggers.Add(exitEntry);
            }
        }
        
        /// <summary>
        /// Show tooltip at position
        /// </summary>
        private void ShowTooltip(string text, Vector2 position)
        {
            if (tooltipPanel == null || tooltipText == null)
                return;
                
            tooltipText.text = text;
            tooltipPanel.SetActive(true);
            
            // Position tooltip
            RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            if (tooltipRect != null)
            {
                tooltipRect.position = position + new Vector2(10, 10); // Offset slightly
            }
        }
        
        /// <summary>
        /// Hide tooltip
        /// </summary>
        private void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Add a marker at the specified minimap position
        /// </summary>
        private void AddMarkerAtPosition(Vector2 minimapPosition, MarkerType markerType)
        {
            if (minimapSystem == null || minimapIconsContainer == null)
                return;
                
            // Convert minimap position to world position (simplified)
            Vector3 worldPos = new Vector3(minimapPosition.x, 0, minimapPosition.y);
            
            // Create marker in minimap system
            minimapSystem.AddCustomMarker(worldPos, markerType);
            
            // Create visual marker
            GameObject markerObj = new GameObject("CustomMarker");
            markerObj.transform.SetParent(minimapIconsContainer, false);
            
            RectTransform markerRect = markerObj.AddComponent<RectTransform>();
            markerRect.anchoredPosition = minimapPosition;
            markerRect.sizeDelta = new Vector2(24, 24);
            
            Image markerImage = markerObj.AddComponent<Image>();
            int markerIndex = (int)markerType;
            if (markerIndex < markerIcons.Length)
            {
                markerImage.sprite = markerIcons[markerIndex];
            }
            
            if (markerIndex < markerColors.Length)
            {
                markerImage.color = markerColors[markerIndex];
            }
            
            // Add to list for tracking
            customMarkers.Add(markerObj);
            
            // Add tooltip
            string tooltip = markerIndex < markerTooltips.Length ? markerTooltips[markerIndex] : markerType.ToString();
            markerTooltipMap[markerObj] = tooltip;
            
            // Add event trigger for tooltip
            EventTrigger eventTrigger = markerObj.AddComponent<EventTrigger>();
            
            // Pointer enter event
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { 
                ShowTooltip(markerTooltipMap[markerObj], ((PointerEventData)data).position); 
            });
            eventTrigger.triggers.Add(enterEntry);
            
            // Pointer exit event
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { HideTooltip(); });
            eventTrigger.triggers.Add(exitEntry);
            
            // Add click event to remove
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => { 
                if (((PointerEventData)data).button == PointerEventData.InputButton.Right)
                {
                    RemoveMarker(markerObj, worldPos);
                }
            });
            eventTrigger.triggers.Add(clickEntry);
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Marker");
            }
        }
        
        /// <summary>
        /// Remove a specific marker
        /// </summary>
        private void RemoveMarker(GameObject markerObj, Vector3 worldPos)
        {
            if (minimapSystem != null)
            {
                minimapSystem.RemoveCustomMarker(worldPos);
            }
            
            // Remove from dictionary and list
            markerTooltipMap.Remove(markerObj);
            customMarkers.Remove(markerObj);
            
            // Destroy the marker object
            Destroy(markerObj);
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Delete");
            }
        }
        
        /// <summary>
        /// Clear all custom markers
        /// </summary>
        public void ClearAllMarkers()
        {
            foreach (GameObject marker in customMarkers)
            {
                Destroy(marker);
            }
            
            customMarkers.Clear();
            markerTooltipMap.Clear();
            
            // Clear markers in minimap system
            // This would require additional functionality in the minimap system
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Clear");
            }
        }
        
        /// <summary>
        /// Create a temporary ping that fades out
        /// </summary>
        private void CreatePing(Vector2 minimapPosition)
        {
            if (minimapIconsContainer == null)
                return;
                
            // Create ping object
            GameObject pingObj = new GameObject("Ping");
            pingObj.transform.SetParent(minimapIconsContainer, false);
            
            RectTransform pingRect = pingObj.AddComponent<RectTransform>();
            pingRect.anchoredPosition = minimapPosition;
            pingRect.sizeDelta = new Vector2(32, 32);
            
            Image pingImage = pingObj.AddComponent<Image>();
            pingImage.sprite = markerIcons.Length > 0 ? markerIcons[0] : null;
            pingImage.color = Color.yellow;
            
            // Add animation component to handle fade out
            PingAnimation pingAnim = pingObj.AddComponent<PingAnimation>();
            pingAnim.Initialize(2.0f); // Duration in seconds
            
            // Play ping sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Ping");
            }
        }
    }
    
    /// <summary>
    /// Component to handle ping animation and auto-destruction
    /// </summary>
    public class PingAnimation : MonoBehaviour
    {
        private float duration = 2.0f;
        private float startTime;
        private Image image;
        private RectTransform rectTransform;
        
        public void Initialize(float pingDuration)
        {
            duration = pingDuration;
            startTime = Time.time;
            image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
        }
        
        private void Update()
        {
            float elapsed = Time.time - startTime;
            float t = elapsed / duration;
            
            if (t >= 1.0f)
            {
                Destroy(gameObject);
                return;
            }
            
            // Fade out
            if (image != null)
            {
                Color color = image.color;
                color.a = Mathf.Lerp(1.0f, 0.0f, t);
                image.color = color;
            }
            
            // Expand
            if (rectTransform != null)
            {
                float scale = Mathf.Lerp(1.0f, 2.0f, t);
                rectTransform.localScale = new Vector3(scale, scale, 1.0f);
            }
        }
    }
}