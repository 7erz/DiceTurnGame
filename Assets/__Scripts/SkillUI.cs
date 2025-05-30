// __Scripts/SkillUI.cs
using UnityEngine;
// using UnityEngine.UI; // 필요시 사용
using TMPro;

public class SkillUI : MonoBehaviour
{
    public TextMeshProUGUI skillNameText_Button; // 인스펙터에서 할당 (버튼 위 텍스트)

    private SkillData currentSkill;
    private SkillPanel skillPanelManager; // SkillPanel 참조

    // SkillPanel에서 호출하여 스킬 데이터와 매니저를 설정 (버튼 전용)
    public void SetupButtonOnly(SkillData skillToDisplay, SkillPanel panelManager)
    {
        currentSkill = skillToDisplay;
        skillPanelManager = panelManager;

        if (currentSkill != null)
        {
            if (skillNameText_Button != null)
            {
                skillNameText_Button.text = currentSkill.skillName;
            }
        }
        else
        {
            if (skillNameText_Button != null)
            {
                skillNameText_Button.text = "X"; // 스킬 없음 표시
            }
        }
    }

    public void OnSkillButtonClicked()
    {
        if (currentSkill != null && skillPanelManager != null)
        {
            Debug.Log(currentSkill.skillName + " 버튼 클릭됨! 정보 패널 업데이트 요청.");
            skillPanelManager.SetCurrentSkillForInfoPanel(currentSkill); // 정보 패널 업데이트 요청
        }
    }
}