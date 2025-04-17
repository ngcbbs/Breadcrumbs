using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class AICombatController : MonoBehaviour {
        public AICombatSettings combatSettings;
        
        [Header("전투 행동 가중치")]
        [Range(0, 1)] public float meleeAttackWeight = 0.8f;
        [Range(0, 1)] public float rangedAttackWeight = 0.7f;
        
        private List<AICombatBehaviorBase> _combatBehaviors = new List<AICombatBehaviorBase>();
        private AICombatBehaviorBase _currentCombatBehavior;
        private AIContextData _contextData;
        
        private Dictionary<Type, float> _combatCooldowns = new Dictionary<Type, float>();
        
        private void Start() {
            InitializeComponents();
        }
        
        private void InitializeComponents() {
            // 이동 컨트롤러로부터 컨텍스트 데이터 가져오기
            AIMovementController movementController = GetComponent<AIMovementController>();
            if (movementController != null) {
                _contextData = new AIContextData(transform, movementController.target);
            } else {
                _contextData = new AIContextData(transform, null);
            }
            
            // 컨텍스트 데이터 초기화
            _contextData.customData["inMeleeRange"] = false;
            _contextData.customData["inRangedRange"] = false;
            
            // 전투 행동 등록
            RegisterCombatBehavior(new MeleeAttackBehavior(combatSettings), meleeAttackWeight);
            RegisterCombatBehavior(new RangedAttackBehavior(combatSettings), rangedAttackWeight);
        }
        
        private void Update() {
            if (_contextData == null) return;
            
            // 컨텍스트 데이터 업데이트
            _contextData.UpdateData();
            _contextData.UpdateCombatData();
            
            UpdateCombatCooldowns();
            
            // 행동 선택 및 실행
            AICombatBehaviorBase newBehavior = SelectBestCombatBehavior();
            
            if (newBehavior != _currentCombatBehavior) {
                _currentCombatBehavior?.Cleanup(_contextData);
                _currentCombatBehavior = newBehavior;
                _currentCombatBehavior?.Initialize(_contextData);
            }
            
            _currentCombatBehavior?.Execute(_contextData);
        }
        
        private void UpdateCombatCooldowns() {
            List<Type> expiredCooldowns = new List<Type>();
            
            foreach (var cooldown in _combatCooldowns) {
                _combatCooldowns[cooldown.Key] -= Time.deltaTime;
                
                if (_combatCooldowns[cooldown.Key] <= 0) {
                    expiredCooldowns.Add(cooldown.Key);
                }
            }
            
            foreach (var type in expiredCooldowns) {
                _combatCooldowns.Remove(type);
            }
        }
        
        private AICombatBehaviorBase SelectBestCombatBehavior() {
            AICombatBehaviorBase bestBehavior = null;
            float highestScore = float.MinValue;
            
            foreach (var behavior in _combatBehaviors) {
                // 쿨다운 중인 행동은 제외
                Type behaviorType = behavior.GetType();
                if (_combatCooldowns.ContainsKey(behaviorType)) {
                    continue;
                }
                
                // 점수 = 적합성 * 가중치
                float weight = GetCombatBehaviorWeight(behavior);
                float suitability = behavior.EvaluateSuitability(_contextData);
                float score = suitability * weight;
                
                if (score > highestScore) {
                    highestScore = score;
                    bestBehavior = behavior;
                }
            }
            
            return bestBehavior;
        }
        
        private float GetCombatBehaviorWeight(AICombatBehaviorBase behavior) {
            if (behavior is MeleeAttackBehavior) return meleeAttackWeight;
            if (behavior is RangedAttackBehavior) return rangedAttackWeight;
            return 0.5f;
        }
        
        public void SetCombatBehaviorCooldown<T>(float cooldownTime) where T : AICombatBehaviorBase {
            _combatCooldowns[typeof(T)] = cooldownTime;
        }
        
        private void RegisterCombatBehavior(AICombatBehaviorBase behavior, float weight) {
            _combatBehaviors.Add(behavior);
        }
        
        public bool IsPerformingCombatAction() {
            return _currentCombatBehavior != null && _currentCombatBehavior.IsActionInProgress();
        }
        
        private void OnDrawGizmos() {
            if (_currentCombatBehavior != null && _contextData != null) {
                _currentCombatBehavior.DrawGizmos(_contextData);
            }
        }
    }
}