using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName; // ��ų �̸�
    public int baseDamage;
    public string description; // ��ų ����

    [Header("�׼� ������ ����")]
    public bool isBasicAttack;
    public int gaugeChargeAmount = 1; // �׼� ������ ������
    public int gaugePointCost = 0; // �׼� ������ �Ҹ�
}
