# UI/UX 디자인 및 구현

## 시스템 개요

UI/UX 시스템은 던전 크롤러 게임의 사용자 인터페이스와 경험을 구현합니다. 이 시스템은 게임 정보 표시, 플레이어 상호작용, 시각적 피드백을 담당하며, 직관적이고 몰입감 있는 게임 경험을 제공하는 것을 목표로 합니다.

## 설계 원칙

1. **가독성**: 중요 정보를 명확하게 전달
2. **즉각적 피드백**: 플레이어 액션에 대한 빠른 응답
3. **일관성**: 일관된 시각적 언어와 상호작용 패턴
4. **비침투성**: 게임플레이를 방해하지 않는 UI 디자인
5. **확장 가능성**: 새로운 기능 추가에 유연한 구조

## UI 시스템 아키텍처

### UI 관리자

```csharp
// UI 관리자 클래스
public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UIManager Instance { get; private set; }
    
    // UI 화면 참조
    [SerializeField] private GameObject hudScreen;
    [SerializeField] private GameObject inventoryScreen;
    [SerializeField] private GameObject pauseMenuScreen;
    [SerializeField] private GameObject deathScreen;
    
    // UI 컴포넌트 참조
    [SerializeField] private HUDController hudController;
    [SerializeField] private InventoryUIController inventoryController;
    [SerializeField] private InteractionPromptController interactionPromptController;
    
    // 현재 활성화된 화면 추적
    private List<GameObject> activeScreens = new List<GameObject>();
    
    // UI 상태 관리 및 화면 전환 메서드
    public void ShowScreen(GameObject screen)
    {
        // 구현...
    }
    
    public void HideScreen(GameObject screen)
    {
        // 구현...
    }
    
    // 이벤트 리스너 및 UI 업데이트 메서드
    public void UpdatePlayerHealth(float currentHealth, float maxHealth)
    {
        hudController.UpdateHealthBar(currentHealth, maxHealth);
    }
    
    // 추가 메서드...
}
```

### UI 컴포넌트 계층

```
UIManager (전체 UI 관리)
├── GameScreens (화면 관리)
│   ├── HUDScreen
│   ├── InventoryScreen
│   ├── PauseMenuScreen
│   ├── DeathScreen
│   └── MainMenuScreen
├── UIControllers (기능별 컨트롤러)
│   ├── HUDController
│   ├── InventoryUIController
│   ├── MinimapController
│   └── InteractionPromptController
└── UIElements (재사용 가능한 UI 요소)
    ├── HealthBar
    ├── ItemSlot
    ├── SkillIcon
    └── DialogueBox
```

## 핵심 UI 요소

### 헤드업 디스플레이(HUD)

#### 플레이어 상태 표시
- **체력 바**: 현재/최대 체력 시각화
- **에너지/마나 바**: 자원 게이지 표시
- **상태 효과 아이콘**: 버프/디버프 표시
- **경험치 바**: 레벨 진행 상황 표시

```csharp
// HUD 컨트롤러
public class HUDController : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Transform statusEffectsContainer;
    [SerializeField] private GameObject statusEffectPrefab;
    
    // 상태 업데이트 메서드
    public void UpdateHealthBar(float current, float max)
    {
        healthBar.value = current / max;
    }
    
    public void UpdateEnergyBar(float current, float max)
    {
        energyBar.value = current / max;
    }
    
    // 상태 효과 관리
    public void AddStatusEffect(StatusEffectData effect)
    {
        // 구현...
    }
    
    public void RemoveStatusEffect(string effectId)
    {
        // 구현...
    }
}
```

#### 미니맵
- **동적 맵 표시**: 탐험한 영역 표시
- **중요 지점 마킹**: 목표, 포탈, 보물 표시
- **플레이어 및 팀원 위치**: 현재 위치 및 방향 표시
- **확대/축소 및 투명도 조절**: 사용자 설정 옵션

#### 상호작용 프롬프트
- **컨텍스트 액션**: 근처 상호작용 가능 객체 표시
- **키 바인딩 표시**: 필요한 입력 안내
- **상호작용 진행 표시**: 시간이 필요한 작업 진행바

### 인벤토리 시스템

#### 그리드 기반 인벤토리
- **슬롯 시스템**: 아이템 저장 및 관리
- **아이템 정보**: 상세 정보 표시 및 비교
- **분류 및 정렬**: 아이템 필터링 및 정렬 기능
- **드래그 앤 드롭**: 직관적인 아이템 조작

