using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Breadcrumbs.Day13
{
    // 시도#1 (흠.. code 기반으로 찾아야 하는군.. 흠)
    public class SensorComponent : MonoBehaviour
    {
        // 디버그 시각화 설정
        [Header("디버그 설정")]
        public bool enableDebugVisualization = true;
        public Color detectionAreaColor = Color.yellow;
        public float debugLineDuration = 0.1f;

        // 센서 기본 매개변수
        [Header("센서 파라미터")]
        public float detectionRadius = 10f;
        public float detectionAngle = 90f;
        public Vector3 detectionBoxSize = Vector3.one;

        // 필터링 조건
        private readonly List<Func<Unit, bool>> _filters = new List<Func<Unit, bool>>();

        // 감지된 게임 오브젝트들
        private List<Unit> _detectedUnits = new List<Unit>();

        /// <summary>
        /// 감지 파이프라인을 초기화합니다.
        /// </summary>
        public SensorComponent Init()
        {
            _detectedUnits.Clear();
            _filters.Clear();
            return this;
        }

        /// <summary>
        /// 거리 기반 필터 추가
        /// </summary>
        public SensorComponent WithinDistance(float radius)
        {
            _filters.Add(obj => Vector3.Distance(transform.position, obj.transform.position) <= radius);
            return this;
        }

        /// <summary>
        /// 방향 기반 필터 추가
        /// </summary>
        public SensorComponent WithinDirection(float angle, float viewAngle)
        {
            _filters.Add(obj =>
            {
                Vector3 directionToObject = (obj.transform.position - transform.position).normalized;
                float angleToObject = Vector3.Angle(transform.forward, directionToObject);
                return angleToObject <= viewAngle / 2f;
            });
            return this;
        }

        /// <summary>
        /// 박스 영역 기반 필터 추가
        /// </summary>
        public SensorComponent WithinBox(Vector3 boxSize)
        {
            _filters.Add(obj =>
            {
                Bounds bounds = new Bounds(transform.position, boxSize);
                return bounds.Contains(obj.transform.position);
            });
            return this;
        }

        /// <summary>
        /// 레이어 마스크 필터 추가
        /// </summary>
        public SensorComponent OnLayer(LayerMask layerMask)
        {
            _filters.Add(obj => layerMask == (layerMask | (1 << obj.gameObject.layer)));
            return this;
        }

        /// <summary>
        /// 태그 기반 필터 추가
        /// </summary>
        public SensorComponent WithTag(string tag)
        {
            _filters.Add(obj => obj.CompareTag(tag));
            return this;
        }

        /// <summary>
        /// 장애물 감지 필터 추가
        /// </summary>
        public SensorComponent NoObstacles()
        {
            _filters.Add(obj =>
            {
                RaycastHit hit;
                return !Physics.Raycast(transform.position, obj.transform.position - transform.position, out hit);
            });
            return this;
        }

        /// <summary>
        /// 속도 범위 필터 추가
        /// </summary>
        public SensorComponent WithVelocity(float minSpeed, float maxSpeed)
        {
            _filters.Add(obj =>
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float speed = rb.linearVelocity.magnitude;
                    return speed >= minSpeed && speed <= maxSpeed;
                }
                return false;
            });
            return this;
        }

        /// <summary>
        /// 감지 수행
        /// </summary>
        public SensorComponent Detect()
        {
            // 모든 Unit 오브젝트에 대해 필터링 수행
            _detectedUnits = FindObjectsByType<Unit>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.InstanceID
            ).ToList();

            if (_detectedUnits == null || _detectedUnits.Count == 0) {
                Debug.Log("can't find Units :P");
                return this;
            }
            
            Debug.Log($"detectedUnits Count = {_detectedUnits.Count}");

            _detectedUnits
                .Where(obj => _filters.All(filter => filter(obj))).ToArray();

            // 디버그 시각화
            if (enableDebugVisualization)
            {
                VisualizeDetection();
            }

            return this;
        }

        /// <summary>
        /// 가장 가까운 대상 선택
        /// </summary>
        public Unit SelectClosest()
        {
            return _detectedUnits
                .OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position))
                .FirstOrDefault();
        }

        /// <summary>
        /// 모든 감지된 대상 선택
        /// </summary>
        public List<Unit> SelectMany()
        {
            return _detectedUnits;
        }

        /// <summary>
        /// 감지된 대상들의 위치 선택
        /// </summary>
        public List<Vector3> SelectManyPositions()
        {
            return _detectedUnits.Select(obj => obj.transform.position).ToList();
        }

        /// <summary>
        /// 커스텀 조건으로 대상 선택
        /// </summary>
        public List<Unit> SelectWithCondition(Func<Unit, bool> condition)
        {
            return _detectedUnits.Where(condition).ToList();
        }

        /// <summary>
        /// 감지 결과 시각화
        /// </summary>
        private void VisualizeDetection()
        {
            // 감지 영역 그리기
            Debug.DrawLine(transform.position, transform.position + transform.forward * detectionRadius, detectionAreaColor, debugLineDuration);

            // 감지된 오브젝트 하이라이트
            foreach (var obj in _detectedUnits)
            {
                Debug.DrawLine(transform.position, obj.transform.position, Color.red, debugLineDuration);
            }
        }

        /// <summary>
        /// 감지 결과 이벤트
        /// </summary>
        public event Action<List<Unit>> OnObjectsDetected;

        /// <summary>
        /// 감지 결과 이벤트 트리거
        /// </summary>
        private void TriggerDetectionEvent()
        {
            OnObjectsDetected?.Invoke(_detectedUnits);
        }
    }
}