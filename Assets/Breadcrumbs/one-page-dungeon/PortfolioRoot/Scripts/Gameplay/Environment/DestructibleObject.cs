using System;
using System.Collections;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Environment
{
    /// <summary>
    /// Represents an object in the environment that can be destroyed
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DestructibleObject : MonoBehaviour, IDamageable
    {
        [Header("Destructible Settings")]
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool invulnerable = false;
        [SerializeField] private DamageResistance[] resistances;
        [SerializeField] private DamageType weaknessType = DamageType.None;
        [SerializeField] private float weaknessMultiplier = 2f;
        
        [Header("Destruction Effects")]
        [SerializeField] private GameObject destroyedVersion;
        [SerializeField] private GameObject destroyVFX;
        [SerializeField] private AudioClip destroySound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private float debrisLifetime = 20f;
        [SerializeField] private bool disableColliderOnDestroy = true;
        
        [Header("Physics")]
        [SerializeField] private bool usePhysicsOnDestroy = true;
        [SerializeField] private float explosionForce = 500f;
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private float upwardsModifier = 0.2f;
        
        [Header("Drops")]
        [SerializeField] private GameObject[] possibleDrops;
        [SerializeField] private float dropChance = 0.5f;
        
        // Components
        private Collider objectCollider;
        private AudioSource audioSource;
        
        // Events
        public event Action<float, float> OnHealthChanged;
        public event Action OnDestroyed;
        
        // Damage handling
        private bool isDestroyed = false;
        private float lastDamageTime = -999f;
        private float damageFlashDuration = 0.2f;
        private Material[] originalMaterials;
        private Renderer[] renderers;
        
        private void Awake()
        {
            objectCollider = GetComponent<Collider>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
            }
            
            currentHealth = maxHealth;
            
            // Cache renderers and materials for damage flash
            renderers = GetComponentsInChildren<Renderer>();
            CacheOriginalMaterials();
        }
        
        private void CacheOriginalMaterials()
        {
            int materialCount = 0;
            foreach (var renderer in renderers)
            {
                materialCount += renderer.materials.Length;
            }
            
            originalMaterials = new Material[materialCount];
            
            int index = 0;
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    originalMaterials[index] = material;
                    index++;
                }
            }
        }
        
        /// <summary>
        /// Apply damage to this destructible object
        /// </summary>
        public void TakeDamage(float amount)
        {
            TakeDamage(amount, DamageType.Physical, null);
        }
        
        /// <summary>
        /// Apply damage to this destructible object with damage type
        /// </summary>
        public void TakeDamage(float amount, DamageType damageType, GameObject instigator)
        {
            if (invulnerable || isDestroyed)
                return;
                
            // Apply weakness multiplier
            if (damageType == weaknessType)
            {
                amount *= weaknessMultiplier;
            }
            
            // Apply resistances
            if (resistances != null)
            {
                foreach (var resistance in resistances)
                {
                    if (resistance.type == damageType)
                    {
                        amount *= (1f - resistance.resistancePercentage);
                        break;
                    }
                }
            }
            
            // Update health
            currentHealth -= amount;
            
            // Trigger event
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Play hit sound
            if (hitSound != null && audioSource != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(hitSound);
            }
            
            // Visual feedback
            StartCoroutine(DamageFlash());
            
            // Check for destruction
            if (currentHealth <= 0 && !isDestroyed)
            {
                Destroy();
            }
        }
        
        /// <summary>
        /// Destroy this object
        /// </summary>
        public void Destroy()
        {
            if (isDestroyed)
                return;
                
            isDestroyed = true;
            
            // Trigger event
            OnDestroyed?.Invoke();
            
            // Play destroy sound
            if (destroySound != null && audioSource != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(destroySound);
            }
            
            // Spawn destroy VFX
            if (destroyVFX != null)
            {
                Instantiate(destroyVFX, transform.position, Quaternion.identity);
            }
            
            // Handle collider
            if (disableColliderOnDestroy && objectCollider != null)
            {
                objectCollider.enabled = false;
            }
            
            // Spawn destroyed version
            if (destroyedVersion != null)
            {
                GameObject destroyed = Instantiate(destroyedVersion, transform.position, transform.rotation);
                
                // Apply explosion force if physics enabled
                if (usePhysicsOnDestroy)
                {
                    Rigidbody[] rigidbodies = destroyed.GetComponentsInChildren<Rigidbody>();
                    foreach (var rb in rigidbodies)
                    {
                        rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier);
                    }
                }
                
                // Destroy after lifetime
                Destroy(destroyed, debrisLifetime);
            }
            
            // Spawn drops
            SpawnDrops();
            
            // Hide original object
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }
            
            // Destroy this gameobject after sound finished playing
            float destroySoundDuration = destroySound != null ? destroySound.length : 0f;
            Destroy(gameObject, destroySoundDuration + 0.1f);
        }
        
        /// <summary>
        /// Spawn random drops from the possible drops list
        /// </summary>
        private void SpawnDrops()
        {
            if (possibleDrops == null || possibleDrops.Length == 0)
                return;
                
            // Chance to spawn a drop
            if (UnityEngine.Random.value <= dropChance)
            {
                // Select a random drop
                int dropIndex = UnityEngine.Random.Range(0, possibleDrops.Length);
                
                if (possibleDrops[dropIndex] != null)
                {
                    // Spawn with slight offset and random rotation
                    Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                    Quaternion spawnRot = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                    
                    Instantiate(possibleDrops[dropIndex], spawnPos, spawnRot);
                }
            }
        }
        
        /// <summary>
        /// Visual feedback when taking damage
        /// </summary>
        private IEnumerator DamageFlash()
        {
            // Record last damage time
            lastDamageTime = Time.time;
            
            // Create flash materials
            foreach (var renderer in renderers)
            {
                Material[] flashMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    // Create instance of material
                    Material flashMaterial = new Material(renderer.materials[i]);
                    // Set to emission mode
                    flashMaterial.EnableKeyword("_EMISSION");
                    flashMaterial.SetColor("_EmissionColor", Color.white);
                    
                    flashMaterials[i] = flashMaterial;
                }
                
                // Apply flash materials
                renderer.materials = flashMaterials;
            }
            
            // Wait for flash duration
            yield return new WaitForSeconds(damageFlashDuration);
            
            // Restore original materials (if not destroyed)
            if (!isDestroyed)
            {
                int materialIndex = 0;
                foreach (var renderer in renderers)
                {
                    Material[] originalMats = new Material[renderer.materials.Length];
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        originalMats[i] = originalMaterials[materialIndex];
                        materialIndex++;
                    }
                    
                    renderer.materials = originalMats;
                }
            }
        }
        
        /// <summary>
        /// Check if object is alive (not destroyed)
        /// </summary>
        public bool IsAlive()
        {
            return !isDestroyed;
        }
        
        /// <summary>
        /// Set object to be invulnerable
        /// </summary>
        public void SetInvulnerable(bool invulnerable)
        {
            this.invulnerable = invulnerable;
        }
        
        /// <summary>
        /// Heal the object
        /// </summary>
        public void Heal(float amount)
        {
            if (isDestroyed)
                return;
                
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            
            // Trigger event
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
    
    /// <summary>
    /// Struct for defining damage resistances
    /// </summary>
    [Serializable]
    public struct DamageResistance
    {
        public DamageType type;
        
        [Range(0f, 1f)]
        public float resistancePercentage;
    }
}