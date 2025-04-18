using System;
using UnityEngine;

namespace Breadcrumbs.Player {
    public class ContextData {
        public float health = 100f;
        public GameObject currentTarget;
        public static event EventHandler<HealthChangedEventArgs> OnHealthChanged;

        public class HealthChangedEventArgs : EventArgs {
            public float NewHealth { get; }
            public float OldHealth { get; }

            public HealthChangedEventArgs(float newHealth, float oldHealth) {
                NewHealth = newHealth;
                OldHealth = oldHealth;
            }
        }

        public void SetHealth(float newHealth) {
            float oldHealth = health;
            health = newHealth;
            OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(newHealth, oldHealth));
        }
    }
}