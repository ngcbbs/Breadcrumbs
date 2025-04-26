using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Character;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Events {
    /// <summary>
    /// Manages special events that can occur during gameplay
    /// </summary>
    public class SpecialEventSystem : Singleton<SpecialEventSystem> {
        [Header("Event Settings")]
        [SerializeField]
        private float minTimeBetweenRandomEvents = 180f; // 3 minutes
        [SerializeField]
        private float maxTimeBetweenRandomEvents = 300f; // 5 minutes
        [SerializeField]
        private bool enableRandomEvents = true;
        [SerializeField]
        private List<SpecialEventData> availableEvents = new List<SpecialEventData>();

        [Header("Event Trigger Settings")]
        [SerializeField]
        private float playerHealthThresholdPercent = 0.3f; // 30% health
        [SerializeField]
        private float lowHealthEventCooldown = 240f; // 4 minutes
        [SerializeField]
        private bool enableLowHealthEvents = true;

        // Event state tracking
        private Dictionary<SpecialEventType, float> lastEventTimes = new Dictionary<SpecialEventType, float>();
        private SpecialEventData currentActiveEvent = null;
        private List<SpecialEventTrigger> registeredTriggers = new List<SpecialEventTrigger>();
        private float nextRandomEventTime;
        private float lastLowHealthEventTime;
        private bool isEventActive = false;

        // Event delegates
        public delegate void EventStartedHandler(SpecialEventType eventType);
        public delegate void EventEndedHandler(SpecialEventType eventType);

        public event EventStartedHandler OnEventStarted;
        public event EventEndedHandler OnEventEnded;

        protected override void Awake() {
            base.Awake();

            // Initialize event times
            foreach (SpecialEventType eventType in Enum.GetValues(typeof(SpecialEventType))) {
                lastEventTimes[eventType] = -999f;
            }

            // Schedule first random event
            ScheduleNextRandomEvent();

            // Initialize low health event timer
            lastLowHealthEventTime = -lowHealthEventCooldown;
        }

        private void Update() {
            // Check for random event trigger
            if (enableRandomEvents && !isEventActive && Time.time >= nextRandomEventTime) {
                TriggerRandomEvent();
            }

            // Check for low health event trigger
            if (enableLowHealthEvents && !isEventActive &&
                Time.time >= lastLowHealthEventTime + lowHealthEventCooldown) {
                CheckLowHealthTrigger();
            }
        }

        /// <summary>
        /// Schedule the next random special event
        /// </summary>
        private void ScheduleNextRandomEvent() {
            float delay = UnityEngine.Random.Range(minTimeBetweenRandomEvents, maxTimeBetweenRandomEvents);
            nextRandomEventTime = Time.time + delay;
        }

        /// <summary>
        /// Check if player is at low health to trigger an event
        /// </summary>
        private void CheckLowHealthTrigger() {
            // Find player health component
            HealthSystem playerHealth = FindObjectOfType<PlayerController>()?.GetComponent<HealthSystem>();

            if (playerHealth != null) {
                // Check if player is at low health
                if (playerHealth.HealthPercentage <= playerHealthThresholdPercent) {
                    // Trigger a low health event
                    TriggerEventOfType(SpecialEventType.HealingShrine);
                    lastLowHealthEventTime = Time.time;
                }
            }
        }

        /// <summary>
        /// Trigger a random special event
        /// </summary>
        public void TriggerRandomEvent() {
            if (isEventActive || availableEvents.Count == 0)
                return;

            // Filter events by cooldown
            List<SpecialEventData> availableNow = new List<SpecialEventData>();

            foreach (var eventData in availableEvents) {
                if (Time.time >= lastEventTimes[eventData.eventType] + eventData.cooldownTime) {
                    availableNow.Add(eventData);
                }
            }

            if (availableNow.Count == 0) {
                // No events available yet, reschedule
                ScheduleNextRandomEvent();
                return;
            }

            // Select random event
            SpecialEventData selectedEvent = availableNow[UnityEngine.Random.Range(0, availableNow.Count)];

            // Start event
            StartCoroutine(RunEvent(selectedEvent));

            // Schedule next random event
            ScheduleNextRandomEvent();
        }

        /// <summary>
        /// Trigger a specific type of event
        /// </summary>
        public void TriggerEventOfType(SpecialEventType eventType) {
            if (isEventActive)
                return;

            // Find event data for type
            SpecialEventData eventData = null;

            foreach (var data in availableEvents) {
                if (data.eventType == eventType) {
                    eventData = data;
                    break;
                }
            }

            if (eventData == null) {
                Debug.LogWarning($"Event type {eventType} not found in available events");
                return;
            }

            // Check cooldown
            if (Time.time < lastEventTimes[eventType] + eventData.cooldownTime) {
                return;
            }

            // Start event
            StartCoroutine(RunEvent(eventData));
        }

        /// <summary>
        /// Run a special event
        /// </summary>
        private IEnumerator RunEvent(SpecialEventData eventData) {
            isEventActive = true;
            currentActiveEvent = eventData;

            // Update last event time
            lastEventTimes[eventData.eventType] = Time.time;

            // Notify listeners
            OnEventStarted?.Invoke(eventData.eventType);

            // Spawn event prefab if provided
            GameObject eventInstance = null;
            if (eventData.eventPrefab != null) {
                // Find a suitable spawn location
                Vector3 spawnPosition = FindEventSpawnLocation();

                eventInstance = Instantiate(eventData.eventPrefab, spawnPosition, Quaternion.identity);

                // Initialize event behavior
                SpecialEventBehavior eventBehavior = eventInstance.GetComponent<SpecialEventBehavior>();
                if (eventBehavior != null) {
                    eventBehavior.Initialize(eventData);
                }
            }

            // Play announcement sound
            if (eventData.announcementSound != null) {
                AudioSource.PlayClipAtPoint(eventData.announcementSound, Camera.main.transform.position, 1f);
            }

            // Show announcement message
            // (This would depend on your UI system, simplified here)
            Debug.Log($"Special Event Started: {eventData.displayName}");

            // Wait for event duration
            yield return new WaitForSeconds(eventData.duration);

            // Clean up event
            if (eventInstance != null) {
                Destroy(eventInstance);
            }

            // Notify listeners
            OnEventEnded?.Invoke(eventData.eventType);

            // Reset state
            isEventActive = false;
            currentActiveEvent = null;
        }

        /// <summary>
        /// Find a suitable location to spawn an event
        /// </summary>
        private Vector3 FindEventSpawnLocation() {
            // Try to find a spawn trigger first
            if (registeredTriggers.Count > 0) {
                // Filter by compatible event types
                List<SpecialEventTrigger> compatibleTriggers = new List<SpecialEventTrigger>();

                foreach (var trigger in registeredTriggers) {
                    if (trigger.IsCompatibleWithEventType(currentActiveEvent.eventType)) {
                        compatibleTriggers.Add(trigger);
                    }
                }

                if (compatibleTriggers.Count > 0) {
                    // Select random compatible trigger
                    SpecialEventTrigger selectedTrigger =
                        compatibleTriggers[UnityEngine.Random.Range(0, compatibleTriggers.Count)];
                    return selectedTrigger.transform.position;
                }
            }

            // Fallback: find a location near the player
            Transform playerTransform = FindObjectOfType<PlayerController>()?.transform;

            if (playerTransform != null) {
                // Find position at some distance from the player
                Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 5f;
                randomDirection.y = 0;

                Vector3 spawnPosition = playerTransform.position + randomDirection;

                // Make sure it's on the navmesh
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas)) {
                    return hit.position;
                }

                return playerTransform.position;
            }

            // Ultimate fallback
            return Vector3.zero;
        }

        /// <summary>
        /// Register a trigger spot for special events
        /// </summary>
        public void RegisterEventTrigger(SpecialEventTrigger trigger) {
            if (!registeredTriggers.Contains(trigger)) {
                registeredTriggers.Add(trigger);
            }
        }

        /// <summary>
        /// Unregister a trigger spot for special events
        /// </summary>
        public void UnregisterEventTrigger(SpecialEventTrigger trigger) {
            registeredTriggers.Remove(trigger);
        }

        /// <summary>
        /// Get the currently active event type
        /// </summary>
        public SpecialEventType GetCurrentEventType() {
            return currentActiveEvent != null ? currentActiveEvent.eventType : SpecialEventType.None;
        }

        /// <summary>
        /// Check if there is an active special event
        /// </summary>
        public bool IsEventActive() {
            return isEventActive;
        }
    }

    /// <summary>
    /// Types of special events
    /// </summary>
    public enum SpecialEventType {
        None,
        HealingShrine,       // A shrine that heals players
        TreasureRoom,        // A room with extra loot
        HordeBattle,         // A wave of enemies attacks
        MysteriousMerchant,  // A special merchant appears
        EliteEnemy,          // A powerful enemy spawns
        EnvironmentalHazard, // Environmental hazard like poison gas or fire
        Invasion,            // PvP invasion event
        LockdownChallenge,   // Players are locked in an area until completing a challenge
        TimeAnomaly,         // Time-based effects (speed up, slow down)
        BloodMoon,           // All enemies are stronger
        TrapMalfunction,     // All traps activate at once
        ShrineOfFortune      // Risk/reward gamble event
    }

    /// <summary>
    /// Data for a special event
    /// </summary>
    [Serializable]
    public class SpecialEventData {
        public SpecialEventType eventType;
        public string displayName;
        public string description;
        public float duration = 60f;
        public float cooldownTime = 300f;
        public GameObject eventPrefab;
        public AudioClip announcementSound;
    }
}