using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    public static class BuffSystem {
        // 임시 버프 적용
        public static void ApplyTemporaryBuff(PlayerCharacter target, StatType statType, float value, StatModifierType modType,
            float duration) {
            StatModifier modifier = new StatModifier(value, modType, "TempBuff");
            target.Stats.AddModifier(statType, modifier);

            // 지속시간 후 버프 제거를 위한 코루틴 시작
            // 실제 구현에서는 MonoBehaviour가 필요하므로 별도 매니저 클래스가 필요
            Debug.Log($"Applied temporary buff to {statType}: {value} for {duration} seconds");

            // 예시 코드 - 실제로는 이런 방식으로 처리하지 않음
            // 실제 구현에서는 버프 매니저를 통해 관리해야 함
            /*
            IEnumerator RemoveBuff()
            {
                yield return new WaitForSeconds(duration);
                target.Stats.RemoveAllModifiersFromSource("TempBuff");
            }
            */
        }

        // 영구 버프 적용
        public static void ApplyPermanentBuff(PlayerCharacter target, StatType statType, float value, StatModifierType modType) {
            StatModifier modifier = new StatModifier(value, modType, "PermanentBuff");
            target.Stats.AddModifier(statType, modifier);

            Debug.Log($"Applied permanent buff to {statType}: {value}");
        }
    }
}