using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName; // 스킬 이름
    public int baseDamage;
    public string description; // 스킬 설명
}
