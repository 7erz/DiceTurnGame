// GameManager.cs (예시)
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public SkillPanel skillPanel_UI; // 인스펙터에서 SkillPanel 할당
    public List<SkillData> playerSelectedSkills; // 플레이어가 선택한 스킬들

    void Start()
    {
        // playerSelectedSkills 목록이 채워졌다고 가정
        // 예: LoadPlayerData();
        if (skillPanel_UI != null && playerSelectedSkills != null && playerSelectedSkills.Count > 0)
        {
            skillPanel_UI.PopulateSkillButtons(playerSelectedSkills);
        }
    }
}