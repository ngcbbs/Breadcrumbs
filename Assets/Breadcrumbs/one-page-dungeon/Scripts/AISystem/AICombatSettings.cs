using System;
using UnityEngine;

namespace Breadcrumbs.AISystem {
    [CreateAssetMenu(fileName = "AICombatSettings", menuName = "Breadcrumbs/Tools/Create AICombatSettings (v2)")]
    public class AICombatSettings  : ScriptableObject{
        [Header("근접 공격 설정")]
        public float meleeAttackRange = 2f;
        public float meleeAttackCooldown = 1.5f;
        public float meleeAttackDuration = 0.5f;
        public float meleeAttackDamage = 10f;
        public float meleeAttackKnockback = 3f;
        public LayerMask meleeAttackLayers;
        
        [Header("원거리 공격 설정")]
        public float rangedAttackRange = 10f;
        public float rangedAttackCooldown = 2f;
        public float rangedAttackChargeTime = 0.5f;
        public float rangedAttackProjectileSpeed = 15f;
        public float rangedAttackDamage = 8f;
        public GameObject projectilePrefab;
        public Transform projectileSpawnPoint;
        public LayerMask rangedAttackLayers;
        
        [Header("타게팅 설정")]
        public float targetUpdateInterval = 0.5f;
        public float targetLostTime = 3f;
        public bool prioritizeClosestTarget = true;
        public LayerMask targetLayers;
    }
}