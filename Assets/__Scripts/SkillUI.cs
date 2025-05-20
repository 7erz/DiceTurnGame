// SkillUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    public SkillData skillData;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescText;
    public Button skillButton;
    public Color modColor = Color.red;

    public void SetSkill(SkillData data)
    {
        skillButton.onClick.AddListener(OnSkillClicked);
        UpdateSkillText(skillData.baseDamage);
    }

    void OnSkillClicked()
    {
        var dice = DiceManager.Instance.GetSelectedDice();
        if (dice == null) return;

        int num;
        if (int.TryParse(dice.LastRollResult, out num))
        {
            int finalDamage = skillData.baseDamage * num;
            skillNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(modColor)}>{finalDamage}</color>의 데미지를 줍니다";
        }
        else
        {
            UpdateSkillText(skillData.baseDamage);
        }
    }

    void UpdateSkillText(int damage)
    {
        skillNameText.text = $"{damage}의 데미지를 줍니다";
    }
}
