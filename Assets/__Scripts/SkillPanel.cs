using UnityEngine;

public class SkillPanel : MonoBehaviour
{
    public SkillUI[] skillSlots; // 5�� ����
    public SkillData[] allSkills; // ������Ʈ �� ��� ��ų ������

    void Start()
    {
        ShowRandomSkills();
    }

    public void ShowRandomSkills()
    {
        var selected = new System.Collections.Generic.List<SkillData>();
        while (selected.Count < 5)
        {
            var candidate = allSkills[Random.Range(0, allSkills.Length)];
            if (!selected.Contains(candidate))
                selected.Add(candidate);
        }
        for (int i = 0; i < skillSlots.Length; i++)
            skillSlots[i].SetSkill(selected[i]);
    }
}

//�������� ���� �� ShowRandomSkills() ȣ��� ��ų ��ü