// __Scripts/Enemy.cs
using UnityEngine;
using TMPro; // 체력 표시용 TextMeshPro (선택 사항)
using System;

public class Enemy : MonoBehaviour
{
    public int maxHp; // 적의 최대 체력
    public int currentHp;

    // 선택 사항: 적 위에 체력을 표시할 TextMeshPro
    public TextMeshPro healthText; // 인스펙터에서 할당하거나 코드로 생성/참조

    // 체력 변경 이벤트: 현재 체력, 최대 체력을 전달
    public event Action<int, int> OnHealthChanged;
    // 사망 이벤트 (인스턴스별)
    public event Action OnDiedInstance;

    // 선택 사항: GameManager 등에 죽음을 알리기 위한 이벤트
    public static event System.Action<Enemy> OnEnemyDiedManager;

    void Start()
    {
        currentHp = maxHp;
        UpdateHealthDisplay(); // 체력 표시 업데이트
        OnHealthChanged?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHp -= damageAmount;
        currentHp = Mathf.Max(currentHp, 0); // 체력이 0 미만으로 내려가지 않도록
        Debug.Log($"{gameObject.name}이 {damageAmount}의 데미지를 받았습니다. 현재 체력: {currentHp}");

        OnHealthChanged?.Invoke(currentHp, maxHp); // 체력 변경 이벤트 호출
        UpdateHealthDisplay();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name}이(가) 죽었습니다.");
        OnDiedInstance?.Invoke(); // 인스턴스 사망 이벤트 호출
        OnEnemyDiedManager?.Invoke(this); // 죽음 이벤트 발생
        // 여기에 죽음 애니메이션, 아이템 드랍 등의 로직 추가 가능
        Destroy(gameObject, 0.1f); // 0.1초 후 오브젝트 파괴
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHp} / {maxHp}";
        }
    }

    // 플레이어가 이 적을 클릭했을 때 호출될 수 있는 함수 (공격 대상 지정용)
    /*void OnMouseDown()
    {
        if (GameManager.Instance != null) // GameManager가 있다는 가정 하에
        {
            GameManager.Instance.SelectTargetEnemy(this);
        }
        Debug.Log($"{gameObject.name} 선택됨 (공격 대상 후보)");
    }*/
}