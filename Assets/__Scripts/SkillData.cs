using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName; // ��ų �̸�
    public int baseDamage;
    public string description; // ��ų ����
}
