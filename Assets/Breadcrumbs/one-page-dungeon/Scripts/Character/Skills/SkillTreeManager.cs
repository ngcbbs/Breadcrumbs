using System.Collections.Generic;
using Breadcrumbs.Character.Services;
using Breadcrumbs.DependencyInjection;
using Breadcrumbs.Singletons;
using UnityEngine;

namespace Breadcrumbs.Character.Skills
{
    /// <summary>
    /// 스킬 트리 UI 연동 및 관리 클래스
    /// </summary>
    public class SkillTreeManager : Singleton<SkillTreeManager>
    {
        [Header("설정")]
        [SerializeField] private GameObject skillNodePrefab;
        [SerializeField] private Transform skillTreeContainer;
        
        // 의존성 주입
        [Inject] private ISkillService skillService;
        
        // 현재 활성화된 스킬 트리 및 캐릭터
        private SkillTree currentTree;
        private ICharacter currentCharacter;
        
        // UI 노드 매핑
        private Dictionary<int, GameObject> nodeObjects = new Dictionary<int, GameObject>();
        
        protected override void Awake()
        {
            base.Awake();
            
            if (skillTreeContainer == null)
            {
                skillTreeContainer = transform;
            }
        }
        
        private void Start()
        {
            // UI 초기화 등
        }
        
        /// <summary>
        /// 특정 캐릭터와 스킬 트리 ID에 대한 스킬 트리를 표시합니다.
        /// </summary>
        public void ShowSkillTree(ICharacter character, string treeId)
        {
            if (character == null || string.IsNullOrEmpty(treeId))
                return;
            
            // 이전 트리 정리
            ClearSkillTree();
            
            currentCharacter = character;
            
            // 실제 게임에서는 스킬 서비스에서 트리를 가져옴
            // 현재는 테스트를 위해 간소화된 로직 사용
            SkillTree tree = Resources.Load<SkillTree>("SkillTrees/" + treeId);
            if (tree == null)
            {
                Debug.LogError($"스킬 트리를 찾을 수 없습니다: {treeId}");
                return;
            }
            
            currentTree = tree;
            BuildSkillTree();
        }
        
        /// <summary>
        /// 스킬 트리 UI를 구성합니다.
        /// </summary>
        private void BuildSkillTree()
        {
            if (currentTree == null || currentCharacter == null || skillNodePrefab == null)
                return;
            
            // 스킬 포인트 표시 업데이트
            UpdateSkillPointsDisplay();
            
            // 스킬 트리 노드 생성
            foreach (var node in currentTree.Nodes)
            {
                GameObject nodeObj = Instantiate(skillNodePrefab, skillTreeContainer);
                nodeObj.name = $"Node_{node.NodeId}";
                
                // 위치 설정
                RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = node.Position;
                }
                
                // 노드 컴포넌트 설정
                SkillTreeNodeUI nodeUI = nodeObj.GetComponent<SkillTreeNodeUI>();
                if (nodeUI != null)
                {
                    nodeUI.Initialize(node, currentCharacter);
                    
                    // 클릭 이벤트 연결
                    nodeUI.OnNodeClicked += HandleNodeClick;
                }
                
                // 노드 참조 저장
                nodeObjects[node.NodeId] = nodeObj;
            }
            
            // 노드 간 연결선 생성
            DrawNodeConnections();
            
            // 노드 상태 업데이트
            UpdateNodeStates();
        }
        
        /// <summary>
        /// 노드 간 연결선을 그립니다.
        /// </summary>
        private void DrawNodeConnections()
        {
            if (currentTree == null)
                return;
            
            foreach (var node in currentTree.Nodes)
            {
                if (node.Prerequisites.Count == 0)
                    continue;
                
                if (!nodeObjects.TryGetValue(node.NodeId, out GameObject nodeObj))
                    continue;
                
                foreach (int prerequisiteId in node.Prerequisites)
                {
                    if (!nodeObjects.TryGetValue(prerequisiteId, out GameObject prereqObj))
                        continue;
                    
                    // 두 노드 사이에 선을 그리는 UI 요소 생성
                    CreateConnectionLine(prereqObj.GetComponent<RectTransform>(), nodeObj.GetComponent<RectTransform>());
                }
            }
        }
        
        /// <summary>
        /// 두 노드 사이의 연결선을 생성합니다.
        /// </summary>
        private void CreateConnectionLine(RectTransform from, RectTransform to)
        {
            if (from == null || to == null)
                return;
            
            // 연결선 게임 오브젝트 생성
            GameObject lineObj = new GameObject("Connection");
            lineObj.transform.SetParent(skillTreeContainer);
            
            // RectTransform 설정
            RectTransform lineRect = lineObj.AddComponent<RectTransform>();
            lineRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Line Renderer 또는 UI 이미지로 선 그리기
            // 여기서는 UI 이미지를 사용한 간단한 방법으로 구현
            UnityEngine.UI.Image lineImage = lineObj.AddComponent<UnityEngine.UI.Image>();
            lineImage.color = Color.gray;
            
            // 두 노드 위치 계산
            Vector2 fromPos = from.anchoredPosition;
            Vector2 toPos = to.anchoredPosition;
            
            // 선의 길이와 각도 계산
            Vector2 direction = toPos - fromPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // 선의 위치와 크기 설정
            lineRect.anchoredPosition = fromPos + direction * 0.5f;
            lineRect.sizeDelta = new Vector2(distance, 2f); // 선의 두께는 2픽셀
            lineRect.localRotation = Quaternion.Euler(0, 0, angle);
        }
        
