using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName; // 스킬 이름
    public int baseDamage;
    public string description; // 스킬 설명

    [Header("액션 게이지 설정")]
    public bool isBasicAttack;
    public int gaugeChargeAmount = 1; // 액션 게이지 충전량
    public int gaugePointCost = 0; // 액션 게이지 소모량
}
