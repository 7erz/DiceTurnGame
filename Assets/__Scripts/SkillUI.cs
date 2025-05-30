// __Scripts/SkillUI.cs
using UnityEngine;
using UnityEngine.UI; // �Ϲ� Text�� ����Ѵٸ� �ʿ�
using TMPro; // TextMeshPro�� ����Ѵٸ� �ʿ�

public class SkillUI : MonoBehaviour
{
    public TextMeshProUGUI skillNameText; // ��ų �̸��� ǥ���� TextMeshProUGUI (�ν����Ϳ��� �Ҵ�)
    public TextMeshProUGUI skillDetailsText; // ��ų �������� ������ ǥ���� TextMeshProUGUI (�ν����Ϳ��� �Ҵ�)
    // public Image skillIconImage; // ��ų �������� ǥ���� Image (���� ����)

    private SkillData currentSkill; // ���� �� UI�� ǥ���ϴ� ��ų ������

    public void DisplaySkill(SkillData skillToDisplay)
    {
        currentSkill = skillToDisplay;

        if (currentSkill != null)
        {
            skillNameText.text = currentSkill.skillName;
            // �������� ������ ���ļ� ǥ�� (���ϴ� �������� ���� ����)
            skillDetailsText.text = $"������: {currentSkill.baseDamage}\n{currentSkill.description}";

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
            // ��ų �����Ͱ� ���� ��� UI�� ��Ȱ��ȭ�ϰų� �⺻ �ؽ�Ʈ ����
            skillNameText.text = "��ų ����";
            skillDetailsText.text = "";
            // if (skillIconImage != null) skillIconImage.enabled = false;
        }
    }

    // (���� ����) ��ư Ŭ�� �� ȣ��� �Լ�
    public void OnSkillButtonClicked()
    {
        if (currentSkill != null)
        {
            Debug.Log(currentSkill.skillName + " ��ư Ŭ����!");
            // ���⿡ ��ų ���� �����̳� �ٸ� ��ȣ�ۿ��� �߰��� �� �ֽ��ϴ�.
            // ��: SkillPanel�̳� GameManager�� ���õ� ��ų�� �˸�
        }
    }
}