```csharp
// 인벤토리 UI 컨트롤러
public class InventoryUIController : MonoBehaviour
{
    [SerializeField] private Transform itemGrid;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private ItemTooltip tooltip;
    
    private List<ItemSlotUI> itemSlots = new List<ItemSlotUI>();
    
    // 인벤토리 초기화
    public void InitializeInventory(int slotCount)
    {
        // 슬롯 생성...
    }
    
    // 아이템 추가/제거
    public void UpdateSlot(int slotIndex, ItemData item)
    {
        // 구현...
    }
    
    // 이벤트 핸들러
    public void OnItemClicked(ItemSlotUI slot)
    {
        // 구현...
    }
    
    // 드래그 앤 드롭 처리
    public void OnBeginDrag(ItemSlotUI slot)
    {
        // 구현...
    }
    
    public void OnEndDrag(ItemSlotUI slot)
    {
        // 구현...
    }
}
```

#### 장비 화면
- **캐릭터 장비 슬롯**: 착용 장비 표시
- **장비 효과**: 장착된 아이템 속성 및 효과
- **장비 외형 미리보기**: 캐릭터 모델 표시
- **세트 효과**: 세트 장비 시너지 표시

#### 퀵 액세스 바
- **단축키 슬롯**: 자주 사용하는 아이템 및 스킬
- **쿨다운 표시**: 스킬 재사용 대기 시간
- **소비 아이템 수량**: 남은 아이템 수량 표시
- **드래그 앤 드롭 설정**: 사용자 설정 가능

### 메뉴 시스템

#### 일시 정지 메뉴
- **게임 재개/종료**: 기본 제어 기능
- **설정 패널**: 그래픽, 오디오, 컨트롤 설정
- **게임 상태**: 현재 목표 및 진행 정보
- **도움말**: 게임 메커니즘 설명

```csharp
// 일시 정지 메뉴 컨트롤러
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject helpPanel;
    
    // 메뉴 제어
    public void TogglePauseMenu(bool isPaused)
    {
        // 시간 스케일 조정 및 UI 토글
        Time.timeScale = isPaused ? 0f : 1f;
        gameObject.SetActive(isPaused);
    }
    
    // 버튼 이벤트 핸들러
    public void OnResumeClicked()
    {
        TogglePauseMenu(false);
    }
    
    public void OnSettingsClicked()
    {
        settingsPanel.SetActive(true);
    }
    
    public void OnExitGameClicked()
    {
        // 게임 종료 로직...
    }
}
```

#### 결과 화면
- **성공/실패 표시**: 미션 결과 명확히 표시
- **획득 아이템**: 획득한 전리품 목록
- **통계 요약**: 전투 및 탐험 통계
- **다음 행동 옵션**: 계속하기, 로비로 돌아가기 등

## 시각적 피드백 시스템

### 전투 피드백

#### 데미지 표시
- **부동 숫자**: 입/출력 데미지 수치
- **색상 코드**: 데미지 유형 및 치명타 표시
- **애니메이션**: 크기 및 페이드 효과
- **방향성**: 피해 원천 표시

```csharp
// 데미지 텍스트 컨트롤러
public class DamageTextController : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Transform worldSpaceCanvas;
    
    // 데미지 표시 생성
    public void CreateDamageText(Vector3 worldPosition, float damageAmount, 
                                DamageType damageType, bool isCritical)
    {
        // 텍스트 인스턴스 생성
        GameObject textInstance = Instantiate(damageTextPrefab, worldSpaceCanvas);
        textInstance.transform.position = worldPosition;
        
        // 데미지 표시 설정
        DamageText damageText = textInstance.GetComponent<DamageText>();
        damageText.Initialize(damageAmount, damageType, isCritical);
    }
}
```

#### 타격 효과
- **히트 파티클**: 충돌 지점 파티클 효과
- **화면 효과**: 타격 시 화면 가장자리 효과
- **카메라 쉐이크**: 강한 타격 시 화면 흔들림
- **타임 스케일 효과**: 중요 타격 시 일시적 슬로우

#### 체력 변화 피드백
- **체력바 애니메이션**: 부드러운 감소/증가
- **색상 변화**: 위험 상태 시 색상 강조
- **맥동 효과**: 낮은 체력 시 경고 효과
- **화면 비네팅**: 체력 감소 시 시야 제한

### 환경 상호작용 피드백

#### 상호작용 하이라이트
- **아웃라인 효과**: 상호작용 가능 객체 강조
- **아이콘 표시**: 상호작용 유형 표시
- **거리 기반 페이드**: 접근성 시각적 표시
- **키 프롬프트**: 필요한 입력 안내

```csharp
// 상호작용 하이라이트 컨트롤러
public class InteractableHighlightController : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float highlightIntensity = 1.5f;
    
    private Renderer[] objectRenderers;
    private Material[] originalMaterials;
    
    // 하이라이트 활성화
    public void EnableHighlight()
    {
        // 아웃라인 머티리얼 적용...
    }
    
    // 하이라이트 비활성화
    public void DisableHighlight()
    {
        // 원래 머티리얼로 복원...
    }
}
```

