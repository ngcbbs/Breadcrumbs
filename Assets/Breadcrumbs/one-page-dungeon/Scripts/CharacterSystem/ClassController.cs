namespace Breadcrumbs.CharacterSystem {
    public abstract class ClassController {
        protected PlayerCharacter character;
        protected CharacterStats stats;

        public ClassController(PlayerCharacter character) {
            this.character = character;
            this.stats = character.Stats;
        }

        // 직업별 초기화
        public virtual void Initialize() { }

        // 직업별 업데이트
        public virtual void Update(float deltaTime) { }

        // 직업별 특수 능력 발동
        public abstract void ActivateClassSpecial();

        // 레벨업 시 직업별 특수 처리
        public virtual void OnLevelUp() { }
    }
}