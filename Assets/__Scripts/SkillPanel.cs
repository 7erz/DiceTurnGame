// __Scripts/SkillPanel.cs
using UnityEngine;
using System.Collections.Generic;
using TMPro; // TextMeshProUGUI ����� ���� �߰�

public class SkillPanel : MonoBehaviour
{
    public GameObject skillButtonPrefab;
    public Transform skillButtonContainer;

    // �ν����Ϳ��� Skill_InfoPanel ������ Title TextMeshProUGUI �Ҵ�
    public TextMeshProUGUI skillInfoPanelTitleText;
    // �ν����Ϳ��� Skill_InfoPanel ������ Info TextMeshProUGUI �Ҵ�
    public TextMeshProUGUI skillInfoPanelDetailsText;

    private List<SkillUI> instantiatedSkillButtons = new List<SkillUI>();
    private SkillData currentDisplayedSkillForInfo;   //���� ���� �гο� ǥ�õ� ��ų

    public void PopulateSkillButtons(List<SkillData> skillsToDisplay)
    {
        foreach (SkillUI buttonUI in instantiatedSkillButtons)
        {
            Destroy(buttonUI.gameObject);
        }
        instantiatedSkillButtons.Clear();

        if (skillsToDisplay == null || skillsToDisplay.Count == 0)
        {
            Debug.LogWarning("ǥ���� ��ų�� �����ϴ�.");
            // ��ų ���� �г� �ʱ�ȭ
            UpdateSkillInfoPanelWithDice(null);
            return;
        }

        if (skillButtonPrefab == null || skillButtonContainer == null)
        {
            Debug.LogError("SkillButtonPrefab �Ǵ� SkillButtonContainer�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }
        if (skillInfoPanelTitleText == null || skillInfoPanelDetailsText == null)
        {
            Debug.LogWarning("SkillPanel: Skill_InfoPanel�� Title �Ǵ� Details Text�� �Ҵ���� �ʾҽ��ϴ�. ���� �г� ������Ʈ�� ���ѵ� �� �ֽ��ϴ�.");
        }


        foreach (SkillData skill in skillsToDisplay)
        {
            GameObject buttonGO = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillUI skillUIComponent = buttonGO.GetComponent<SkillUI>();

            if (skillUIComponent != null)
            {
                // skillUIComponent.Setup(skill, this, skillInfoPanelTitleText, skillInfoPanelDetailsText); // ���� ���
                // SkillUI�� ��ư �̸��� �����ϰ�, ���� �г� ������Ʈ�� SkillPanel�� �����ϵ��� ����
                skillUIComponent.SetupButtonOnly(skill, this); // �� Setup �Լ� ȣ��
                instantiatedSkillButtons.Add(skillUIComponent);
            }
            else
            {
                Debug.LogError("SkillButtonPrefab�� SkillUI ������Ʈ�� �����ϴ�.");
                Destroy(buttonGO);
            }
        }

        if (skillsToDisplay.Count > 0)
        {
            // Populate �� ù ��° ��ų�� ���� �гο� ǥ�� (�ֻ��� ���� ���� �̹ݿ� ���·� ǥ���ϰų�, �ʱ� �ֻ��� ������ ǥ��)
            currentDisplayedSkillForInfo = skillsToDisplay[0];
            UpdateSkillInfoPanelWithDice(currentDisplayedSkillForInfo);
        }
        else
        {
            UpdateSkillInfoPanelWithDice(null);
        }
    }

    // ��ų ��ư Ŭ�� �� ȣ��Ǿ� ���� ǥ���� ��ų�� �����ϰ� ���� �г��� ������Ʈ
    public void SetCurrentSkillForInfoPanel(SkillData skill)
    {
        currentDisplayedSkillForInfo = skill;
        UpdateSkillInfoPanelWithDice(skill);
    }


    public void OnSkillButtonActivated(SkillData skill) // SkillUI�� ��ưŬ���� �� �Լ��� ȣ���Ѵٰ� ����
    {
        // 1. ���� �г� ������Ʈ (���� ����)
        SetCurrentSkillForInfoPanel(skill); // �� �Լ��� currentDisplayedSkillForInfo�� �����ϰ� UpdateSkillInfoPanelWithDice�� ȣ��

        // 2. GameManager�� ���ݿ� ��ų ����
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSelectedSkillForAttack(skill);
        }
        else
        {
            Debug.LogError("GameManager �ν��Ͻ��� �����ϴ�.");
        }
    }


    // Skill_InfoPanel�� ������ ������Ʈ�ϴ� �Լ�
    public void UpdateSkillInfoPanelWithDice(SkillData skill)
    {
        currentDisplayedSkillForInfo = skill;

        if (skillInfoPanelTitleText == null || skillInfoPanelDetailsText == null)
        {
            // ������ ������ ������Ʈ �Ұ�
            return;
        }

        if (skill == null)
        {
            skillInfoPanelTitleText.text = "��ų ����";
            skillInfoPanelDetailsText.text = "��ų�� �������ּ���.";
            return;
        }

        skillInfoPanelTitleText.text = skill.skillName;

        int baseDamage = skill.baseDamage;
        string description = skill.description;
        string finalDamageText = $"������: {baseDamage}";
        string calculationProcess = "";

        if (DiceManager.Instance != null)
        {
            string operation = DiceManager.Instance.GetSignOperation();
            int numberValue;

            if (DiceManager.Instance.TryGetSelectedNumberValue(out numberValue))
            {
                int calculatedDamage = baseDamage;
                string operationDisplay = $"{numberValue}";
                string colorTag = "blue"; // �⺻ ���� (�Ǵ� �ٸ� ��)

                switch (operation)
                {
                    case "+":
                        calculatedDamage += numberValue;
                        operationDisplay = $"+ {numberValue}";
                        colorTag = "green"; // ������ �ʷϻ�
                        break;
                    case "-":
                        calculatedDamage -= numberValue;
                        operationDisplay = $"- {numberValue}";
                        colorTag = "red"; // ������ ������
                        break;
                    case "*":
                        calculatedDamage *= numberValue;
                        operationDisplay = $"�� {numberValue}";
                        colorTag = "yellow"; // ������ �����
                        break;
                    case "/":
                        calculatedDamage *= numberValue;
                        operationDisplay = $"�� {numberValue}";
                        colorTag = "purple"; // �������� �����
                        break;
                }
                finalDamageText = $"������: {calculatedDamage}";
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