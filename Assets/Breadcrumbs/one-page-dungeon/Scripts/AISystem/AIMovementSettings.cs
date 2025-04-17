using System;
using UnityEngine;

namespace Breadcrumbs.AISystem {
    [CreateAssetMenu(fileName = "AIMovementSettings", menuName = "Breadcrumbs/Tools/Create AIMovementSettings (v2)")]
    // 모든 AI 이동 설정을 통합한 셋팅
    public class AIMovementSettings : ScriptableObject {
        [Header("기본 이동 설정")]
        public float moveSpeed = 3.5f;
        public float turnSpeed = 5f;
        public float raycastDistance = 3f;
        public float separationDistance = 2f;

        [Header("전투 이동 설정")]
        public float strafingRadius = 5f;
        public float strafingSpeed = 0.5f;
        public bool changeStrafingDirection = true;
        public float detectionRadius = 10f;
        
        [Header("전투 이동 공격 설정")]
        public float attackRangeRatio = 0.75f;
        public float attackDamage = 10f;
        public float attackDuration = 0.5f;

        [Header("대시 설정")]
        public float dashSpeed = 10f;
        public float dashDuration = 0.3f;
        public float dashCooldown = 3f;
        public float dashProbability = 0.2f;

        [Header("회피 설정")]
        public float dodgeSpeed = 8f;
        public float dodgeDuration = 0.25f;
        public float dodgeCooldown = 2f;
        public float threatDetectionRadius = 5f;

        [Header("순찰 설정")]
        public float patrolSpeed = 2f;
        public float waypointReachedDistance = 0.5f;
        public float waypointWaitTime = 1f;

        [Header("집단 이동 설정")]
        public float formationSpacing = 2f;
        public float cohesionStrength = 0.5f;
        public float alignmentStrength = 0.3f;
    }
}