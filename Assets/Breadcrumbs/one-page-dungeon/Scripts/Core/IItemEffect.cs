using Breadcrumbs.CharacterSystem;

namespace Breadcrumbs.Core {
    /// <summary>
    /// 아이템에 적용될 수 있는 효과 인터페이스
    /// </summary>
    public interface IItemEffect
    {
        string EffectName { get; }
        string Description { get; }
        
        // 효과 적용 및 제거
        void Apply(PlayerCharacter character);
        void Remove(PlayerCharacter character);
        
        // 효과 복제
        IItemEffect Clone();
    }
}