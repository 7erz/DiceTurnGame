// __Scripts/SkillPanel.cs
using UnityEngine;
using System.Collections.Generic;

public class SkillPanel : MonoBehaviour
{
    public GameObject skillButtonPrefab; // ������ ���� SkillButtonPrefab (�ν����Ϳ��� �Ҵ�)
    public Transform skillButtonContainer; // ��ų ��ư���� ������ �θ� Transform (��: �г��� Content ��ü, �ν�PECTOR���� �Ҵ�)

    private List<SkillUI> instantiatedSkillButtons = new List<SkillUI>();

    // ���ϴ� ��ų ����� �޾ƿͼ� UI�� ǥ���ϴ� �Լ�
    public void PopulateSkillButtons(List<SkillData> skills)
    {
        // ���� ��ư�� ���� (����� �������� �ٲ� ���)
        foreach (SkillUI buttonUI in instantiatedSkillButtons)
        {
            Destroy(buttonUI.gameObject);
        }
        instantiatedSkillButtons.Clear();

        // ���ο� ��ų ������� ��ư ����
        if (skills == null || skills.Count == 0)
        {
            Debug.LogWarning("ǥ���� ��ų�� �����ϴ�.");
            return;
        }

        if (skillButtonPrefab == null || skillButtonContainer == null)
        {
            Debug.LogError("SkillButtonPrefab �Ǵ� SkillButtonContainer�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        foreach (SkillData skill in skills)
        {
            GameObject buttonGO = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillUI skillUIComponent = buttonGO.GetComponent<SkillUI>();

            if (skillUIComponent != null)
            {
                skillUIComponent.DisplaySkill(skill);
                instantiatedSkillButtons.Add(skillUIComponent);
            }
            else
            {
                Debug.LogError("SkillButtonPrefab�� SkillUI ������Ʈ�� �����ϴ�.");
                Destroy(buttonGO); // �߸��� �������̸� �ı�
            }
        }
    }

    // ����: ���� ���� �� �÷��̾��� �⺻ ��ų���� ǥ��
    void Start()
    {
        // �� �κ��� ���� ���� ������ �°� �����ؾ� �մϴ�.
        // ���� ���, �÷��̾ ���� ��ų ����� �������ų�,
        // ��� ��� ������ ��ų ����� �����ͼ� ǥ���� �� �ֽ��ϴ�.

        // �ӽ÷� ��� ��ų �ּ��� Resources �������� �ҷ����� ����
        // 1. SkillData �ּµ��� "Resources/Skills" ������ �־�μ���.
        // SkillData[] allSkills = Resources.LoadAll<SkillData>("Skills");
        // PopulateSkillButtons(new List<SkillData>(allSkills));

        // �Ǵ� Ư�� ��ų ����� ���� ���� �׽�Ʈ�� �� �ֽ��ϴ�.
        // ��: �ν����Ϳ��� �Ҵ��� List<SkillData> public �ʵ带 �����, �ű⿡ ��ų �ּµ��� �־��� �� ���
    }
}