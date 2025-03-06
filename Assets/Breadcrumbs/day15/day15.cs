using Breadcrumbs.day15;
using UnityEngine;

public class day15 : MonoBehaviour {
    public int gridWidth = 50;
    public int gridHeight = 50;
    public float cellSize = 1f;
    public Vector2 gridOrigin = Vector2.zero;
    public Transform target;
    public GameObject agentPrefab;
    public int agentCount = 20;

    private FlowField flowField;
    private bool[,] obstacles;

    private void Start() {
        // Flow field 생성
        flowField = new FlowField(gridWidth, gridHeight, cellSize, gridOrigin);

        // 장애물 배열 초기화 (예: 랜덤 장애물)
        obstacles = new bool[gridWidth, gridHeight];
        GenerateRandomObstacles(0.1f); // 10% 확률로 장애물 생성

        // 타겟 위치를 향한 flow field 생성
        UpdateFlowField();

        // 에이전트 생성
        SpawnAgents();
    }

    private void Update() {
        // 타겟이 움직이면 flow field 업데이트
        if (target != null && Vector2.Distance(target.position, lastTargetPosition) > 0.5f) {
            UpdateFlowField();
        }

        // 디버그 용도로 flow field 시각화
        flowField.DebugDrawFlowField();
    }

    private Vector2 lastTargetPosition;

    private void UpdateFlowField() {
        if (target != null) {
            Vector2 targetPosition = new Vector2(target.position.x, target.position.y);
            flowField.GenerateFlowFieldWithObstacles(targetPosition, obstacles);
            lastTargetPosition = targetPosition;
        }
    }

    private void GenerateRandomObstacles(float probability) {
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                obstacles[x, y] = UnityEngine.Random.value < probability;
            }
        }
    }

    private void SpawnAgents() {
        for (int i = 0; i < agentCount; i++) {
            // 랜덤 위치에 에이전트 생성 (장애물이 아닌 위치)
            Vector2 position;
            int x, y;
            do {
                x = UnityEngine.Random.Range(0, gridWidth);
                y = UnityEngine.Random.Range(0, gridHeight);
                position = new Vector2(gridOrigin.x + x * cellSize + cellSize / 2, gridOrigin.y + y * cellSize + cellSize / 2);
            } while (obstacles[x, y]);

            // 에이전트 생성 및 설정
            GameObject agent = Instantiate(agentPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
            FlowFieldAgent agentComponent = agent.GetComponent<FlowFieldAgent>();
            if (agentComponent != null) {
                agentComponent.SetFlowField(flowField);
            }
        }
    }
}
