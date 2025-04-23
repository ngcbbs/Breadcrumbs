using System;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 스탯 수정자 타입
    /// </summary>
    public enum StatModifierType
    {
        Flat,           // 고정값 더하기 (10 + 5 = 15)
        PercentAdd,     // 퍼센트 더하기 (100 + 10% + 20% = 100 + 30% = 130)
        PercentMult     // 퍼센트 곱하기 (100 * (1 + 10%) * (1 + 20%) = 100 * 1.1 * 1.2 = 132)
    }

    /// <summary>
    /// 스탯 수정자 클래스
    /// 캐릭터 스탯을 수정하는데 사용됩니다.
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        [SerializeField] private string _statName;
        [SerializeField] private float _value;
        [SerializeField] private StatModifierType _type;
        [SerializeField] private int _order;
        [SerializeField] private object _source;

        public string StatName => _statName;
        public float Value => _value;
        public StatModifierType Type => _type;
        public int Order => _order;
        public object Source => _source;

        public StatModifier(string statName, float value, StatModifierType type, int order, object source = null)
        {
            _statName = statName;
            _value = value;
            _type = type;
            _order = order;
            _source = source;
        }

        // 스탯 수정자 타입에 따른 기본 순서
        public static int GetDefaultOrder(StatModifierType type)
        {
            switch (type)
            {
                case StatModifierType.Flat: return 0;
                case StatModifierType.PercentAdd: return 1;
                case StatModifierType.PercentMult: return 2;
                default: return 0;
            }
        }

        // 값과 타입만 제공하는 생성자 (기본 순서 사용)
        public StatModifier(string statName, float value, StatModifierType type) 
            : this(statName, value, type, GetDefaultOrder(type)) { }

        // 값, 타입, 소스 제공하는 생성자
        public StatModifier(string statName, float value, StatModifierType type, object source) 
            : this(statName, value, type, GetDefaultOrder(type), source) { }
    }
}
