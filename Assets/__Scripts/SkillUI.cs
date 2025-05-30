// __Scripts/SkillUI.cs
using UnityEngine;
using UnityEngine.UI; // 일반 Text를 사용한다면 필요
using TMPro; // TextMeshPro를 사용한다면 필요

public class SkillUI : MonoBehaviour
{
    public TextMeshProUGUI skillNameText; // 스킬 이름을 표시할 TextMeshProUGUI (인스펙터에서 할당)
    public TextMeshProUGUI skillDetailsText; // 스킬 데미지와 설명을 표시할 TextMeshProUGUI (인스펙터에서 할당)
    // public Image skillIconImage; // 스킬 아이콘을 표시할 Image (선택 사항)

    private SkillData currentSkill; // 현재 이 UI가 표시하는 스킬 데이터

    public void DisplaySkill(SkillData skillToDisplay)
    {
        currentSkill = skillToDisplay;

        if (currentSkill != null)
        {
            skillNameText.text = currentSkill.skillName;
            // 데미지와 설명을 합쳐서 표시 (원하는 형식으로 수정 가능)
            skillDetailsText.text = $"데미지: {currentSkill.baseDamage}\n{currentSkill.description}";

            // if (skillIconImage != null && currentSkill.icon != null)
            // {
            //     skillIconImage.sprite = currentSkill.icon;
            //     skillIconImage.enabled = true;
            // }
            // else if (skillIconImage != null)
            // {
            //     skillIconImage.enabled = false;
            // }
        }
        else
        {
            // 스킬 데이터가 없는 경우 UI를 비활성화하거나 기본 텍스트 설정
            skillNameText.text = "스킬 없음";
            skillDetailsText.text = "";
            // if (skillIconImage != null) skillIconImage.enabled = false;
        }
    }

    // (선택 사항) 버튼 클릭 시 호출될 함수
    public void OnSkillButtonClicked()
    {
        if (currentSkill != null)
        {
            Debug.Log(currentSkill.skillName + " 버튼 클릭됨!");
            // 여기에 스킬 선택 로직이나 다른 상호작용을 추가할 수 있습니다.
            // 예: SkillPanel이나 GameManager에 선택된 스킬을 알림
        }
    }
}