# 전투 시스템 예제 코드 (Part 1)

이 문서는 기본 전투 시스템의 구현을 보여줍니다. 공격 메커니즘, 히트박스 처리, 대미지 계산 및 전투 관련 피드백 시스템을 포함합니다.

## CombatSystem.cs

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CombatSystem : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngle = 60f;
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("콤보 설정")]
    [SerializeField] private bool enableCombos = true;
    [SerializeField] private float comboTimeWindow = 1.2f;
    [SerializeField] private int maxComboCount = 3;
    
    [Header("타격 이펙트")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject criticalHitEffect;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip criticalHitSound;
    
    // 레이어 마스크 및 충돌 설정
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private LayerMask obstacleLayer;
    
    // 컴포넌트 참조
    private CharacterStats characterStats;
    private Animator animator;
    private AudioSource audioSource;
    
    // 상태 변수
    private bool canAttack = true;
    private float lastAttackTime;
    private int currentCombo = 0;
    private float comboTimer = 0f;
    
    // 이벤트
    public event Action<GameObject, int, bool> OnHit;
    public event Action<int> OnComboUpdated;
    public event Action OnAttackStarted;
    public event Action OnAttackEnded;
    
    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }
    }
    
    private void Update()
    {
        // 콤보 타이머 업데이트
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }
    
    // 공격 시도
    public bool TryAttack()
    {
        if (!canAttack || Time.time - lastAttackTime < attackCooldown)
        {
            return false;
        }
        
        // 공격 시작
        StartAttack();
        return true;
    }
    
    // 방향을 향해 공격 시도
    public bool TryAttackInDirection(Vector3 direction)
    {
        if (TryAttack())
        {
            // 공격 방향으로 회전
            if (direction != Vector3.zero)
            {
                transform.forward = new Vector3(direction.x, 0, direction.z).normalized;
            }
            
            return true;
        }
        
        return false;
    }
    
    // 공격 시작
    private void StartAttack()
    {
        canAttack = false;
        lastAttackTime = Time.time;
        
        // 콤보 업데이트
        if (enableCombos)
        {
            UpdateCombo();
        }
        
        // 애니메이션 실행
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            
            if (enableCombos)
            {
                animator.SetInteger("ComboCount", currentCombo);
            }
        }
        
        // 이벤트 발생
        OnAttackStarted?.Invoke();
        
        // 실제 공격은 애니메이션 이벤트로 실행
        // 테스트를 위해 직접 호출 옵션도 제공
        // ExecuteAttack();
    }

    // 애니메이션 이벤트에서 호출될 실제 공격 실행 함수
    public void ExecuteAttack()
    {
        // 대미지 계산
        int baseDamage = characterStats.GetAttackDamage();
        bool isCritical = RollForCritical();
        int finalDamage = CalculateDamage(baseDamage, isCritical);
        
        // 공격 영역 내 대상 찾기
        List<GameObject> hitTargets = FindTargetsInAttackRange();
        
        // 대상별 대미지 적용
        foreach (var target in hitTargets)
        {
            ApplyDamage(target, finalDamage, isCritical);
        }
    }
    
    // 공격 종료 (애니메이션 이벤트에서 호출)
    public void EndAttack()
    {
        canAttack = true;
        
        // 이벤트 발생
        OnAttackEnded?.Invoke();
    }
    
    // 공격 범위 내 대상 찾기
    private List<GameObject> FindTargetsInAttackRange()
    {
        List<GameObject> targets = new List<GameObject>();
        
        // 공격 범위 내 콜라이더 검색
        Collider[] colliders = Physics.OverlapSphere(attackOrigin.position, attackRange, targetLayers);
        
        foreach (var collider in colliders)
        {
            // 대상 방향 계산
            Vector3 directionToTarget = (collider.transform.position - attackOrigin.position).normalized;
            
            // 시야각 계산
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            
            // 공격 각도 내에 있는지 확인
            if (angleToTarget <= attackAngle / 2)
            {
                // 장애물 확인
                RaycastHit hit;
                Vector3 targetPosition = collider.bounds.center;
                Vector3 directionToCenter = (targetPosition - attackOrigin.position).normalized;
                float distanceToTarget = Vector3.Distance(attackOrigin.position, targetPosition);
                
                if (!Physics.Raycast(attackOrigin.position, directionToCenter, out hit, distanceToTarget, obstacleLayer))
                {
                    // 대상에게 가시성이 있으면 타겟 목록에 추가
                    targets.Add(collider.gameObject);
                }
            }
        }
        
        return targets;
    }
    
    // 대미지 적용
    private void ApplyDamage(GameObject target, int damage, bool isCritical)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        
        if (damageable != null)
        {
            // 대미지 정보 생성
            DamageInfo damageInfo = new DamageInfo(
                damage,
                DamageType.Physical, // 기본값, 무기에 따라 변경 가능
                gameObject,
                isCritical
            );
            
            // 대미지 적용
            damageable.TakeDamage(damageInfo);
            
            // 이펙트 생성
            if (isCritical && criticalHitEffect != null)
            {
                Instantiate(criticalHitEffect, target.transform.position + Vector3.up, Quaternion.identity);
            }
            else if (hitEffect != null)
            {
                Instantiate(hitEffect, target.transform.position + Vector3.up, Quaternion.identity);
            }
            
            // 사운드 재생
            if (audioSource != null)
            {
                audioSource.clip = isCritical ? criticalHitSound : hitSound;
                audioSource.Play();
            }
            
            // 이벤트 발생
            OnHit?.Invoke(target, damage, isCritical);
        }
    }
    
    // 치명타 확률 계산
    private bool RollForCritical()
    {
        float criticalChance = characterStats.GetCriticalChance();
        return UnityEngine.Random.value <= criticalChance;
    }
    
    // 최종 대미지 계산
    private int CalculateDamage(int baseDamage, bool isCritical)
    {
        float criticalMultiplier = characterStats.GetCriticalDamageMultiplier();
        float damageMultiplier = isCritical ? criticalMultiplier : 1f;
        
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    // 콤보 업데이트
    private void UpdateCombo()
    {
        if (currentCombo < maxComboCount)
        {
            currentCombo++;
        }
        else
        {
            currentCombo = 1;
        }
        
        comboTimer = comboTimeWindow;
        OnComboUpdated?.Invoke(currentCombo);
    }
    
    // 콤보 초기화
    private void ResetCombo()
    {
        currentCombo = 0;
        comboTimer = 0f;
        
        if (animator != null)
        {
            animator.SetInteger("ComboCount", 0);
        }
        
        OnComboUpdated?.Invoke(currentCombo);
    }
    
    // 공격 쿨다운 시간 반환 (외부에서 참조용)
    public float GetAttackCooldown()
    {
        return attackCooldown;
    }
    
    // 현재 콤보 상태 반환
    public int GetCurrentCombo()
    {
        return currentCombo;
    }
    
    // 공격 가능 여부 확인
    public bool CanAttack()
    {
        return canAttack && Time.time - lastAttackTime >= attackCooldown;
    }
    
    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null)
            return;
            
        // 공격 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, attackRange);
        
        // 공격 각도 시각화
        Vector3 forward = transform.forward * attackRange;
        float halfAngle = attackAngle * 0.5f * Mathf.Deg2Rad;
        Vector3 right = new Vector3(Mathf.Sin(halfAngle), 0, Mathf.Cos(halfAngle)) * attackRange;
        Vector3 left = new Vector3(-right.x, 0, right.z);
        
        Vector3 origin = attackOrigin.position;
        
        Gizmos.DrawRay(origin, forward);
        Gizmos.DrawRay(origin, Quaternion.Euler(0, attackAngle * 0.5f, 0) * forward);
        Gizmos.DrawRay(origin, Quaternion.Euler(0, -attackAngle * 0.5f, 0) * forward);
        
        // 부채꼴 표시
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Vector3 prev = origin + Quaternion.Euler(0, -attackAngle * 0.5f, 0) * forward;
        
        for (int i = 0; i <= 20; i++)
        {
            float angle = -attackAngle * 0.5f + attackAngle * i / 20f;
            Vector3 next = origin + Quaternion.Euler(0, angle, 0) * forward;
            
            Gizmos.DrawLine(origin, next);
            Gizmos.DrawLine(prev, next);
            
            prev = next;
        }
    }
}

