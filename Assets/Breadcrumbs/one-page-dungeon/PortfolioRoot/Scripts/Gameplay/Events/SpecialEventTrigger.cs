using System.Collections.Generic;
using UnityEngine;

namespace GamePortfolio.Gameplay.Events
{
    /// <summary>
    /// Marks a location where special events can be spawned
    /// </summary>
    public class SpecialEventTrigger : MonoBehaviour
    {
        [SerializeField] private List<SpecialEventType> compatibleEventTypes = new List<SpecialEventType>();
        [SerializeField] private bool allowAllEvents = false;
        [SerializeField] private float triggerRadius = 1.5f;
        [SerializeField] private Color gizmoColor = new Color(1f, 0.5f, 0f, 0.5f);
        
        private void OnEnable()
        {
            // Register with the event system
            if (SpecialEventSystem.HasInstance)
            {
                SpecialEventSystem.Instance.RegisterEventTrigger(this);
            }
        }
        
        private void OnDisable()
        {
            // Unregister from the event system
            if (SpecialEventSystem.HasInstance)
            {
                SpecialEventSystem.Instance.UnregisterEventTrigger(this);
            }
        }
        
        /// <summary>
        /// Check if this trigger is compatible with a specific event type
        /// </summary>
        public bool IsCompatibleWithEventType(SpecialEventType eventType)
        {
            return allowAllEvents || compatibleEventTypes.Contains(eventType);
        }
        
        /// <summary>
        /// Manually trigger a specific event at this location
        /// </summary>
        public void TriggerEvent(SpecialEventType eventType)
        {
            if (IsCompatibleWithEventType(eventType) && SpecialEventSystem.HasInstance)
            {
                SpecialEventSystem.Instance.TriggerEventOfType(eventType);
            }
        }
        
        /// <summary>
        /// Manually trigger a random compatible event at this location
        /// </summary>
        public void TriggerRandomEvent()
        {
            if (SpecialEventSystem.HasInstance)
            {
                SpecialEventSystem.Instance.TriggerRandomEvent();
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, 0.3f);
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}