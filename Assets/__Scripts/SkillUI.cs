// __Scripts/SkillUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUI : MonoBehaviour
{
    // 인스펙터에서 할당: SkillButtonPrefab 내부의 스킬 이름 표시용 Text
    public TextMeshProUGUI skillNameText_Button;

    // 인스펙터에서 할당 또는 코드로 찾아 할당: Skill_InfoPanel의 Title Text
    public TextMeshProUGUI skillInfoPanel_Title;
    // 인스펙터에서 할당 또는 코드로 찾아 할당: Skill_InfoPanel의 Info Text
    public TextMeshProUGUI skillInfoPanel_Details;

    private SkillData currentSkill;
    // SkillPanel을 통해 Skill_InfoPanel의 UI 요소들을 제어할 경우 필요
    private SkillPanel skillPanelManager;

    // SkillPanel에서 호출하여 스킬 데이터와 매니저를 설정
    public void Setup(SkillData skillToDisplay, SkillPanel panelManager, TextMeshProUGUI infoPanelTitle, TextMeshProUGUI infoPanelDetails)
    {
        currentSkill = skillToDisplay;
        skillPanelManager = panelManager;
        skillInfoPanel_Title = infoPanelTitle; // Skill_InfoPanel의 Title 참조
        skillInfoPanel_Details = infoPanelDetails; // Skill_InfoPanel의 Info 참조

        if (currentSkill != null)
        {
            if (skillNameText_Button != null)
            {
                skillNameText_Button.text = currentSkill.skillName;
            }
            else
            {
                Debug.LogError("SkillUI: skillNameText_Button이 할당되지 않았습니다.");
            }
        }
        else
        {
            if (skillNameText_Button != null)
            {
                skillNameText_Button.text = "스킬 없음";
            }
        }
    }

    // 버튼 클릭 시 호출될 함수
    public void OnSkillButtonClicked()
    {
        if (currentSkill != null && skillPanelManager != null)
        {
            Debug.Log(currentSkill.skillName + " 버튼 클릭됨!");
            // SkillPanelManager를 통해 Skill_InfoPanel 업데이트 요청
            skillPanelManager.UpdateSkillInfoPanel(currentSkill);
        }
        else if (currentSkill != null) // 스킬 정보 패널 직접 업데이트 (대안)
        {
            if (skillInfoPanel_Title != null)
                skillInfoPanel_Title.text = currentSkill.skillName;
            if (skillInfoPanel_Details != null)
                skillInfoPanel_Details.text = $"데미지: {currentSkill.baseDamage}\n{currentSkill.description}";
        }
    }
}