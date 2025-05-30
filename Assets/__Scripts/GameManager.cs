// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq ����� ���� �߰� (Take �Լ� ��)

public class GameManager : MonoBehaviour
{
    public SkillPanel skillPanel_UI; // �ν����Ϳ��� SkillPanel �Ҵ�
    public List<SkillData> allGameSkills; // ���� �� ��� ��ų ������ (�ε��)
    public int maxSkillsToDisplay = 5; // ǥ���� �ִ� ��ų ����

    void Start()
    {
        LoadAllSkillData(); // ��ų ������ �ε�

        if (skillPanel_UI != null && allGameSkills != null && allGameSkills.Count > 0)
        {
            // ǥ���� ��ų ��� ���� (��: ó�� 5��)
            List<SkillData> skillsToDisplay = allGameSkills.Take(Mathf.Min(maxSkillsToDisplay, allGameSkills.Count)).ToList();
            skillPanel_UI.PopulateSkillButtons(skillsToDisplay);
        }
        else if (skillPanel_UI != null)
        {
            skillPanel_UI.PopulateSkillButtons(new List<SkillData>()); // ��ų�� ���� ��� �� ����Ʈ ����
        }
    }

    void LoadAllSkillData()
    {
        // Resources ���� ������ "Data" �������� ��� SkillData ������ �ҷ��ɴϴ�.
        // �߿�: SkillData ���µ��� "Assets/Resources/Data" ������ �־�� �մϴ�.
        allGameSkills = new List<SkillData>(Resources.LoadAll<SkillData>("Data"));

        if (allGameSkills.Count == 0)
        {
            Debug.LogWarning("GameManager: Resources/Data �������� SkillData�� ã�� �� �����ϴ�.");
        }
    }
}