// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq 사용을 위해 추가 (Take 함수 등)

public class GameManager : MonoBehaviour
{
    public SkillPanel skillPanel_UI; // 인스펙터에서 SkillPanel 할당
    public List<SkillData> allGameSkills; // 게임 내 모든 스킬 데이터 (로드용)
    public int maxSkillsToDisplay = 5; // 표시할 최대 스킬 개수

    void Start()
    {
        LoadAllSkillData(); // 스킬 데이터 로드

        if (skillPanel_UI != null && allGameSkills != null && allGameSkills.Count > 0)
        {
            // 표시할 스킬 목록 선택 (예: 처음 5개)
            List<SkillData> skillsToDisplay = allGameSkills.Take(Mathf.Min(maxSkillsToDisplay, allGameSkills.Count)).ToList();
            skillPanel_UI.PopulateSkillButtons(skillsToDisplay);
        }
        else if (skillPanel_UI != null)
        {
            skillPanel_UI.PopulateSkillButtons(new List<SkillData>()); // 스킬이 없을 경우 빈 리스트 전달
        }
    }

    void LoadAllSkillData()
    {
        // Resources 폴더 하위의 "Data" 폴더에서 모든 SkillData 에셋을 불러옵니다.
        // 중요: SkillData 에셋들이 "Assets/Resources/Data" 폴더에 있어야 합니다.
        allGameSkills = new List<SkillData>(Resources.LoadAll<SkillData>("Data"));

        if (allGameSkills.Count == 0)
        {
            Debug.LogWarning("GameManager: Resources/Data 폴더에서 SkillData를 찾을 수 없습니다.");
        }
    }
}