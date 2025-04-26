using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GamePortfolio.Core;
using GamePortfolio.Dungeon;
using GamePortfolio.Dungeon.Generation;
using GamePortfolio.Gameplay.Character;

namespace GamePortfolio.Gameplay
{
    /// <summary>
    /// Manages the dungeon exit portal system, including activation, visualization, and escape mechanics
    /// </summary>
    public class PortalSystem : MonoBehaviour
    {
        [Header("Portal Settings")]
        [SerializeField] private float initialDelay = 300f; // 5 minutes before portal starts
        [SerializeField] private float activationDuration = 90f; // 1.5 minutes of portal active time
        [SerializeField] private float warningInterval = 60f; // Time between warnings
        [SerializeField] private float finalWarningTime = 30f; // Final warning time
        
        [Header("Portal References")]
        [SerializeField] private GameObject portalPrefab;
        [SerializeField] private GameObject portalVFXPrefab;
        [SerializeField] private GameObject portalClosingVFXPrefab;
        
        [Header("Audio")]
        [SerializeField] private string portalOpeningSoundKey = "PortalOpening";
        [SerializeField] private string portalActiveSoundKey = "PortalActive";
        [SerializeField] private string portalClosingSoundKey = "PortalClosing";
        [SerializeField] private string portalWarningKey = "PortalWarning";
        
        [Header("UI References")]
        [SerializeField] private Text timerText;
        [SerializeField] private Text warningText;
        [SerializeField] private Image portalIndicator;
        
        // Events
        public event Action OnPortalSpawning;
        public event Action<Vector3> OnPortalSpawned;
        public event Action OnPortalActivated;
        public event Action OnPortalDeactivating;
        public event Action OnPortalClosed;
        public event Action<float> OnTimeRemainingUpdated;
        
        // Runtime state
        private bool isInitialized = false;
        private PortalState currentState = PortalState.Inactive;
        private float timeRemainingToPortal = 0f;
        private float portalTimeRemaining = 0f;
        private GameObject activePortal;
        private Vector3 portalLocation;
        private bool playerEscaped = false;
        private int warningsGiven = 0;
        private DungeonManager dungeonManager;
        
        // Portal coroutines
        private Coroutine portalTimerCoroutine;
        private Coroutine activationSequenceCoroutine;
        
        private void Awake()
        {
            // Find required references
            dungeonManager = FindObjectOfType<DungeonManager>();
            
            // Set initial UI state
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
            
            if (warningText != null)
            {
                warningText.gameObject.SetActive(false);
            }
            
            if (portalIndicator != null)
            {
                portalIndicator.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Initialize the portal system with the current dungeon
        /// </summary>
        public void Initialize(float customDelay = 0f)
        {
            // Set timer based on custom delay or default
            timeRemainingToPortal = customDelay > 0 ? customDelay : initialDelay;
            portalTimeRemaining = activationDuration;
            
            // Reset state
            currentState = PortalState.Countdown;
            isInitialized = true;
            warningsGiven = 0;
            playerEscaped = false;
            
            // Start timer
            StopAllCoroutines();
            portalTimerCoroutine = StartCoroutine(PortalCountdownRoutine());
            
            Debug.Log($"Portal system initialized. Portal will open in {timeRemainingToPortal} seconds.");
        }
        
        /// <summary>
        /// Coroutine for counting down to portal activation
        /// </summary>
        private IEnumerator PortalCountdownRoutine()
        {
            // Show timer UI
            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
            }
            
            // Calculate warning times
            List<float> warningTimes = new List<float>();
            float currentWarningTime = timeRemainingToPortal - warningInterval;
            
            while (currentWarningTime > finalWarningTime)
            {
                warningTimes.Add(currentWarningTime);
                currentWarningTime -= warningInterval;
            }
            
            warningTimes.Add(finalWarningTime);
            
            // Main countdown loop
            while (timeRemainingToPortal > 0)
            {
                // Update time
                timeRemainingToPortal -= Time.deltaTime;
                OnTimeRemainingUpdated?.Invoke(timeRemainingToPortal);
                
                // Update UI
                if (timerText != null)
                {
                    int minutes = Mathf.FloorToInt(timeRemainingToPortal / 60);
                    int seconds = Mathf.FloorToInt(timeRemainingToPortal % 60);
                    timerText.text = $"Portal: {minutes:00}:{seconds:00}";
                }
                
                // Check for warnings
                if (warningsGiven < warningTimes.Count && timeRemainingToPortal <= warningTimes[warningsGiven])
                {
                    ShowPortalWarning(warningTimes[warningsGiven]);
                    warningsGiven++;
                }
                
                yield return null;
            }
            
            // Time's up - spawn portal
            SpawnPortal();
        }
        
        /// <summary>
        /// Show a warning about portal opening soon
        /// </summary>
        private void ShowPortalWarning(float timeLeft)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            string timeString = minutes > 0 ? $"{minutes} minute(s)" : $"{seconds} seconds";
            
            // Show warning text
            if (warningText != null)
            {
                warningText.gameObject.SetActive(true);
                warningText.text = $"WARNING: Portal opening in {timeString}!";
                StartCoroutine(HideWarningAfterDelay(5f));
            }
            
            // Play warning sound
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySfx(portalWarningKey);
            }
            