        /// <summary>
        /// 모든 노드의 상태를 업데이트합니다.
        /// </summary>
        private void UpdateNodeStates()
        {
            if (currentTree == null || currentCharacter == null)
                return;
            
            // 실제 게임에서는 스킬 서비스에서 잠금 해제 상태를 가져옴
            // 여기서는 테스트를 위해 간소화된 로직 사용
            Dictionary<int, bool> unlockedNodes = new Dictionary<int, bool>();
            int availablePoints = skillService != null ? 
                skillService.GetAvailableSkillPoints(currentCharacter) : 0;
            
            foreach (var entry in nodeObjects)
            {
                int nodeId = entry.Key;
                GameObject nodeObj = entry.Value;
                
                SkillTreeNodeUI nodeUI = nodeObj.GetComponent<SkillTreeNodeUI>();
                if (nodeUI != null)
                {
                    // 노드 상태 업데이트
                    bool isUnlocked = unlockedNodes.TryGetValue(nodeId, out bool unlocked) && unlocked;
                    bool canUnlock = !isUnlocked && currentTree.CanUnlockNode(nodeId, unlockedNodes, availablePoints);
                    
                    nodeUI.UpdateState(isUnlocked, canUnlock);
                }
            }
        }
        
        /// <summary>
        /// 스킬 포인트 표시를 업데이트합니다.
        /// </summary>
        private void UpdateSkillPointsDisplay()
        {
            if (currentCharacter == null || skillService == null)
                return;
            
            int availablePoints = skillService.GetAvailableSkillPoints(currentCharacter);
            
            // 실제 게임에서는 UI 요소로 표시
            Debug.Log($"사용 가능한 스킬 포인트: {availablePoints}");
        }
        
        /// <summary>
        /// 노드 클릭 이벤트를 처리합니다.
        /// </summary>
        private void HandleNodeClick(SkillTreeNode node)
        {
            if (node == null || currentCharacter == null || skillService == null)
                return;
            
            // 노드 잠금 해제 시도
            if (currentTree != null)
            {
                bool success = skillService.UnlockSkillTreeNode(currentCharacter, currentTree.TreeId, node.NodeId);
                
                if (success)
                {
                    // 노드 및 포인트 표시 업데이트
                    UpdateNodeStates();
                    UpdateSkillPointsDisplay();
                }
            }
        }
        
        /// <summary>
        /// 스킬 트리 UI를 정리합니다.
        /// </summary>
        private void ClearSkillTree()
        {
            // 모든 노드 오브젝트 제거
            foreach (var nodeObj in nodeObjects.Values)
            {
                if (nodeObj != null)
                {
                    Destroy(nodeObj);
                }
            }
            
            nodeObjects.Clear();
            currentTree = null;
            
            // 추가 정리 작업
            // ...
        }
        
        /// <summary>
        /// 현재 트리에서 특정 노드의 정보를 반환합니다.
        /// </summary>
        public SkillTreeNode GetNode(int nodeId)
        {
            if (currentTree == null)
                return null;
            
            return currentTree.GetNode(nodeId);
        }
    }
    
    /// <summary>
    /// 스킬 트리 노드 UI 클래스 (실제 구현은 별도 파일에 작성)
    /// </summary>
    public class SkillTreeNodeUI : MonoBehaviour
    {
        // 노드 클릭 이벤트 델리게이트
        public System.Action<SkillTreeNode> OnNodeClicked;
        
        // 노드 데이터
        private SkillTreeNode nodeData;
        
        // 캐릭터 참조
        private ICharacter character;
        
        // UI 컴포넌트
        private UnityEngine.UI.Image iconImage;
        private UnityEngine.UI.Button button;
        
        /// <summary>
        /// 노드 UI를 초기화합니다.
        /// </summary>
        public void Initialize(SkillTreeNode node, ICharacter character)
        {
            this.nodeData = node;
            this.character = character;
            
            // UI 컴포넌트 가져오기
            button = GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
            
            iconImage = transform.Find("Icon")?.GetComponent<UnityEngine.UI.Image>();
            
            // 스킬 아이콘 설정
            if (iconImage != null && node.SkillDefinition != null)
            {
                iconImage.sprite = node.SkillDefinition.Icon;
            }
        }
        
        /// <summary>
        /// 노드 상태를 업데이트합니다.
        /// </summary>
        public void UpdateState(bool isUnlocked, bool canUnlock)
        {
            // 버튼 상호작용 상태 설정
            if (button != null)
            {
                button.interactable = canUnlock;
            }
            
            // 시각적 상태 업데이트
            if (iconImage != null)
            {
                if (isUnlocked)
                {
                    // 잠금 해제된 노드 스타일
                    iconImage.color = Color.white;
                }
                else if (canUnlock)
                {
                    // 잠금 해제 가능한 노드 스타일
                    iconImage.color = new Color(0.8f, 0.8f, 1f);
                }
                else
                {
                    // 잠긴 노드 스타일
                    iconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                }
            }
        }
        
        /// <summary>
        /// 버튼 클릭 이벤트 처리
        /// </summary>
        private void OnClick()
        {
            OnNodeClicked?.Invoke(nodeData);
        }
    }
}