#### 상호작용 진행 표시
- **진행 원형 UI**: 긴 상호작용 진행 표시
- **소리 피드백**: 작업 시작/진행/완료 소리
- **취소 안내**: 중단 가능성 표시
- **성공/실패 피드백**: 결과에 따른 명확한 표시

## 사용자 경험 최적화

### 온보딩 시스템

#### 튜토리얼 요소
- **컨텍스트 힌트**: 필요할 때 나타나는 도움말
- **가이드 마커**: 첫 목표 안내 표시
- **인터랙티브 튜토리얼**: 직접 해보며 배우는 방식
- **진행형 학습**: 점진적으로 복잡한 메커니즘 소개

```csharp
// 튜토리얼 시스템
public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<TutorialStep> tutorialSequence;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Text tutorialText;
    
    private int currentStepIndex = 0;
    
    // 튜토리얼 시작
    public void StartTutorial()
    {
        ShowCurrentStep();
    }
    
    // 다음 단계로 진행
    public void AdvanceToNextStep()
    {
        // 구현...
    }
    
    // 튜토리얼 단계 표시
    private void ShowCurrentStep()
    {
        // 구현...
    }
}
```

#### 사용자 지원
- **툴팁 시스템**: 요소에 마우스 오버 시 정보 제공
- **컨트롤 미리보기**: 현재 컨텍스트에서 가능한 행동 표시
- **도움말 메뉴**: 모든 게임 메커니즘 설명
- **오류 알림**: 불가능한 행동 시도 시 안내

### 접근성 기능

#### 시각적 접근성
- **색맹 모드**: 색상 구분이 어려운 사용자 지원
- **텍스트 크기 조절**: 가독성 향상 옵션
- **UI 스케일**: 전체 인터페이스 크기 조절
- **고대비 모드**: 시각적 명확성 향상

#### 조작 접근성
- **키 재맵핑**: 사용자 정의 컨트롤 설정
- **조작 난이도 조절**: 정밀 조작 요구사항 조절
- **자동 지원 기능**: 조준 보조, 움직임 보조 등
- **터치 영역 확장**: 모바일 버전 터치 인식 영역 확대

## 포트폴리오 구현 계획

1개월의 제한된 시간 내에서 다음과 같은 UI/UX 요소를 우선적으로 구현할 계획입니다:

### 1주차: 기본 UI 프레임워크 설정
- UI 관리자 및 기본 구조 구현
- 화면 전환 시스템 구축
- 기본 UI 요소 디자인 및 제작
- UI 에셋 통합 및 스타일 가이드 확립

### 2주차: 핵심 HUD 및 전투 피드백
- 플레이어 상태 HUD 구현
- 미니맵 기본 기능 구현
- 데미지 표시 및 타격 효과 시스템
- 전투 관련 시각적 피드백 구현

### 3주차: 인벤토리 및 상호작용 시스템
- 인벤토리 UI 및 기능 구현
- 아이템 정보 및 툴팁 시스템
- 상호작용 프롬프트 및 하이라이트
- 드래그 앤 드롭 시스템 구현

### 4주차: 메뉴 및 최종 폴리싱
- 일시 정지 및 결과 화면 구현
- 설정 메뉴 및 튜토리얼 요소
- 시각적 일관성 및 스타일 확립
- 최적화 및 버그 수정

## 기술적 도전과 해결책

### 도전 1: 반응형 UI
- **문제**: 다양한 화면 크기 및 해상도 지원
- **해결책**: 앵커 및 레이아웃 그룹 활용, UI 스케일링 시스템 구현

### 도전 2: 성능 최적화
- **문제**: 다수의 UI 요소로 인한 성능 저하
- **해결책**: 오브젝트 풀링, 캔버스 최적화, 드로우 콜 감소 기법

### 도전 3: 네트워크 지연 고려
- **문제**: 네트워크 지연으로 인한 UI 응답성 문제
- **해결책**: 로컬 예측 및 서버 확인 모델, 비동기 UI 업데이트

## 평가 기준

포트폴리오 프로젝트로서 UI/UX 시스템의 성공 여부는 다음 기준으로 평가합니다:

1. **사용성**: 직관적이고 배우기 쉬운 인터페이스
2. **반응성**: 지연 없는 피드백 및 응답
3. **시각적 일관성**: 테마 및 스타일 통일성
4. **정보 전달**: 게임 상태의 명확한 시각화
5. **최적화**: 성능에 미치는 영향 최소화

[메인 계획으로 돌아가기](./MasterPlan.md)
