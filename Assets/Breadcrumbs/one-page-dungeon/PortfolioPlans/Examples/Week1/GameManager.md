# GameManager 클래스 예제

기본적인 싱글톤 패턴을 적용한 게임 관리자 클래스 구현 예시입니다.

```csharp
// 핵심 시스템 매니저 예시
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;
    
    [Header("Core Systems")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private UIManager uiManager;
    
    // 게임 상태 관리
    public GameState CurrentState { get; private set; }
    
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 시스템 초기화
        Initialize();
    }
    
    private void Initialize()
    {
        // 시스템 초기화 로직...
    }
    
    // 게임 상태 열거형
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver
    }
    
    // 게임 상태 변경 메서드
    public void ChangeState(GameState newState)
    {
        // 상태 전환 로직...
    }
}
```

## 구현 세부 사항

### 싱글톤 패턴 구현

게임 매니저는 전체 게임에서 단일 진입점과 중앙 제어 역할을 하므로 싱글톤 패턴을 적용하여 다른 클래스에서 쉽게 접근할 수 있도록 합니다.

```csharp
private static GameManager instance;
public static GameManager Instance => instance;

private void Awake()
{
    if (instance != null && instance != this)
    {
        Destroy(gameObject);
        return;
    }
    instance = this;
    DontDestroyOnLoad(gameObject);
}
```

### 시스템 참조 관리

게임의 주요 서브시스템에 대한 참조를 관리하여 중앙 집중식 접근을 제공합니다.

```csharp
[Header("Core Systems")]
[SerializeField] private DungeonManager dungeonManager;
[SerializeField] private PlayerManager playerManager;
[SerializeField] private UIManager uiManager;
```

### 게임 상태 관리

게임의 다양한 상태(메인 메뉴, 로딩, 플레이 등)를 관리하고 전환하는 로직을 포함합니다.

```csharp
public enum GameState
{
    MainMenu,
    Loading,
    Playing,
    Paused,
    GameOver
}

public GameState CurrentState { get; private set; }

public void ChangeState(GameState newState)
{
    // 이전 상태 종료 로직
    switch (CurrentState)
    {
        case GameState.MainMenu:
            // 메인 메뉴 종료 로직
            break;
        case GameState.Playing:
            // 게임 플레이 종료 로직
            break;
        // 기타 상태 처리...
    }
    
    // 새 상태 저장
    CurrentState = newState;
    
    // 새 상태 시작 로직
    switch (CurrentState)
    {
        case GameState.MainMenu:
            // 메인 메뉴 시작 로직
            break;
        case GameState.Loading:
            // 로딩 시작 로직
            StartCoroutine(LoadGameRoutine());
            break;
        case GameState.Playing:
            // 게임 플레이 시작 로직
            break;
        // 기타 상태 처리...
    }
    
    // 상태 변경 이벤트 발생
    OnGameStateChanged?.Invoke(CurrentState);
}

// 게임 상태 변경 이벤트
public event Action<GameState> OnGameStateChanged;

private IEnumerator LoadGameRoutine()
{
    // 게임 로딩 로직
    yield return StartCoroutine(dungeonManager.GenerateDungeonRoutine());
    yield return StartCoroutine(playerManager.SpawnPlayersRoutine());
    
    // 로딩 완료 후 플레이 상태로 전환
    ChangeState(GameState.Playing);
}
```

### 초기화 및 설정

게임 시작 시 필요한 초기화 작업을 수행합니다.

```csharp
private void Initialize()
{
    // 기본 데이터 로드
    LoadGameSettings();
    
    // 서브시스템 초기화
    dungeonManager.Initialize();
    playerManager.Initialize();
    uiManager.Initialize();
    
    // 초기 상태 설정
    ChangeState(GameState.MainMenu);
}

private void LoadGameSettings()
{
    // 게임 설정 로드 로직
    // PlayerPrefs 또는 ScriptableObject 등에서 설정 로드
}
```