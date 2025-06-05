// __Scripts/Data/EnemyData.cs (���ο� ���� �� ���� ���� ��õ)
using UnityEngine;
using System.Collections.Generic; // List ���

[System.Serializable]
public class EnemyActionPattern
{
    public EnemyActionType actionType; // �ൿ Ÿ��
    [Range(0, 100)]
    public int probabilityWeight; // �ൿ Ȯ�� ����ġ (���� Ȯ���� �ƴ�, �ٸ� �ൿ����� ����� ����)
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "New Enemy";
    public int maxHp = 100;
    // public Sprite enemySprite; // ���߿� ��������Ʈ �߰� �� ���

    [Header("Behavior Patterns")]
    public List<EnemyActionPattern> actionPatterns = new List<EnemyActionPattern>();

    // ���⿡ �� �ൿ�� ��ü���� �Ķ���� �߰� ����
    // ��: Attack �� �⺻ ������, Heal �� ȸ���� ��
    [Header("Action Parameters")]
    public int attackDamage = 10; // Attack �ൿ �� �⺻ ������
    public int healAmount = 20;   // Heal �ൿ �� ȸ����
    public int defenseAmount = 5; // Defend �ൿ �� ��� ���� (�ӽ�)
    // �����/���� ���� �� ȿ�� ���Ǵ� �� �����ϹǷ� �ϴ� �⺻ �Ķ���͸� ����
}