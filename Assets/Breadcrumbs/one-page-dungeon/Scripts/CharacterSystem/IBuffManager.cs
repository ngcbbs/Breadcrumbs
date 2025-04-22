using System.Collections.Generic;

namespace Breadcrumbs.CharacterSystem {
    /// <summary>
    /// 버프 매니저 인터페이스
    /// </summary>
    public interface IBuffManager {
        /// <summary>
        /// 버프 적용
        /// </summary>
        ActiveBuff ApplyBuff(PlayerCharacter target, BuffData buffData, object source = null, float? customDuration = null);

        /// <summary>
        /// 버프 제거
        /// </summary>
        bool RemoveBuff(PlayerCharacter target, string buffId);

        /// <summary>
        /// 특정 소스의 모든 버프 제거
        /// </summary>
        void RemoveAllBuffsFromSource(PlayerCharacter target, object source);

        /// <summary>
        /// 캐릭터의 모든 버프 제거
        /// </summary>
        void RemoveAllBuffs(PlayerCharacter target);

        /// <summary>
        /// 캐릭터가 갖고 있는 모든 버프 조회
        /// </summary>
        List<ActiveBuff> GetAllBuffs(PlayerCharacter target);

        /// <summary>
        /// 특정 ID의 버프 조회
        /// </summary>
        ActiveBuff GetBuffById(PlayerCharacter target, string buffId);
    }
}