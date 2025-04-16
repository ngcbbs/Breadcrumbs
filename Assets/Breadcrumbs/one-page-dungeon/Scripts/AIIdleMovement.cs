using UnityEngine;
using System.Collections;

namespace Breadcrumbs.one_page_dungeon.Scripts
{
    public class AIIdleMovement : MonoBehaviour
    {
        public AIIdleMovementSetting setting;
        
        private Vector3 _currentDirection;
        private Vector3 _targetPosition;
        private Vector3 _areaCenter;
        private float _directionChangeTimer;
        
        private Collider[] _nearbyColliders;
        private Vector3[] _directions;
        private float[] _weights;
        
        private void Start()
        {
            // 이동 영역 중심점 설정
            _areaCenter = setting.useTransformAsCenter ? transform.position : setting.areaCenter;
            
            // 방향 배열 초기화
            _directions = new Vector3[]
            {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right,
                (Vector3.forward + Vector3.left).normalized,
                (Vector3.forward + Vector3.right).normalized,
                (Vector3.back + Vector3.left).normalized,
                (Vector3.back + Vector3.right).normalized
            };
            
            _weights = new float[_directions.Length];
            _nearbyColliders = new Collider[16];
            
            // 초기 목표 위치 설정
            SetNewRandomTarget();
            
            // 초기 타이머 설정
            _directionChangeTimer = Random.Range(setting.minDirectionChangeTime, setting.maxDirectionChangeTime);
        }
        
        private void Update()
        {
            // 타이머 감소 및 새 방향 선택
            _directionChangeTimer -= Time.deltaTime;
            if (_directionChangeTimer <= 0f)
            {
                SetNewRandomTarget();
                _directionChangeTimer = Random.Range(setting.minDirectionChangeTime, setting.maxDirectionChangeTime);
            }
            
            // 현재 위치가 영역 반경을 벗어났는지 확인
            float distanceFromCenter = Vector3.Distance(transform.position, _areaCenter);
            if (distanceFromCenter > setting.wanderRadius * 0.9f)
            {
                // 영역 중심을 향해 방향 조정
                Vector3 directionToCenter = (_areaCenter - transform.position).normalized;
                _targetPosition = transform.position + directionToCenter * (setting.wanderRadius * 0.8f);
            }
            
            // 최적의 이동 방향 계산
            Vector3 bestDirection = CalculateBestDirection();
            
            // 부드러운 방향 전환
            _currentDirection = Vector3.Slerp(_currentDirection, bestDirection, Time.deltaTime * setting.turnSpeed);
            
            // 이동 실행
            MoveInDirection(_currentDirection);
        }
        
        private void SetNewRandomTarget()
        {
            // 영역 내에서 랜덤한 목표 위치 설정
            Vector2 randomCircle = Random.insideUnitCircle * setting.wanderRadius;
            Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
            _targetPosition = _areaCenter + randomOffset;
            
            // 디버그 용 메시지
            if (setting.showDebugVisuals)
            {
                Debug.DrawLine(transform.position, _targetPosition, Color.blue, setting.minDirectionChangeTime);
            }
        }
        
        private Vector3 CalculateBestDirection()
        {
            Vector3 directionToTarget = (_targetPosition - transform.position).normalized;
            
            // 각 방향에 가중치 계산
            float bestWeight = float.MinValue;
            Vector3 bestDir = directionToTarget; // 기본값으로 목표 방향 설정
            
            for (int i = 0; i < _directions.Length; i++)
            {
                // 월드 좌표계 기준으로 방향 변환
                Vector3 worldDir = transform.TransformDirection(_directions[i]);
                _weights[i] = EvaluateDirection(worldDir);
                
                if (_weights[i] > bestWeight)
                {
                    bestWeight = _weights[i];
                    bestDir = worldDir;
                }
            }
            
            // 다른 AI와의 분리 벡터 계산
            Vector3 separation = GetSeparationVector();
            
            // 방향, 분리 벡터 결합 (분리에 더 가중치 부여)
            return (bestDir + separation * 0.8f).normalized;
        }
        
        private float EvaluateDirection(Vector3 dir)
        {
            // 목표 지점을 향한 방향과의 일치도 계산
            Vector3 toTarget = (_targetPosition - transform.position).normalized;
            float targetWeight = Vector3.Dot(dir, toTarget);
            
            // 장애물 검사
            if (Physics.Raycast(transform.position, dir, setting.raycastDistance, setting.obstacleLayer))
            {
                return -1f; // 장애물이 있으면 페널티
            }
            
            // 영역 중심과의 거리 고려
            float distanceFromCenter = Vector3.Distance(transform.position, _areaCenter);
            if (distanceFromCenter > setting.wanderRadius * 0.8f)
            {
                // 중심에서 멀어질수록 페널티
                Vector3 toCenter = (_areaCenter - transform.position).normalized;
                float centerAlignment = Vector3.Dot(dir, toCenter);
                targetWeight += centerAlignment * 0.7f; // 중심 방향을 선호하도록 가중치 추가
            }
            
            return targetWeight;
        }
        
        private void MoveInDirection(Vector3 dir)
        {
            // 정규화된 방향으로 이동
            Vector3 movement = dir.normalized * (setting.moveSpeed * Time.deltaTime);
            transform.position += movement;
            
            // 이동 방향으로 회전
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
        
        private Vector3 GetSeparationVector()
        {
            Vector3 separation = Vector3.zero;
            int count = Physics.OverlapSphereNonAlloc(transform.position, setting.separationDistance, _nearbyColliders);
            
            for (int i = 0; i < count; i++)
            {
                Collider col = _nearbyColliders[i];
                
                // 자기 자신 제외, Enemy 태그 있는 객체만 고려
                if (col.gameObject != gameObject && col.CompareTag("Enemy"))
                {
                    Vector3 awayDir = (transform.position - col.transform.position).normalized;
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    
                    // 거리에 반비례하여 분리 강도 증가
                    float strength = 1.0f - Mathf.Clamp01(distance / setting.separationDistance);
                    separation += awayDir * strength;
                }
            }
            
            return separation.normalized;
        }
        
        private void OnDrawGizmos()
        {
            if (!setting || !setting.showDebugVisuals) return;
            
            // 이동 영역 시각화
            Vector3 center = Application.isPlaying ? _areaCenter : 
                (setting.useTransformAsCenter ? transform.position : setting.areaCenter);
            
            // 영역 반경 표시
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(center, setting.wanderRadius);
            
            if (Application.isPlaying && _weights != null)
            {
                // 방향 가중치 시각화
                for (int i = 0; i < _directions.Length; i++)
                {
                    Vector3 worldDir = transform.TransformDirection(_directions[i]);
                    float weight = _weights[i];
                    
                    // 가중치에 따라 색상 결정
                    Gizmos.color = weight > 0 ? Color.green : Color.red;
                    
                    // 가중치 크기에 따른 선 길이
                    float length = Mathf.Clamp(Mathf.Abs(weight), 0, 1) * 2f;
                    Gizmos.DrawLine(transform.position, transform.position + worldDir * length);
                }
                
                // 목표 위치 표시
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_targetPosition, 0.2f);
                Gizmos.DrawLine(transform.position, _targetPosition);
            }
        }
    }
}