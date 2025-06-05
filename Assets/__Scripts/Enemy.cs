// __Scripts/Enemy.cs
using UnityEngine;
using TMPro; // 체력 표시용 TextMeshPro (선택 사항)
using System;
using System.Linq;

public enum EnemyActionType
{
    Attack,
    Defend,
    Heal,
    Debuff_Player, // 플레이어에게 디버프
    Buff_Self,     // 자신에게 버프
    // 필요에 따라 더 다양한 행동 추가 가능

}

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    //public int maxHp; // 적의 최대 체력
    //public int currentHp;
    public int CurrentHp { get; private set; } //외부에서 읽기만 가능

    // 선택 사항: 적 위에 체력을 표시할 TextMeshPro
    public TextMeshPro healthText; // 인스펙터에서 할당하거나 코드로 생성/참조

    // 체력 변경 이벤트: 현재 체력, 최대 체력을 전달
    public event Action<int, int> OnHealthChanged;
    // 사망 이벤트 (인스턴스별)
    public event Action OnDiedInstance;

    // 선택 사항: GameManager 등에 죽음을 알리기 위한 이벤트
    public static event System.Action<Enemy> OnEnemyDiedManager;

    private int currentDefense = 0;

    void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError($"{gameObject.name}: EnemyData가 할당되지 않았습니다!");
            // 기본값으로 초기화하거나, 에러 처리
            CurrentHp = 50; // 임시 기본값
            // maxHp = 50;
        }
        else
        {
            CurrentHp = enemyData.maxHp;
            // maxHp = enemyData.maxHp; // 필요하다면 maxHp도 저장
        }
        UpdateHealthDisplay(); // 체력 표시 업데이트
        OnHealthChanged?.Invoke(CurrentHp, enemyData != null ? enemyData.maxHp : CurrentHp);
    }

    public void TakeDamage(int damageAmount)
    {
        int actualDamage = damageAmount;
        if (currentDefense > 0) // 방어력 적용
        {
            int defendedAmount = Mathf.Min(currentDefense, damageAmount);
            actualDamage -= defendedAmount;
            currentDefense -= defendedAmount;
            Debug.Log($"{gameObject.name}이 방어력으로 {defendedAmount}의 데미지를 막았습니다. 남은 방어력: {currentDefense}");
        }
        actualDamage = Mathf.Max(0, actualDamage); // 실제 데미지가 음수가 되지 않도록

        CurrentHp -= actualDamage;
        CurrentHp = Mathf.Max(CurrentHp, 0);
        Debug.Log($"{gameObject.name}이 {actualDamage}의 실제 데미지를 받았습니다. 현재 체력: {CurrentHp}");

        OnHealthChanged?.Invoke(CurrentHp, enemyData != null ? enemyData.maxHp : CurrentHp);
        UpdateHealthDisplay();

        if (CurrentHp <= 0)
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
            healthText.text = $"{CurrentHp} / {(enemyData != null ? enemyData.maxHp : CurrentHp)}";
        }
    }

    // 플레이어가 이 적을 클릭했을 때 호출될 수 있는 함수 (공격 대상 지정용)
    void OnMouseDown()
    {
        // GameManager에게 이 적이 클릭되었음을 알림
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyClicked(this);
        }
        else
        {
            Debug.LogError("GameManager 인스턴스가 없습니다.");
        }
        // Debug.Log($"{gameObject.name} 선택됨 (공격 대상 후보)"); // GameManager.EnemyClicked에서 로그 처리
    }
    // 적의 행동 결정 및 실행
    public void PerformAction()
    {
        if (enemyData == null || enemyData.actionPatterns.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 행동 패턴이 없거나 EnemyData가 없습니다. 기본 공격을 수행합니다.");
            PerformAttack(); // 기본 행동
            return;
        }

        // 확률 가중치에 따라 행동 선택
        EnemyActionType chosenActionType = ChooseActionByProbability();
        ExecuteAction(chosenActionType);
    }

    private EnemyActionType ChooseActionByProbability()
    {
        int totalWeight = enemyData.actionPatterns.Sum(pattern => pattern.probabilityWeight);
        if (totalWeight <= 0) return EnemyActionType.Attack; // 가중치가 없으면 기본 공격

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
        return EnemyActionType.Attack; // 혹시 모를 오류 시 기본 공격
    }

    private void ExecuteAction(EnemyActionType actionType)
    {
        Debug.Log($"{gameObject.name}이(가) {actionType} 행동을 선택했습니다.");
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
                Debug.LogWarning($"알 수 없는 행동 타입: {actionType}");
                PerformAttack(); // 기본 공격
                break;
        }
        // 행동 후 방어력 초기화 (매 턴 방어력이 리셋되는 방식이라면)
        // 또는 Defend 효과가 일정 턴 동안 지속되도록 구현 가능
        // currentDefense = 0; // 여기서는 매 행동 후 방어력이 초기화되지 않도록 주석 처리. Defend에서만 설정.
    }

    // 각 행동별 구체적인 로직
    private void PerformAttack()
    {
        // 플레이어 참조 방법 필요 (GameManager를 통해 또는 직접)
        // 예: Player player = GameManager.Instance.GetPlayer();
        // if (player != null) player.TakeDamage(enemyData.attackDamage);
        Debug.Log($"{gameObject.name}이(가) 플레이어를 공격합니다. (데미지: {enemyData?.attackDamage ?? 10})");
        // 실제 플레이어에게 데미지를 주는 로직 필요
    }

    private void PerformDefend()
    {
        currentDefense += enemyData?.defenseAmount ?? 5;
        Debug.Log($"{gameObject.name}이(가) 방어합니다. (방어력 {enemyData?.defenseAmount ?? 5} 증가, 현재 방어력: {currentDefense})");
    }

    private void PerformHeal()
    {
        if (enemyData == null) return;
        CurrentHp += enemyData.healAmount;
        CurrentHp = Mathf.Min(CurrentHp, enemyData.maxHp); // 최대 체력을 넘지 않도록
        OnHealthChanged?.Invoke(CurrentHp, enemyData.maxHp);
        UpdateHealthDisplay();
        Debug.Log($"{gameObject.name}이(가) 스스로를 회복합니다. (회복량: {enemyData.healAmount}, 현재 체력: {CurrentHp})");
    }

    private void PerformDebuffPlayer()
    {
        Debug.Log($"{gameObject.name}이(가) 플레이어에게 디버프를 겁니다. (구현 필요)");
        // 실제 디버프 효과 로직 필요
    }

    private void PerformBuffSelf()
    {
        Debug.Log($"{gameObject.name}이(가) 자신에게 버프를 겁니다. (구현 필요)");
        // 실제 버프 효과 로직 필요
    }


}