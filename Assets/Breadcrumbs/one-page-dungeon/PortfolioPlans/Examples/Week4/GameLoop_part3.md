# 게임 루프 구현 예제 - Part 3

## ObjectiveManager 클래스

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectiveData
{
    public string Id;
    public string Description;
    public int TargetAmount;
    public int CurrentAmount;
    public bool IsCompleted;
    public ObjectiveType Type;
}

public enum ObjectiveType
{
    DefeatEnemies,
    CollectItems,
    ReachDestination,
    SurviveTime,
    DefeatBoss
}

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private List<ObjectiveData> _currentObjectives = new List<ObjectiveData>();
    
    // 이벤트
    public event Action<ObjectiveData> OnObjectiveUpdated;
    public event Action<ObjectiveData> OnObjectiveCompleted;
    public event Action OnAllObjectivesCompleted;
    
    private Dictionary<ObjectiveType, Func<int, ObjectiveData>> _objectiveGenerators;
    
    private void Awake()
    {
        // 목표 생성기 초기화
        InitializeObjectiveGenerators();
    }
    
    private void InitializeObjectiveGenerators()
    {
        _objectiveGenerators = new Dictionary<ObjectiveType, Func<int, ObjectiveData>>
        {
            // 적 처치 목표 생성기
            { ObjectiveType.DefeatEnemies, (level) => new ObjectiveData 
              { 
                  Id = Guid.NewGuid().ToString(),
                  Description = $"적 {5 * level}마리 처치하기",
                  TargetAmount = 5 * level,
                  CurrentAmount = 0,
                  IsCompleted = false,
                  Type = ObjectiveType.DefeatEnemies
              }
            },
            
            // 아이템 수집 목표 생성기
            { ObjectiveType.CollectItems, (level) => new ObjectiveData 
              { 
                  Id = Guid.NewGuid().ToString(),
                  Description = $"보물 {3 * level}개 수집하기",
                  TargetAmount = 3 * level,
                  CurrentAmount = 0,
                  IsCompleted = false,
                  Type = ObjectiveType.CollectItems
              }
            },
            
            // 목적지 도달 목표 생성기
            { ObjectiveType.ReachDestination, (level) => new ObjectiveData 
              { 
                  Id = Guid.NewGuid().ToString(),
                  Description = "던전의 비밀 방에 도달하기",
                  TargetAmount = 1,
                  CurrentAmount = 0,
                  IsCompleted = false,
                  Type = ObjectiveType.ReachDestination
              }
            },
            
            // 시간 생존 목표 생성기
            { ObjectiveType.SurviveTime, (level) => new ObjectiveData 
              { 
                  Id = Guid.NewGuid().ToString(),
                  Description = $"{5 * level}분 동안 생존하기",
                  TargetAmount = 5 * level,
                  CurrentAmount = 0,
                  IsCompleted = false,
                  Type = ObjectiveType.SurviveTime
              }
            },
            
            // 보스 처치 목표 생성기 
            { ObjectiveType.DefeatBoss, (level) => new ObjectiveData 
              { 
                  Id = Guid.NewGuid().ToString(),
                  Description = "던전의 보스 처치하기",
                  TargetAmount = 1,
                  CurrentAmount = 0,
                  IsCompleted = false,
                  Type = ObjectiveType.DefeatBoss
              }
            }
        };
    }
    
    // 레벨에 맞는 목표 설정
    public void SetupObjectives(int level)
    {
        // 기존 목표 초기화
        _currentObjectives.Clear();
        
        // 레벨에 따라 목표 생성
        if (level <= 3)
        {
            // 초반 레벨: 적 처치 + 수집 목표
            _currentObjectives.Add(_objectiveGenerators[ObjectiveType.DefeatEnemies](level));
            _currentObjectives.Add(_objectiveGenerators[ObjectiveType.CollectItems](level));
        }
        else if (level <= 6)
        {
            // 중간 레벨: 적 처치 + 수집 + 목적지 도달
            _currentObjectives.Add(_objectiveGenerators[ObjectiveType.DefeatEnemies](level));
            _currentObjectives.Add(_objectiveGenerators[ObjectiveType.CollectItems](level));
            _currentObjectives.Add(_objectiveGenerators[ObjectiveType.ReachDestination](level));
        }
        else
        {
            // 후반 레벨: 적 처치 + 수집 + 목적지 도달 + 보스 처치
            _currentObjectives.Add(_objectiveGenerators[ObjectiveType.DefeatEnemies](level));
            _currentObjectives.Add(_objectiveGenerators[ObjectiveType.CollectItems](level));
            _currentObjectives.Add(_objectiveGenerators[ObjectiveType.ReachDestination](level));
            _currentObjectives.Add(_objectiveGenerators[ObjectiveType.DefeatBoss](level));
        }
    }
    
    // 목표 진행 상황 업데이트
    public void UpdateObjectiveProgress(ObjectiveType type, int amount = 1)
    {
        foreach (var objective in _currentObjectives)
        {
            if (objective.Type == type && !objective.IsCompleted)
            {
                objective.CurrentAmount += amount;
                
                // 목표 완료 여부 확인
                if (objective.CurrentAmount >= objective.TargetAmount)
                {
                    objective.IsCompleted = true;
                    objective.CurrentAmount = objective.TargetAmount; // 최대값 제한
                    OnObjectiveCompleted?.Invoke(objective);
                }
                else
                {
                    OnObjectiveUpdated?.Invoke(objective);
                }
            }
        }
        
        // 모든 목표 완료 확인
        CheckAllObjectivesCompleted();
    }
    
    // 특정 목표 업데이트
    public void UpdateObjective(string objectiveId, int amount = 1)
    {
        var objective = _currentObjectives.Find(o => o.Id == objectiveId);
        if (objective != null && !objective.IsCompleted)
        {
            objective.CurrentAmount += amount;
            
            // 목표 완료 여부 확인
            if (objective.CurrentAmount >= objective.TargetAmount)
            {
                objective.IsCompleted = true;
                objective.CurrentAmount = objective.TargetAmount; // 최대값 제한
                OnObjectiveCompleted?.Invoke(objective);
            }
            else
            {
                OnObjectiveUpdated?.Invoke(objective);
            }
            
            // 모든 목표 완료 확인
            CheckAllObjectivesCompleted();
        }
    }
    
    // 모든 목표 완료 여부 확인
    private void CheckAllObjectivesCompleted()
    {
        if (AreAllObjectivesCompleted())
        {
            OnAllObjectivesCompleted?.Invoke();
            GameManager.Instance.CheckObjectiveCompletion();
        }
    }
    
    // 모든 목표가 완료되었는지 확인
    public bool AreAllObjectivesCompleted()
    {
        if (_currentObjectives.Count == 0)
            return false;
            
        foreach (var objective in _currentObjectives)
        {
            if (!objective.IsCompleted)
                return false;
        }
        
        return true;
    }
    
    // 현재 목표 목록 가져오기
    public List<ObjectiveData> GetCurrentObjectives()
    {
        return _currentObjectives;
    }
}
```
