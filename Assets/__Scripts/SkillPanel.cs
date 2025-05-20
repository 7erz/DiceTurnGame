using UnityEngine;

public class SkillPanel : MonoBehaviour
{
    public SkillUI[] skillSlots; // 5개 슬롯
    public SkillData[] allSkills; // 프로젝트 내 모든 스킬 데이터

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

//스테이지 종료 시 ShowRandomSkills() 호출로 스킬 교체