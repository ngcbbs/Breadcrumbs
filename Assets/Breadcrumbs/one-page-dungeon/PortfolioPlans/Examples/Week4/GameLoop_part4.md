# 게임 루프 구현 예제 - Part 4

## GameLoopController 클래스

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameLoopSettings
{
    public float LevelIntroductionTime = 3f;
    public float LevelCompletionTime = 5f;
    public float GameOverDelayTime = 3f;
    public float NextLevelTransitionTime = 2f;
    public int MaxLevels = 10;
}

public enum GameLoopState
{
    MainMenu,
    LevelIntroduction,
    Gameplay,
    LevelCompletion,
    GameOver,
    Victory,
    Paused
}

public class GameLoopController : MonoBehaviour
{
    [SerializeField] private GameLoopSettings _settings;
    
    // 현재 게임 상태
    private GameLoopState _currentState = GameLoopState.MainMenu;
    private GameLoopState _previousState;
    
    // 현재 레벨 정보
    private int _currentLevel = 1;
    private float _levelStartTime;
    private float _levelTime;
    
    // 이벤트
    public event Action<GameLoopState> OnGameStateChanged;
    public event Action<int> OnLevelStarted;
    public event Action<int, float> OnLevelCompleted;
    public event Action OnGameOver;
    public event Action OnGameVictory;
    
    // 싱글톤 인스턴스
    public static GameLoopController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 기본 설정값 초기화
        if (_settings == null)
        {
            _settings = new GameLoopSettings();
        }
    }
    
    private void Start()
    {
        // 초기 상태 설정
        ChangeState(GameLoopState.MainMenu);
    }
    
    private void Update()
    {
        // 현재 상태에 따른 업데이트 처리
        switch (_currentState)
        {
            case GameLoopState.Gameplay:
                UpdateGameplay();
                break;
                
            case GameLoopState.LevelIntroduction:
                UpdateLevelIntroduction();
                break;
                
            case GameLoopState.LevelCompletion:
                UpdateLevelCompletion();
                break;
                
            // 기타 상태는 별도 업데이트 로직 필요 없음
        }
        
        // 일시정지 처리
        if (Input.GetKeyDown(KeyCode.Escape) && 
            _currentState != GameLoopState.MainMenu && 
            _currentState != GameLoopState.GameOver &&
            _currentState != GameLoopState.Victory)
        {
            TogglePause();
        }
    }
    
    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        _currentLevel = 1;
        StartLevel(_currentLevel);
    }
    
    /// <summary>
    /// 레벨 시작
    /// </summary>
    public void StartLevel(int level)
    {
        _currentLevel = level;
        _levelStartTime = Time.time;
        _levelTime = 0f;
        
        // 레벨 소개 상태로 전환
        ChangeState(GameLoopState.LevelIntroduction);
        
        // 레벨 매니저에 레벨 설정 요청
        LevelManager.Instance.SetupLevel(_currentLevel);
        
        // 목표 매니저에 레벨 목표 설정 요청
        FindObjectOfType<ObjectiveManager>()?.SetupObjectives(_currentLevel);
        
        Debug.Log($"Starting level {_currentLevel}");
    }
    
    /// <summary>
    /// 레벨 소개 업데이트
    /// </summary>
    private void UpdateLevelIntroduction()
    {
        if (Time.time >= _levelStartTime + _settings.LevelIntroductionTime)
        {
            // 소개 시간이 지나면 실제 게임플레이 상태로 전환
            ChangeState(GameLoopState.Gameplay);
            
            // 레벨 시작 이벤트 발생
            OnLevelStarted?.Invoke(_currentLevel);
        }
    }
    
    /// <summary>
    /// 게임플레이 업데이트
    /// </summary>
    private void UpdateGameplay()
    {
        // 게임 진행 시간 업데이트
        _levelTime = Time.time - _levelStartTime;
        
        // 필요한 경우 추가 게임플레이 검사 로직
    }
    
    /// <summary>
    /// 레벨 완료 시 호출
    /// </summary>
    public void CompletedLevel()
    {
        if (_currentState != GameLoopState.Gameplay)
            return;
            
        // 레벨 완료 상태로 전환
        ChangeState(GameLoopState.LevelCompletion);
        
        // 레벨 완료 이벤트 발생
        OnLevelCompleted?.Invoke(_currentLevel, _levelTime);
        
        // 일정 시간 후 다음 레벨로 진행
        StartCoroutine(TransitionToNextLevel());
    }
    
    /// <summary>
    /// 레벨 완료 업데이트
    /// </summary>
    private void UpdateLevelCompletion()
    {
        // 필요한 경우 추가 레벨 완료 로직
    }
    
    /// <summary>
    /// 다음 레벨로 전환
    /// </summary>
    private IEnumerator TransitionToNextLevel()
    {
        yield return new WaitForSeconds(_settings.LevelCompletionTime);
        
        _currentLevel++;
        
        // 모든 레벨 완료 여부 확인
        if (_currentLevel > _settings.MaxLevels)
        {
            // 게임 승리
            ChangeState(GameLoopState.Victory);
            OnGameVictory?.Invoke();
        }
        else
        {
            // 다음 레벨 시작
            StartLevel(_currentLevel);
        }
    }
    
    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    public void GameOver()
    {
        if (_currentState == GameLoopState.GameOver || 
            _currentState == GameLoopState.Victory)
            return;
            
        StartCoroutine(GameOverSequence());
    }
    
    /// <summary>
    /// 게임 오버 시퀀스
    /// </summary>
    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(_settings.GameOverDelayTime);
        
        ChangeState(GameLoopState.GameOver);
        OnGameOver?.Invoke();
        
        Debug.Log("Game Over");
    }
    
    /// <summary>
    /// 일시정지 토글
    /// </summary>
    public void TogglePause()
    {
        if (_currentState == GameLoopState.Paused)
        {
            // 일시정지 해제
            Time.timeScale = 1f;
            ChangeState(_previousState);
        }
        else
        {
            // 일시정지
            _previousState = _currentState;
            Time.timeScale = 0f;
            ChangeState(GameLoopState.Paused);
        }
    }
    
    /// <summary>
    /// 게임 상태 변경
    /// </summary>
    private void ChangeState(GameLoopState newState)
    {
        _currentState = newState;
        OnGameStateChanged?.Invoke(newState);
        
        Debug.Log($"Game state changed to: {newState}");
    }
    
    /// <summary>
    /// 메인 메뉴로 돌아가기
    /// </summary>
    public void ReturnToMainMenu()
    {
        // 시간 스케일 복원
        Time.timeScale = 1f;
        
        // 상태 변경
        ChangeState(GameLoopState.MainMenu);
        
        // 씬 로드 또는 기타 초기화 로직
        // SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// 현재 게임 상태 가져오기
    /// </summary>
    public GameLoopState GetCurrentState()
    {
        return _currentState;
    }
    
    /// <summary>
    /// 현재 레벨 정보 가져오기
    /// </summary>
    public int GetCurrentLevel()
    {
        return _currentLevel;
    }
    
    /// <summary>
    /// 현재 레벨 진행 시간 가져오기
    /// </summary>
    public float GetLevelTime()
    {
        return _levelTime;
    }
}
```