            Debug.Log($"Portal warning: {timeString} remaining.");
        }
        
        /// <summary>
        /// Hide warning text after delay
        /// </summary>
        private IEnumerator HideWarningAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (warningText != null)
            {
                warningText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Spawn the exit portal
        /// </summary>
        private void SpawnPortal()
        {
            // Notify that portal is spawning
            OnPortalSpawning?.Invoke();
            
            // Find a suitable location for the portal
            portalLocation = FindPortalLocation();
            
            // Change state
            currentState = PortalState.Spawning;
            
            // Start activation sequence
            activationSequenceCoroutine = StartCoroutine(PortalActivationSequence());
            
            Debug.Log($"Portal spawning at {portalLocation}.");
        }
        
        /// <summary>
        /// Portal activation sequence
        /// </summary>
        private IEnumerator PortalActivationSequence()
        {
            // Play portal opening sound
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySfx(portalOpeningSoundKey);
            }
            
            // Spawn portal VFX first
            if (portalVFXPrefab != null)
            {
                GameObject vfx = Instantiate(portalVFXPrefab, portalLocation, Quaternion.identity);
                StartCoroutine(ScaleOverTime(vfx.transform, Vector3.zero, Vector3.one, 3f));
            }
            
            // Wait for portal to "form"
            yield return new WaitForSeconds(3f);
            
            // Spawn actual portal object
            if (portalPrefab != null)
            {
                activePortal = Instantiate(portalPrefab, portalLocation, Quaternion.identity);
                
                // Set up portal trigger
                PortalTrigger trigger = activePortal.GetComponent<PortalTrigger>();
                if (trigger != null)
                {
                    trigger.Initialize(this);
                }
            }
            
            // Change state to active
            currentState = PortalState.Active;
            
            // Update UI
            if (timerText != null)
            {
                timerText.color = Color.green;
            }
            
            if (portalIndicator != null)
            {
                portalIndicator.gameObject.SetActive(true);
                portalIndicator.color = Color.green;
            }
            
            // Notify that portal is spawned
            OnPortalSpawned?.Invoke(portalLocation);
            OnPortalActivated?.Invoke();
            
            // Play portal active ambient sound
            if (AudioManager.HasInstance) {
                Debug.Log($"TODO: PlayLoopingSound {portalActiveSoundKey}");
                //AudioManager.Instance.PlayLoopingSound(portalActiveSoundKey);
            }
            
            // Start portal active timer
            portalTimeRemaining = activationDuration;
            
            // Wait for portal duration
            while (portalTimeRemaining > 0 && !playerEscaped)
            {
                // Update time
                portalTimeRemaining -= Time.deltaTime;
                
                // Update UI
                if (timerText != null)
                {
                    int minutes = Mathf.FloorToInt(portalTimeRemaining / 60);
                    int seconds = Mathf.FloorToInt(portalTimeRemaining % 60);
                    timerText.text = $"Escape: {minutes:00}:{seconds:00}";
                    
                    // Blink when time is running out
                    if (portalTimeRemaining <= 30f)
                    {
                        timerText.color = Mathf.Sin(Time.time * 5f) > 0 ? Color.red : Color.white;
                    }
                }
                
                yield return null;
            }
            
            // Portal closing - unless player escaped
            if (!playerEscaped)
            {
                ClosePortal();
            }
        }
        
        /// <summary>
        /// Close the portal
        /// </summary>
        private void ClosePortal()
        {
            // Change state
            currentState = PortalState.Closing;
            
            // Notify that portal is deactivating
            OnPortalDeactivating?.Invoke();
            
            // Stop active sound
            if (AudioManager.HasInstance)
            {
                Debug.Log($"TODO: StopLoopingSound {portalActiveSoundKey}");
                //AudioManager.Instance.StopLoopingSound(portalActiveSoundKey);
                AudioManager.Instance.PlaySfx(portalClosingSoundKey);
            }
            
            // Start closing sequence
            StartCoroutine(PortalClosingSequence());
        }
        
        /// <summary>
        /// Portal closing sequence
        /// </summary>
        private IEnumerator PortalClosingSequence()
        {
            // Spawn closing VFX
            if (portalClosingVFXPrefab != null && activePortal != null)
            {
                GameObject vfx = Instantiate(portalClosingVFXPrefab, portalLocation, Quaternion.identity);
            }
            
            // Shrink portal
            if (activePortal != null)
            {
                StartCoroutine(ScaleOverTime(activePortal.transform, activePortal.transform.localScale, Vector3.zero, 2f));
            }
            
            // Wait for closing animation
            yield return new WaitForSeconds(2f);
            
            // Destroy portal object
            if (activePortal != null)
            {
                Destroy(activePortal);
                activePortal = null;
            }
            
            // Change state
            currentState = PortalState.Inactive;
            
            // Update UI
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
            
            if (portalIndicator != null)
            {
                portalIndicator.gameObject.SetActive(false);
            }
            
            // Notify that portal is closed
            OnPortalClosed?.Invoke();
            
            Debug.Log("Portal has closed.");
        }
        
