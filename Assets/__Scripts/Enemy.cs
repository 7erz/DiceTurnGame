// __Scripts/Enemy.cs
using UnityEngine;
using TMPro; // ü�� ǥ�ÿ� TextMeshPro (���� ����)
using System;
using System.Linq;

public enum EnemyActionType
{
    Attack,
    Defend,
    Heal,
    Debuff_Player, // �÷��̾�� �����
    Buff_Self,     // �ڽſ��� ����
    // �ʿ信 ���� �� �پ��� �ൿ �߰� ����

}

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    //public int maxHp; // ���� �ִ� ü��
    //public int currentHp;
    public int CurrentHp { get; private set; } //�ܺο��� �б⸸ ����

    // ���� ����: �� ���� ü���� ǥ���� TextMeshPro
    public TextMeshPro healthText; // �ν����Ϳ��� �Ҵ��ϰų� �ڵ�� ����/����

    // ü�� ���� �̺�Ʈ: ���� ü��, �ִ� ü���� ����
    public event Action<int, int> OnHealthChanged;
    // ��� �̺�Ʈ (�ν��Ͻ���)
    public event Action OnDiedInstance;

    // ���� ����: GameManager � ������ �˸��� ���� �̺�Ʈ
    public static event System.Action<Enemy> OnEnemyDiedManager;

    private int currentDefense = 0;

    void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError($"{gameObject.name}: EnemyData�� �Ҵ���� �ʾҽ��ϴ�!");
            // �⺻������ �ʱ�ȭ�ϰų�, ���� ó��
            CurrentHp = 50; // �ӽ� �⺻��
            // maxHp = 50;
        }
        else
        {
            CurrentHp = enemyData.maxHp;
            // maxHp = enemyData.maxHp; // �ʿ��ϴٸ� maxHp�� ����
        }
        UpdateHealthDisplay(); // ü�� ǥ�� ������Ʈ
        OnHealthChanged?.Invoke(CurrentHp, enemyData != null ? enemyData.maxHp : CurrentHp);
    }

    public void TakeDamage(int damageAmount)
    {
        int actualDamage = damageAmount;
        if (currentDefense > 0) // ���� ����
        {
            int defendedAmount = Mathf.Min(currentDefense, damageAmount);
            actualDamage -= defendedAmount;
            currentDefense -= defendedAmount;
            Debug.Log($"{gameObject.name}�� �������� {defendedAmount}�� �������� ���ҽ��ϴ�. ���� ����: {currentDefense}");
        }
        actualDamage = Mathf.Max(0, actualDamage); // ���� �������� ������ ���� �ʵ���

        CurrentHp -= actualDamage;
        CurrentHp = Mathf.Max(CurrentHp, 0);
        Debug.Log($"{gameObject.name}�� {actualDamage}�� ���� �������� �޾ҽ��ϴ�. ���� ü��: {CurrentHp}");

        OnHealthChanged?.Invoke(CurrentHp, enemyData != null ? enemyData.maxHp : CurrentHp);
        UpdateHealthDisplay();

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name}��(��) �׾����ϴ�.");
        OnDiedInstance?.Invoke(); // �ν��Ͻ� ��� �̺�Ʈ ȣ��
        OnEnemyDiedManager?.Invoke(this); // ���� �̺�Ʈ �߻�
        // ���⿡ ���� �ִϸ��̼�, ������ ��� ���� ���� �߰� ����
        Destroy(gameObject, 0.1f); // 0.1�� �� ������Ʈ �ı�
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = $"{CurrentHp} / {(enemyData != null ? enemyData.maxHp : CurrentHp)}";
        }
    }

    // �÷��̾ �� ���� Ŭ������ �� ȣ��� �� �ִ� �Լ� (���� ��� ������)
    void OnMouseDown()
    {
        // GameManager���� �� ���� Ŭ���Ǿ����� �˸�
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyClicked(this);
        }
        else
        {
            Debug.LogError("GameManager �ν��Ͻ��� �����ϴ�.");
        }
        // Debug.Log($"{gameObject.name} ���õ� (���� ��� �ĺ�)"); // GameManager.EnemyClicked���� �α� ó��
    }
    // ���� �ൿ ���� �� ����
    public void PerformAction()
    {
        if (enemyData == null || enemyData.actionPatterns.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: �ൿ ������ ���ų� EnemyData�� �����ϴ�. �⺻ ������ �����մϴ�.");
            PerformAttack(); // �⺻ �ൿ
            return;
        }

        // Ȯ�� ����ġ�� ���� �ൿ ����
        EnemyActionType chosenActionType = ChooseActionByProbability();
        ExecuteAction(chosenActionType);
    }

    private EnemyActionType ChooseActionByProbability()
    {
        int totalWeight = enemyData.actionPatterns.Sum(pattern => pattern.probabilityWeight);
        if (totalWeight <= 0) return EnemyActionType.Attack; // ����ġ�� ������ �⺻ ����

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var pattern in enemyData.actionPatterns)
        {
            cumulativeWeight += pattern.probabilityWeight;
            if (randomValue < cumulativeWeight)
            {
                return pattern.actionType;
            }
        }
        return EnemyActionType.Attack; // Ȥ�� �� ���� �� �⺻ ����
    }

    private void ExecuteAction(EnemyActionType actionType)
    {
        Debug.Log($"{gameObject.name}��(��) {actionType} �ൿ�� �����߽��ϴ�.");
        switch (actionType)
        {
            case EnemyActionType.Attack:
                PerformAttack();
                break;
            case EnemyActionType.Defend:
                PerformDefend();
                break;
            case EnemyActionType.Heal:
                PerformHeal();
                break;
            case EnemyActionType.Debuff_Player:
                PerformDebuffPlayer();
                break;
            case EnemyActionType.Buff_Self:
                PerformBuffSelf();
                break;
            default:
                Debug.LogWarning($"�� �� ���� �ൿ Ÿ��: {actionType}");
                PerformAttack(); // �⺻ ����
                break;
        }
        // �ൿ �� ���� �ʱ�ȭ (�� �� ������ ���µǴ� ����̶��)
        // �Ǵ� Defend ȿ���� ���� �� ���� ���ӵǵ��� ���� ����
        // currentDefense = 0; // ���⼭�� �� �ൿ �� ������ �ʱ�ȭ���� �ʵ��� �ּ� ó��. Defend������ ����.
    }

    // �� �ൿ�� ��ü���� ����
    private void PerformAttack()
    {
        // �÷��̾� ���� ��� �ʿ� (GameManager�� ���� �Ǵ� ����)
        // ��: Player player = GameManager.Instance.GetPlayer();
        // if (player != null) player.TakeDamage(enemyData.attackDamage);
        Debug.Log($"{gameObject.name}��(��) �÷��̾ �����մϴ�. (������: {enemyData?.attackDamage ?? 10})");
        // ���� �÷��̾�� �������� �ִ� ���� �ʿ�
    }

    private void PerformDefend()
    {
        currentDefense += enemyData?.defenseAmount ?? 5;
        Debug.Log($"{gameObject.name}��(��) ����մϴ�. (���� {enemyData?.defenseAmount ?? 5} ����, ���� ����: {currentDefense})");
    }

    private void PerformHeal()
    {
        if (enemyData == null) return;
        CurrentHp += enemyData.healAmount;
        CurrentHp = Mathf.Min(CurrentHp, enemyData.maxHp); // �ִ� ü���� ���� �ʵ���
        OnHealthChanged?.Invoke(CurrentHp, enemyData.maxHp);
        UpdateHealthDisplay();
        Debug.Log($"{gameObject.name}��(��) �����θ� ȸ���մϴ�. (ȸ����: {enemyData.healAmount}, ���� ü��: {CurrentHp})");
    }

    private void PerformDebuffPlayer()
    {
        Debug.Log($"{gameObject.name}��(��) �÷��̾�� ������� �̴ϴ�. (���� �ʿ�)");
        // ���� ����� ȿ�� ���� �ʿ�
    }

    private void PerformBuffSelf()
    {
        Debug.Log($"{gameObject.name}��(��) �ڽſ��� ������ �̴ϴ�. (���� �ʿ�)");
        // ���� ���� ȿ�� ���� �ʿ�
    }


}