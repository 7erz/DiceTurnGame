// __Scripts/SkillUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUI : MonoBehaviour
{
    // �ν����Ϳ��� �Ҵ�: SkillButtonPrefab ������ ��ų �̸� ǥ�ÿ� Text
    public TextMeshProUGUI skillNameText_Button;

    // �ν����Ϳ��� �Ҵ� �Ǵ� �ڵ�� ã�� �Ҵ�: Skill_InfoPanel�� Title Text
    public TextMeshProUGUI skillInfoPanel_Title;
    // �ν����Ϳ��� �Ҵ� �Ǵ� �ڵ�� ã�� �Ҵ�: Skill_InfoPanel�� Info Text
    public TextMeshProUGUI skillInfoPanel_Details;

    private SkillData currentSkill;
    // SkillPanel�� ���� Skill_InfoPanel�� UI ��ҵ��� ������ ��� �ʿ�
    private SkillPanel skillPanelManager;

    // SkillPanel���� ȣ���Ͽ� ��ų �����Ϳ� �Ŵ����� ����
    public void Setup(SkillData skillToDisplay, SkillPanel panelManager, TextMeshProUGUI infoPanelTitle, TextMeshProUGUI infoPanelDetails)
    {
        currentSkill = skillToDisplay;
        skillPanelManager = panelManager;
        skillInfoPanel_Title = infoPanelTitle; // Skill_InfoPanel�� Title ����
        skillInfoPanel_Details = infoPanelDetails; // Skill_InfoPanel�� Info ����

        if (currentSkill != null)
        {
            if (skillNameText_Button != null)
            {
                skillNameText_Button.text = currentSkill.skillName;
            }
            else
            {
                Debug.LogError("SkillUI: skillNameText_Button�� �Ҵ���� �ʾҽ��ϴ�.");
            }
        }
        else
        {
            if (skillNameText_Button != null)
            {
                skillNameText_Button.text = "��ų ����";
            }
        }
    }

    // ��ư Ŭ�� �� ȣ��� �Լ�
    public void OnSkillButtonClicked()
    {
        if (currentSkill != null && skillPanelManager != null)
        {
            Debug.Log(currentSkill.skillName + " ��ư Ŭ����!");
            // SkillPanelManager�� ���� Skill_InfoPanel ������Ʈ ��û
            skillPanelManager.UpdateSkillInfoPanel(currentSkill);
        }
        else if (currentSkill != null) // ��ų ���� �г� ���� ������Ʈ (���)
        {
            if (skillInfoPanel_Title != null)
                skillInfoPanel_Title.text = currentSkill.skillName;
            if (skillInfoPanel_Details != null)
                skillInfoPanel_Details.text = $"������: {currentSkill.baseDamage}\n{currentSkill.description}";
        }
    }
}