        /// <summary>
        /// Handle player entering the portal
        /// </summary>
        public void PlayerEnteredPortal(GameObject player)
        {
            // Only handle if portal is active
            if (currentState != PortalState.Active)
                return;
                
            // Set escaped flag
            playerEscaped = true;
            
            // Trigger escape sequence
            StartCoroutine(PlayerEscapeSequence(player));
            
            Debug.Log("Player has entered the portal!");
        }
        
        /// <summary>
        /// Sequence for player escaping through portal
        /// </summary>
        private IEnumerator PlayerEscapeSequence(GameObject player)
        {
            // Disable player controls
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            // Teleport player to center of portal
            player.transform.position = portalLocation;
            
            // Start shrinking player
            StartCoroutine(ScaleOverTime(player.transform, player.transform.localScale, Vector3.zero, 1.5f));
            
            // Wait for animation
            yield return new WaitForSeconds(2f);
            
            // Notify game manager of successful escape
            if (GameManager.HasInstance) {
                Debug.Log("TODO: GameManager.Instance.PlayerEscaped");
                //GameManager.Instance.PlayerEscaped();
            }
            
            // Close the portal
            ClosePortal();
        }
        
        /// <summary>
        /// Find a suitable location for the portal
        /// </summary>
        private Vector3 FindPortalLocation()
        {
            // Check if dungeon manager is available
            if (dungeonManager != null && dungeonManager.CurrentDungeon != null)
            {
                // Try to find exit room
                Room exitRoom = dungeonManager.CurrentDungeon.GetRoomByType(RoomType.Exit);
                
                if (exitRoom != null)
                {
                    // Use center of exit room
                    Vector2Int center = exitRoom.GetCenter();
                    return new Vector3(center.x, 0.1f, center.y);
                }
                
                // Fallback: Find the room farthest from entrance
                Room entranceRoom = dungeonManager.CurrentDungeon.GetRoomByType(RoomType.Entrance);
                if (entranceRoom != null)
                {
                    Vector2Int entranceCenter = entranceRoom.GetCenter();
                    
                    Room farthestRoom = null;
                    float maxDistance = 0;
                    
                    foreach (Room room in dungeonManager.CurrentDungeon.Rooms)
                    {
                        if (room.Type == RoomType.Entrance)
                            continue;
                            
                        Vector2Int roomCenter = room.GetCenter();
                        float distance = Vector2Int.Distance(entranceCenter, roomCenter);
                        
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            farthestRoom = room;
                        }
                    }
                    
                    if (farthestRoom != null)
                    {
                        Vector2Int center = farthestRoom.GetCenter();
                        return new Vector3(center.x, 0.1f, center.y);
                    }
                }
            }
            
            // Last resort: random position
            return new Vector3(UnityEngine.Random.Range(-20, 20), 0.1f, UnityEngine.Random.Range(-20, 20));
        }
        
        /// <summary>
        /// Utility method to scale an object over time
        /// </summary>
        private IEnumerator ScaleOverTime(Transform target, Vector3 startScale, Vector3 endScale, float duration)
        {
            float time = 0;
            
            while (time < duration && target != null)
            {
                time += Time.deltaTime;
                float t = time / duration;
                
                if (target != null)
                {
                    target.localScale = Vector3.Lerp(startScale, endScale, t);
                }
                
                yield return null;
            }
            
            if (target != null)
            {
                target.localScale = endScale;
            }
        }
        
        /// <summary>
        /// Force the portal to open immediately
        /// </summary>
        public void ForcePortalOpen()
        {
            if (portalTimerCoroutine != null)
            {
                StopCoroutine(portalTimerCoroutine);
            }
            
            SpawnPortal();
        }
        
        /// <summary>
        /// Get current portal state
        /// </summary>
        public PortalState GetPortalState()
        {
            return currentState;
        }
        
        /// <summary>
        /// Get time until portal opens
        /// </summary>
        public float GetTimeUntilPortal()
        {
            return timeRemainingToPortal;
        }
        
        /// <summary>
        /// Get time until portal closes
        /// </summary>
        public float GetTimeUntilPortalCloses()
        {
            return portalTimeRemaining;
        }
        
        /// <summary>
        /// Get portal location
        /// </summary>
        public Vector3 GetPortalLocation()
        {
            return portalLocation;
        }
        
        /// <summary>
        /// Check if the portal is currently active
        /// </summary>
        public bool IsPortalActive()
        {
            return currentState == PortalState.Active;
        }
    }
    
    /// <summary>
    /// States for the portal system
    /// </summary>
    public enum PortalState
    {
        Inactive,   // No portal
        Countdown,  // Counting down to portal spawn
        Spawning,   // Portal is spawning
        Active,     // Portal is active and usable
        Closing     // Portal is closing
    }
}