## CharacterStats.cs (관련 부분)

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("기본 스탯")]
    [SerializeField] private int baseHealth = 100;
    [SerializeField] private int baseAttackDamage = 10;
    [SerializeField] private float baseAttackSpeed = 1.0f;
    [SerializeField] private int baseDefense = 5;
    
    [Header("고급 전투 스탯")]
    [SerializeField] private float baseCriticalChance = 0.05f; // 5% 기본 치명타 확률
    [SerializeField] private float baseCriticalMultiplier = 1.5f; // 50% 추가 대미지
    
    // 현재 스탯 값
    private int currentHealth;
    
    // 스탯 수정자 (장비, 버프 등에 의한)
    private List<StatModifier> attackDamageModifiers = new List<StatModifier>();
    private List<StatModifier> attackSpeedModifiers = new List<StatModifier>();
    private List<StatModifier> defenseModifiers = new List<StatModifier>();
    private List<StatModifier> criticalChanceModifiers = new List<StatModifier>();
    private List<StatModifier> criticalMultiplierModifiers = new List<StatModifier>();
    
    // 이벤트
    public event Action<int, int> OnHealthChanged; // 현재 체력, 최대 체력
    public event Action OnDeath;
    
    // 처리된 최종 스탯 캐시
    private int cachedMaxHealth;
    private int cachedAttackDamage;
    private float cachedAttackSpeed;
    private int cachedDefense;
    private float cachedCriticalChance;
    private float cachedCriticalMultiplier;
    
    // 캐시 갱신 필요 플래그
    private bool isDirty = true;
    
    private void Awake()
    {
        // 체력 초기화
        RecalculateStats();
        currentHealth = cachedMaxHealth;
    }
    
    private void Start()
    {
        // 시작 시 이벤트 발생
        OnHealthChanged?.Invoke(currentHealth, cachedMaxHealth);
    }
    
    // 스탯 다시 계산 (수정자 적용)
    public void RecalculateStats()
    {
        cachedMaxHealth = CalculateFinalStatValue(baseHealth, null);
        cachedAttackDamage = CalculateFinalStatValue(baseAttackDamage, attackDamageModifiers);
        cachedAttackSpeed = CalculateFinalStatValue(baseAttackSpeed, attackSpeedModifiers);
        cachedDefense = CalculateFinalStatValue(baseDefense, defenseModifiers);
        
        // 크리티컬 관련 스탯은 0-1 사이로 클램프
        cachedCriticalChance = Mathf.Clamp01(CalculateFinalStatValue(baseCriticalChance, criticalChanceModifiers));
        cachedCriticalMultiplier = CalculateFinalStatValue(baseCriticalMultiplier, criticalMultiplierModifiers);
        
        // 체력이 최대 체력을 넘지 않도록 보정
        currentHealth = Mathf.Min(currentHealth, cachedMaxHealth);
        
        // 캐시 갱신 완료
        isDirty = false;
    }
    
    // 스탯 수정자 적용한 최종 값 계산
    private T CalculateFinalStatValue<T>(T baseValue, List<StatModifier> modifiers) where T : IConvertible
    {
        if (modifiers == null || modifiers.Count == 0)
            return baseValue;
            
        float finalValue = Convert.ToSingle(baseValue);
        float additive = 0f;
        float percentAdd = 0f;
        float percentMult = 1f;
        
        // 수정자 타입별로 적용
        foreach (var modifier in modifiers)
        {
            switch (modifier.Type)
            {
                case ModifierType.Additive:
                    additive += modifier.Value;
                    break;
                    
                case ModifierType.PercentAdd:
                    percentAdd += modifier.Value;
                    break;
                    
                case ModifierType.PercentMultiply:
                    percentMult *= (1 + modifier.Value);
                    break;
            }
        }
        
        // 순서대로 적용
        finalValue += additive; // 1. 가산 수정자 적용
        finalValue *= (1 + percentAdd); // 2. 퍼센트 가산 적용
        finalValue *= percentMult; // 3. 퍼센트 승산 적용
        
        // 타입에 맞게 변환하여 반환
        if (typeof(T) == typeof(int))
            return (T)(object)Mathf.RoundToInt(finalValue);
        else if (typeof(T) == typeof(float))
            return (T)(object)finalValue;
        
        return baseValue;
    }
    
    // 대미지 적용
    public void TakeDamage(int amount, GameObject attacker)
    {
        // 방어력 적용한 대미지 계산
        int reducedAmount = Mathf.Max(1, amount - cachedDefense);
        
        // 체력 감소
        currentHealth = Mathf.Max(0, currentHealth - reducedAmount);
        
        // 이벤트 발생
        OnHealthChanged?.Invoke(currentHealth, cachedMaxHealth);
        
        // 사망 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // 사망 처리
    private void Die()
    {
        // 이벤트 발생
        OnDeath?.Invoke();
        
        // 추가 사망 처리 로직...
    }
    
    // 무기 대미지 추가 (장비 시스템에서 호출)
    public void AddWeaponDamage(int amount)
    {
        attackDamageModifiers.Add(new StatModifier(StatType.Attack, ModifierType.Additive, amount, "Weapon"));
        isDirty = true;
        RecalculateStats();
    }
    
    // 무기 대미지 제거 (장비 해제 시 호출)
    public void RemoveWeaponDamage(int amount)
    {
        attackDamageModifiers.RemoveAll(m => m.Source == "Weapon" && m.Type == ModifierType.Additive && m.Value == amount);
        isDirty = true;
        RecalculateStats();
    }
    
    // 공격 속도 추가
    public void AddAttackSpeed(float amount)
    {
        attackSpeedModifiers.Add(new StatModifier(StatType.AttackSpeed, ModifierType.PercentAdd, amount, "Weapon"));
        isDirty = true;
        RecalculateStats();
    }
    
    // 공격 속도 제거
    public void RemoveAttackSpeed(float amount)
    {
        attackSpeedModifiers.RemoveAll(m => m.Source == "Weapon" && m.Type == ModifierType.PercentAdd && m.Value == amount);
        isDirty = true;
        RecalculateStats();
    }
    
    // 공격력 반환
    public int GetAttackDamage()
    {
        if (isDirty)
            RecalculateStats();
            
        return cachedAttackDamage;
    }
    
    // 치명타 확률 반환
    public float GetCriticalChance()
    {
        if (isDirty)
            RecalculateStats();
            
        return cachedCriticalChance;
    }
    
    // 치명타 배율 반환
    public float GetCriticalDamageMultiplier()
    {
        if (isDirty)
            RecalculateStats();
            
        return cachedCriticalMultiplier;
    }
    
    // 체력 회복
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(cachedMaxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, cachedMaxHealth);
    }
```
