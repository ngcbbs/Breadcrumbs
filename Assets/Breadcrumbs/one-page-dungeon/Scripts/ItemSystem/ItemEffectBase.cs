using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;

namespace Breadcrumbs.ItemSystem {
    /// <summary>
    /// 아이템 효과의 기본 추상 클래스
    /// </summary>
    public abstract class ItemEffectBase : IItemEffect
    {
        public string EffectName { get; protected set; }
        public string Description { get; protected set; }
        
        public abstract void Apply(PlayerCharacter character);
        public abstract void Remove(PlayerCharacter character);
        public abstract IItemEffect Clone();
    }
}