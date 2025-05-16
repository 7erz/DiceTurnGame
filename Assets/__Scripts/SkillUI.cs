// SkillUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [Header("Settings")]
    public int baseDamage = 10;
    public Color modifiedColor = Color.red;

    [Header("References")]
    public TextMeshProUGUI damageText;
    public Button skillButton;

    void Start()
    {
        skillButton.onClick.AddListener(OnSkillClick);
        UpdateDisplay(baseDamage);
    }

    void OnEnable() => DiceManager.Instance.OnDiceSelected.AddListener(OnDiceSelected);
    void OnDisable() => DiceManager.Instance.OnDiceSelected.RemoveListener(OnDiceSelected);

    void OnDiceSelected(BodyDice dice)
    {
        if (dice == null)
            UpdateDisplay(baseDamage);
    }

    void OnSkillClick()
    {
        var selectedDice = DiceManager.Instance.GetSelectedDice();
        if (selectedDice == null) return;

        int finalDamage = CalculateDamage(selectedDice);
        UpdateDisplay(finalDamage, true);
    }

    int CalculateDamage(BodyDice dice)
    {
        if (int.TryParse(dice.LastRollResult, out int value))
            return baseDamage * value;

        // 부호 주사위 처리 (예: *는 2배)
        return dice.LastRollResult switch
        {
            "*" => baseDamage * 2,
            "+" => baseDamage + 5,
            "-" => baseDamage - 3,
            _ => baseDamage
        };
    }

    void UpdateDisplay(int damage, bool isModified = false)
    {
        damageText.text = isModified
            ? $"<color=#{ColorUtility.ToHtmlStringRGB(modifiedColor)}>{damage}</color>의 데미지를 줍니다"
            : $"{damage}의 데미지를 줍니다";
    }
}
