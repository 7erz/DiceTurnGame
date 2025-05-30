// __Scripts/SkillUI.cs
using UnityEngine;
// using UnityEngine.UI; // �ʿ�� ���
using TMPro;

public class SkillUI : MonoBehaviour
{
    public TextMeshProUGUI skillNameText_Button; // �ν����Ϳ��� �Ҵ� (��ư �� �ؽ�Ʈ)

    private SkillData currentSkill;
    private SkillPanel skillPanelManager; // SkillPanel ����

    // SkillPanel���� ȣ���Ͽ� ��ų �����Ϳ� �Ŵ����� ���� (��ư ����)
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
                skillNameText_Button.text = "X"; // ��ų ���� ǥ��
            }
        }
    }

    public void OnSkillButtonClicked()
    {
        if (currentSkill != null && skillPanelManager != null)
        {
            Debug.Log(currentSkill.skillName + " ��ư Ŭ����! ���� �г� ������Ʈ ��û.");
            skillPanelManager.SetCurrentSkillForInfoPanel(currentSkill); // ���� �г� ������Ʈ ��û
        }
    }
}