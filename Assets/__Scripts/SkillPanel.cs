// __Scripts/SkillPanel.cs
using UnityEngine;
using System.Collections.Generic;
using TMPro; // TextMeshProUGUI ����� ���� �߰�

public class SkillPanel : MonoBehaviour
{
    public GameObject skillButtonPrefab;
    public Transform skillButtonContainer;

    // �ν����Ϳ��� Skill_InfoPanel ������ Title TextMeshProUGUI �Ҵ�
    public TextMeshProUGUI skillInfoPanelTitleText;
    // �ν����Ϳ��� Skill_InfoPanel ������ Info TextMeshProUGUI �Ҵ�
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
            Debug.LogWarning("ǥ���� ��ų�� �����ϴ�.");
            // ��ų ���� �г� �ʱ�ȭ
            UpdateSkillInfoPanel(null);
            return;
        }

        if (skillButtonPrefab == null || skillButtonContainer == null)
        {
            Debug.LogError("SkillButtonPrefab �Ǵ� SkillButtonContainer�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }
        if (skillInfoPanelTitleText == null || skillInfoPanelDetailsText == null)
        {
            Debug.LogWarning("SkillPanel: Skill_InfoPanel�� Title �Ǵ� Details Text�� �Ҵ���� �ʾҽ��ϴ�. ���� �г� ������Ʈ�� ���ѵ� �� �ֽ��ϴ�.");
        }


        foreach (SkillData skill in skillsToDisplay)
        {
            GameObject buttonGO = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillUI skillUIComponent = buttonGO.GetComponent<SkillUI>();

            if (skillUIComponent != null)
            {
                // skillUIComponent.Setup(skill, this); // SkillUI�� SkillPanel�� ���� �����ϴ� ���
                // SkillUI�� InfoPanel�� Text���� ���� �����ϵ��� ���������Ƿ�, �ش� ������ �Ѱ��ݴϴ�.
                skillUIComponent.Setup(skill, this, skillInfoPanelTitleText, skillInfoPanelDetailsText);
                instantiatedSkillButtons.Add(skillUIComponent);
            }
            else
            {
                Debug.LogError("SkillButtonPrefab�� SkillUI ������Ʈ�� �����ϴ�.");
                Destroy(buttonGO);
            }
        }

        // �⺻������ ù ��° ��ų ������ ǥ���ϰų�, �ƹ��͵� ���õ��� ���� ���·� �� �� �ֽ��ϴ�.
        if (skillsToDisplay.Count > 0)
        {
            UpdateSkillInfoPanel(skillsToDisplay[0]); // ����: ù ��° ��ų ������ �ʱ�ȭ
        }
        else
        {
            UpdateSkillInfoPanel(null); // ��ų�� ������ ���� �г� ����
        }
    }

    // Skill_InfoPanel�� ������ ������Ʈ�ϴ� �Լ�
    public void UpdateSkillInfoPanel(SkillData skill)
    {
        if (skillInfoPanelTitleText == null || skillInfoPanelDetailsText == null)
        {
            // ������ ������ ������Ʈ �Ұ�
            return;
        }

        if (skill != null)
        {
            skillInfoPanelTitleText.text = skill.skillName;
            skillInfoPanelDetailsText.text = $"������: {skill.baseDamage}\n{skill.description}";
        }
        else
        {
            skillInfoPanelTitleText.text = "��ų ����"; // �⺻ �ؽ�Ʈ
            skillInfoPanelDetailsText.text = "��ų�� �������ּ���."; // �⺻ �ؽ�Ʈ
        }
    }

    // (���� ����) ���� ���� �� �÷��̾� ��ų �ε� ���� (GameManager���� ȣ���ϵ��� ���� ����)
    // void Start()
    // {
    //     // �� ������ GameManager�� �ű�� ���� �����ϴ�.
    //     // List<SkillData> testSkills = new List<SkillData>(Resources.LoadAll<SkillData>("Data")); // "Data" ������ SkillData ���µ��� �ִٰ� ����
    //     // PopulateSkillButtons(testSkills);
    // }
}