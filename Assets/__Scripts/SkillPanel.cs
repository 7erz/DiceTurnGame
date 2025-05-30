// __Scripts/SkillPanel.cs
using UnityEngine;
using System.Collections.Generic;
using TMPro; // TextMeshProUGUI 사용을 위해 추가

public class SkillPanel : MonoBehaviour
{
    public GameObject skillButtonPrefab;
    public Transform skillButtonContainer;

    // 인스펙터에서 Skill_InfoPanel 하위의 Title TextMeshProUGUI 할당
    public TextMeshProUGUI skillInfoPanelTitleText;
    // 인스펙터에서 Skill_InfoPanel 하위의 Info TextMeshProUGUI 할당
    public TextMeshProUGUI skillInfoPanelDetailsText;

    private List<SkillUI> instantiatedSkillButtons = new List<SkillUI>();

    public void PopulateSkillButtons(List<SkillData> skillsToDisplay)
    {
        foreach (SkillUI buttonUI in instantiatedSkillButtons)
        {
            Destroy(buttonUI.gameObject);
        }
        instantiatedSkillButtons.Clear();

        if (skillsToDisplay == null || skillsToDisplay.Count == 0)
        {
            Debug.LogWarning("표시할 스킬이 없습니다.");
            // 스킬 정보 패널 초기화
            UpdateSkillInfoPanel(null);
            return;
        }

        if (skillButtonPrefab == null || skillButtonContainer == null)
        {
            Debug.LogError("SkillButtonPrefab 또는 SkillButtonContainer가 할당되지 않았습니다.");
            return;
        }
        if (skillInfoPanelTitleText == null || skillInfoPanelDetailsText == null)
        {
            Debug.LogWarning("SkillPanel: Skill_InfoPanel의 Title 또는 Details Text가 할당되지 않았습니다. 정보 패널 업데이트가 제한될 수 있습니다.");
        }


        foreach (SkillData skill in skillsToDisplay)
        {
            GameObject buttonGO = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillUI skillUIComponent = buttonGO.GetComponent<SkillUI>();

            if (skillUIComponent != null)
            {
                // skillUIComponent.Setup(skill, this); // SkillUI가 SkillPanel을 직접 참조하는 방식
                // SkillUI가 InfoPanel의 Text들을 직접 참조하도록 수정했으므로, 해당 참조를 넘겨줍니다.
                skillUIComponent.Setup(skill, this, skillInfoPanelTitleText, skillInfoPanelDetailsText);
                instantiatedSkillButtons.Add(skillUIComponent);
            }
            else
            {
                Debug.LogError("SkillButtonPrefab에 SkillUI 컴포넌트가 없습니다.");
                Destroy(buttonGO);
            }
        }

        // 기본적으로 첫 번째 스킬 정보를 표시하거나, 아무것도 선택되지 않은 상태로 둘 수 있습니다.
        if (skillsToDisplay.Count > 0)
        {
            UpdateSkillInfoPanel(skillsToDisplay[0]); // 예시: 첫 번째 스킬 정보로 초기화
        }
        else
        {
            UpdateSkillInfoPanel(null); // 스킬이 없으면 정보 패널 비우기
        }
    }

    // Skill_InfoPanel의 내용을 업데이트하는 함수
    public void UpdateSkillInfoPanel(SkillData skill)
    {
        if (skillInfoPanelTitleText == null || skillInfoPanelDetailsText == null)
        {
            // 참조가 없으면 업데이트 불가
            return;
        }

        if (skill != null)
        {
            skillInfoPanelTitleText.text = skill.skillName;
            skillInfoPanelDetailsText.text = $"데미지: {skill.baseDamage}\n{skill.description}";
        }
        else
        {
            skillInfoPanelTitleText.text = "스킬 정보"; // 기본 텍스트
            skillInfoPanelDetailsText.text = "스킬을 선택해주세요."; // 기본 텍스트
        }
    }

    // (선택 사항) 게임 시작 시 플레이어 스킬 로드 예시 (GameManager에서 호출하도록 변경 권장)
    // void Start()
    // {
    //     // 이 로직은 GameManager로 옮기는 것이 좋습니다.
    //     // List<SkillData> testSkills = new List<SkillData>(Resources.LoadAll<SkillData>("Data")); // "Data" 폴더에 SkillData 에셋들이 있다고 가정
    //     // PopulateSkillButtons(testSkills);
    // }
}