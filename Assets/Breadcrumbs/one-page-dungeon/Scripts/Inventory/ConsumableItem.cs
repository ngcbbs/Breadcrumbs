using System;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 소비 아이템 클래스
    /// </summary>
    [Serializable]
    public class ConsumableItem : InventoryItemBase
    {
        [SerializeField] private bool _isReusable;
        [SerializeField] private int _uses;
        [SerializeField] private int _maxUses;
        [SerializeField] private float _cooldown;
        [SerializeField] private float _lastUseTime;

        public bool IsReusable => _isReusable;
        public int Uses 
        { 
            get => _uses; 
            set => _uses = Mathf.Clamp(value, 0, _maxUses);
        }
        public int MaxUses => _maxUses;
        public float Cooldown => _cooldown;
        public float LastUseTime 
        { 
            get => _lastUseTime; 
            private set => _lastUseTime = value;
        }
        public bool IsOnCooldown => Time.time - LastUseTime < Cooldown;
        public bool HasUses => IsReusable || Uses > 0;

        public ConsumableItem(string id, string displayName, string description, Sprite icon, 
            ItemRarity rarity, bool isStackable = true, int maxStackCount = 99, 
            bool isReusable = false, int maxUses = 1, float cooldown = 0f,
            int width = 1, int height = 1) 
            : base(id, displayName, description, icon, ItemType.Consumable, rarity, width, height, isStackable, maxStackCount)
        {
            _isReusable = isReusable;
            _maxUses = Mathf.Max(1, maxUses);
            _uses = _maxUses;
            _cooldown = Mathf.Max(0f, cooldown);
            _lastUseTime = -cooldown; // 처음에는 즉시 사용 가능
        }

        /// <summary>
        /// 아이템 사용 메서드
        /// </summary>
        public override bool Use()
        {
            // 쿨다운 중이거나 사용 가능 횟수가 없으면 사용 불가
            if (IsOnCooldown || !HasUses)
                return false;

            // 아이템 사용 효과 적용 (상속 클래스에서 구현)
            bool used = OnUse();
            
            if (used)
            {
                // 사용 시간 기록
                LastUseTime = Time.time;
                
                // 재사용 가능하지 않은 경우 사용 횟수 감소
                if (!IsReusable)
                {
                    Uses--;
                }
                
                // 스택 가능한 아이템은 스택에서 하나 제거
                if (IsStackable)
                {
                    RemoveFromStack(1);
                }
            }
            
            return used;
        }
        
        /// <summary>
        /// 상속 클래스에서 구현할 실제 사용 효과
        /// </summary>
        protected virtual bool OnUse()
        {
            // 기본 구현은 단순히 사용됨을 반환
            return true;
        }
        
        /// <summary>
        /// 아이템 충전 (사용 횟수 회복)
        /// </summary>
        public virtual bool Recharge(int amount)
        {
            if (amount <= 0 || Uses >= MaxUses)
                return false;
                
            Uses += amount;
            return true;
        }
        
        /// <summary>
        /// 쿨다운 리셋
        /// </summary>
        public virtual void ResetCooldown()
        {
            LastUseTime = -Cooldown;
        }
    }
}
