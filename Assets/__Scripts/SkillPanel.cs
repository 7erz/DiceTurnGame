// __Scripts/SkillPanel.cs
using UnityEngine;
using System.Collections.Generic;

public class SkillPanel : MonoBehaviour
{
    public GameObject skillButtonPrefab; // 위에서 만든 SkillButtonPrefab (인스펙터에서 할당)
    public Transform skillButtonContainer; // 스킬 버튼들이 생성될 부모 Transform (예: 패널의 Content 객체, 인스PECTOR에서 할당)

    private List<SkillUI> instantiatedSkillButtons = new List<SkillUI>();

    // 원하는 스킬 목록을 받아와서 UI에 표시하는 함수
    public void PopulateSkillButtons(List<SkillData> skills)
    {
        // 기존 버튼들 제거 (목록이 동적으로 바뀔 경우)
        foreach (SkillUI buttonUI in instantiatedSkillButtons)
        {
            Destroy(buttonUI.gameObject);
        }
        instantiatedSkillButtons.Clear();

        // 새로운 스킬 목록으로 버튼 생성
        if (skills == null || skills.Count == 0)
        {
            Debug.LogWarning("표시할 스킬이 없습니다.");
            return;
        }

        if (skillButtonPrefab == null || skillButtonContainer == null)
        {
            Debug.LogError("SkillButtonPrefab 또는 SkillButtonContainer가 할당되지 않았습니다.");
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
                Debug.LogError("SkillButtonPrefab에 SkillUI 컴포넌트가 없습니다.");
                Destroy(buttonGO); // 잘못된 프리팹이면 파괴
            }
        }
    }

    // 예시: 게임 시작 시 플레이어의 기본 스킬들을 표시
    void Start()
    {
        // 이 부분은 실제 게임 로직에 맞게 수정해야 합니다.
        // 예를 들어, 플레이어가 가진 스킬 목록을 가져오거나,
        // 모든 사용 가능한 스킬 목록을 가져와서 표시할 수 있습니다.

        // 임시로 모든 스킬 애셋을 Resources 폴더에서 불러오는 예시
        // 1. SkillData 애셋들을 "Resources/Skills" 폴더에 넣어두세요.
        // SkillData[] allSkills = Resources.LoadAll<SkillData>("Skills");
        // PopulateSkillButtons(new List<SkillData>(allSkills));

        // 또는 특정 스킬 목록을 직접 만들어서 테스트할 수 있습니다.
        // 예: 인스펙터에서 할당할 List<SkillData> public 필드를 만들고, 거기에 스킬 애셋들을 넣어준 뒤 사용
    }
}