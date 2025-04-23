using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Breadcrumbs.Character.Services;
using Breadcrumbs.DependencyInjection;

namespace Breadcrumbs.Character.Skills.Examples
{
    /// <summary>
    /// 스킬 시스템을 테스트하기 위한 예제 클래스
    /// </summary>
    public class SkillExecutionExample : MonoBehaviour
    {
        [Header("캐릭터 참조")]
        [SerializeField] private Character playerCharacter;
        
        [Header("UI 요소")]
        [SerializeField] private Button[] skillButtons;
        [SerializeField] private Image[] cooldownImages;
        [SerializeField] private Text[] skillNameTexts;
        
        // 의존성 주입
        [Inject] private ISkillService skillService;
        
        // 캐릭터가 가진 스킬들
        private string[] skillIds = new string[4];
        
        private void Start()
        {
            if (playerCharacter == null)
            {
                playerCharacter = GetComponent<Character>();
                
                if (playerCharacter == null)
                {
                    Debug.LogError("Character 컴포넌트를 찾을 수 없습니다.");
                    return;
                }
            }
            
            // 의존성 주입 확인
            if (skillService == null)
            {
                Debug.LogError("ISkillService가 주입되지 않았습니다.");
                return;
            }
            
            Debug.Log("fixme!!");
            /*
            // 캐릭터의 스킬 초기화
            skillService.InitializeCharacter(playerCharacter);
            // */
            
            // 테스트용 스킬 배우기
            if (playerCharacter.ClassType == ClassType.Warrior)
            {
                skillIds[0] = "skill_warrior_slash";
                skillIds[1] = "skill_warrior_block";
                skillIds[2] = "skill_warrior_whirlwind";
                skillIds[3] = "skill_warrior_berserker_rage";
                
                for (int i = 0; i < skillIds.Length; i++)
                {
                    if (!string.IsNullOrEmpty(skillIds[i]))
                    {
                        skillService.LearnSkill(playerCharacter, skillIds[i]);
                        
                        // UI 업데이트
                        UpdateSkillUI(i);
                    }
                }
            }
            
            // 버튼에 이벤트 연결
            for (int i = 0; i < skillButtons.Length; i++)
            {
                if (skillButtons[i] != null)
                {
                    int index = i; // 클로저를 위한 복사
                    skillButtons[i].onClick.AddListener(() => UseSkill(index));
                }
            }
        }
        
        private void Update()
        {
            // 스킬 쿨다운 UI 업데이트
            UpdateCooldownUI();
            
            // 키보드 입력으로 스킬 사용
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseSkill(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) UseSkill(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) UseSkill(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) UseSkill(3);
        }
        
        /// <summary>
        /// 특정 슬롯의 스킬을 사용합니다.
        /// </summary>
        private void UseSkill(int index)
        {
            if (index < 0 || index >= skillIds.Length || string.IsNullOrEmpty(skillIds[index]))
                return;
            
            // 커서 위치의 적을 대상으로 스킬 사용
            Transform target = GetTargetUnderCursor();
            
            // 스킬 사용
            bool success = skillService.UseSkill(playerCharacter, skillIds[index], target);
            
            if (success)
            {
                Debug.Log($"스킬 사용 성공: {skillIds[index]}");
            }
            else
            {
                Debug.LogWarning($"스킬 사용 실패: {skillIds[index]}");
            }
        }
        
        /// <summary>
        /// 커서 아래의 대상을 찾습니다.
        /// </summary>
        private Transform GetTargetUnderCursor()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit))
            {
                return hit.transform;
            }
            
            return null;
        }
        
        /// <summary>
        /// 특정 슬롯의 스킬 UI를 업데이트합니다.
        /// </summary>
        private void UpdateSkillUI(int index)
        {
            if (index < 0 || index >= skillIds.Length || string.IsNullOrEmpty(skillIds[index]))
                return;
            
            if (skillNameTexts[index] != null)
            {
                // 스킬 정보 가져오기
                var skillInfo = skillService.GetAllSkills(playerCharacter)
                    .FirstOrDefault(s => s.SkillId == skillIds[index]);
                
                if (skillInfo != null)
                {
                    // 이름 표시
                    skillNameTexts[index].text = skillInfo.Name;
                }
                else
                {
                    skillNameTexts[index].text = "Unknown";
                }
            }
        }
        
        /// <summary>
        /// 쿨다운 UI를 업데이트합니다.
        /// </summary>
        private void UpdateCooldownUI()
        {
            for (int i = 0; i < skillIds.Length; i++)
            {
                if (string.IsNullOrEmpty(skillIds[i]) || cooldownImages[i] == null)
                    continue;
                
                // 실제 게임에서는 Skill 객체에서 직접 정보를 가져와야 함
                // 여기서는 예시로 간단히 구현
                float remainingCooldown = 0f; // 실제로는 스킬의 RemainingCooldown 값
                float maxCooldown = 1f; // 실제로는 스킬의 총 쿨다운 값
                
                if (remainingCooldown > 0f)
                {
                    cooldownImages[i].fillAmount = remainingCooldown / maxCooldown;
                    cooldownImages[i].gameObject.SetActive(true);
                }
                else
                {
                    cooldownImages[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
