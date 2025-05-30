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

        if (DiceManager.Instance != null && skillPanel_UI != null)
        {
            DiceManager.Instance.OnDiceSelected.AddListener(HandleDiceSelectionChanged);
        }
    }

    // DiceManager.OnDiceSelected 이벤트가 발생했을 때 호출될 함수
    void HandleDiceSelectionChanged(BodyDice selectedDice) // BodyDice 파라미터는 이벤트 시그니처에 따름
    {
        if (skillPanel_UI != null)
        {
            skillPanel_UI.RefreshSkillInfoPanelWithCurrentDiceState();
        }
    }

    // RollButton의 OnClick 이벤트에 연결될 함수 (예시)
    public void OnRollDiceButtonClicked()
    {
        if (DiceManager.Instance != null)
        {
            DiceManager.Instance.RollAllDices();
            // 주사위를 굴린 후 잠시 기다렸다가 (애니메이션 등) 정보 패널 업데이트
            // 또는 RollAllDices 내부의 코루틴 마지막에 이벤트 발생시켜 처리
            if (skillPanel_UI != null)
            {
                // 바로 업데이트하거나, 주사위 롤 애니메이션 완료 후 업데이트
                skillPanel_UI.RefreshSkillInfoPanelWithCurrentDiceState();
            }
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