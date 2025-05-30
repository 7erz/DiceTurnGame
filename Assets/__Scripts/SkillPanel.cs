// __Scripts/SkillPanel.cs
using UnityEngine;
using System.Collections.Generic;
using TMPro; // TextMeshProUGUI 사용을 위해 추가

public class SkillPanel : MonoBehaviour
{
    public GameObject skillButtonPrefab;
    public Transform skillButtonContainer;

    // 인스펙터에서 Skill_InfoPanel 하위의 Title TextMeshProUGUI 할당
    public TextMeshProUGUI skillInfoPanelTitleText;
    // 인스펙터에서 Skill_InfoPanel 하위의 Info TextMeshProUGUI 할당
    public TextMeshProUGUI skillInfoPanelDetailsText;

    private List<SkillUI> instantiatedSkillButtons = new List<SkillUI>();
    private SkillData currentDisplayedSkillForInfo;   //현재 정보 패널에 표시된 스킬

    public void PopulateSkillButtons(List<SkillData> skillsToDisplay)
    {
        foreach (SkillUI buttonUI in instantiatedSkillButtons)
        {
            Destroy(buttonUI.gameObject);
        }
        instantiatedSkillButtons.Clear();

        if (skillsToDisplay == null || skillsToDisplay.Count == 0)
        {
            Debug.LogWarning("표시할 스킬이 없습니다.");
            // 스킬 정보 패널 초기화
            UpdateSkillInfoPanelWithDice(null);
            return;
        }

        if (skillButtonPrefab == null || skillButtonContainer == null)
        {
            Debug.LogError("SkillButtonPrefab 또는 SkillButtonContainer가 할당되지 않았습니다.");
            return;
        }
        if (skillInfoPanelTitleText == null || skillInfoPanelDetailsText == null)
        {
            Debug.LogWarning("SkillPanel: Skill_InfoPanel의 Title 또는 Details Text가 할당되지 않았습니다. 정보 패널 업데이트가 제한될 수 있습니다.");
        }


        foreach (SkillData skill in skillsToDisplay)
        {
            GameObject buttonGO = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillUI skillUIComponent = buttonGO.GetComponent<SkillUI>();

            if (skillUIComponent != null)
            {
                // skillUIComponent.Setup(skill, this, skillInfoPanelTitleText, skillInfoPanelDetailsText); // 이전 방식
                // SkillUI는 버튼 이름만 설정하고, 정보 패널 업데이트는 SkillPanel이 전담하도록 변경
                skillUIComponent.SetupButtonOnly(skill, this); // 새 Setup 함수 호출
                instantiatedSkillButtons.Add(skillUIComponent);
            }
            else
            {
                Debug.LogError("SkillButtonPrefab에 SkillUI 컴포넌트가 없습니다.");
                Destroy(buttonGO);
            }
        }

        if (skillsToDisplay.Count > 0)
        {
            // Populate 후 첫 번째 스킬을 정보 패널에 표시 (주사위 값은 아직 미반영 상태로 표시하거나, 초기 주사위 값으로 표시)
            currentDisplayedSkillForInfo = skillsToDisplay[0];
            UpdateSkillInfoPanelWithDice(currentDisplayedSkillForInfo);
        }
        else
        {
            UpdateSkillInfoPanelWithDice(null);
        }
    }

    // 스킬 버튼 클릭 시 호출되어 현재 표시할 스킬을 설정하고 정보 패널을 업데이트
    public void SetCurrentSkillForInfoPanel(SkillData skill)
    {
        currentDisplayedSkillForInfo = skill;
        UpdateSkillInfoPanelWithDice(skill);
    }


    public void OnSkillButtonActivated(SkillData skill) // SkillUI의 버튼클릭이 이 함수를 호출한다고 가정
    {
        // 1. 정보 패널 업데이트 (기존 로직)
        SetCurrentSkillForInfoPanel(skill); // 이 함수는 currentDisplayedSkillForInfo를 설정하고 UpdateSkillInfoPanelWithDice를 호출

        // 2. GameManager에 공격용 스킬 설정
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSelectedSkillForAttack(skill);
        }
        else
        {
            Debug.LogError("GameManager 인스턴스가 없습니다.");
        }
    }


    // Skill_InfoPanel의 내용을 업데이트하는 함수
    public void UpdateSkillInfoPanelWithDice(SkillData skill)
    {
        currentDisplayedSkillForInfo = skill;

        if (skillInfoPanelTitleText == null || skillInfoPanelDetailsText == null)
        {
            // 참조가 없으면 업데이트 불가
            return;
        }

        if (skill == null)
        {
            skillInfoPanelTitleText.text = "스킬 정보";
            skillInfoPanelDetailsText.text = "스킬을 선택해주세요.";
            return;
        }

        skillInfoPanelTitleText.text = skill.skillName;

        int baseDamage = skill.baseDamage;
        string description = skill.description;
        string finalDamageText = $"데미지: {baseDamage}";
        string calculationProcess = "";

        if (DiceManager.Instance != null)
        {
            string operation = DiceManager.Instance.GetSignOperation();
            int numberValue;

            if (DiceManager.Instance.TryGetSelectedNumberValue(out numberValue))
            {
                int calculatedDamage = baseDamage;
                string operationDisplay = $"{numberValue}";
                string colorTag = "blue"; // 기본 색상 (또는 다른 색)

                switch (operation)
                {
                    case "+":
                        calculatedDamage += numberValue;
                        operationDisplay = $"+ {numberValue}";
                        colorTag = "green"; // 덧셈은 초록색
                        break;
                    case "-":
                        calculatedDamage -= numberValue;
                        operationDisplay = $"- {numberValue}";
                        colorTag = "red"; // 뺄셈은 빨간색
                        break;
                    case "*":
                        calculatedDamage *= numberValue;
                        operationDisplay = $"× {numberValue}";
                        colorTag = "yellow"; // 곱셈은 노란색
                        break;
                    case "/":
                        calculatedDamage *= numberValue;
                        operationDisplay = $"÷ {numberValue}";
                        colorTag = "purple"; // 나눗셈은 보라색
                        break;
                }
                finalDamageText = $"데미지: {calculatedDamage}";
                calculationProcess = $" ({baseDamage} <color={colorTag}>{operationDisplay}</color>)";
            }
        }

        skillInfoPanelDetailsText.text = $"{finalDamageText}{calculationProcess}\n{description}";
    }

    public void RefreshSkillInfoPanelWithCurrentDiceState()
    {
        if (currentDisplayedSkillForInfo != null)
        {
            UpdateSkillInfoPanelWithDice(currentDisplayedSkillForInfo);
        }
    }



}