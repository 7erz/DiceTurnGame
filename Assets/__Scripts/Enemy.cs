// __Scripts/Enemy.cs
using UnityEngine;
using TMPro; // ü�� ǥ�ÿ� TextMeshPro (���� ����)
using System;

public class Enemy : MonoBehaviour
{
    public int maxHp; // ���� �ִ� ü��
    public int currentHp;

    // ���� ����: �� ���� ü���� ǥ���� TextMeshPro
    public TextMeshPro healthText; // �ν����Ϳ��� �Ҵ��ϰų� �ڵ�� ����/����

    // ü�� ���� �̺�Ʈ: ���� ü��, �ִ� ü���� ����
    public event Action<int, int> OnHealthChanged;
    // ��� �̺�Ʈ (�ν��Ͻ���)
    public event Action OnDiedInstance;

    // ���� ����: GameManager � ������ �˸��� ���� �̺�Ʈ
    public static event System.Action<Enemy> OnEnemyDiedManager;

    void Start()
    {
        currentHp = maxHp;
        UpdateHealthDisplay(); // ü�� ǥ�� ������Ʈ
        OnHealthChanged?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHp -= damageAmount;
        currentHp = Mathf.Max(currentHp, 0); // ü���� 0 �̸����� �������� �ʵ���
        Debug.Log($"{gameObject.name}�� {damageAmount}�� �������� �޾ҽ��ϴ�. ���� ü��: {currentHp}");

        OnHealthChanged?.Invoke(currentHp, maxHp); // ü�� ���� �̺�Ʈ ȣ��
        UpdateHealthDisplay();

        if (currentHp <= 0)
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
            healthText.text = $"{currentHp} / {maxHp}";
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
}