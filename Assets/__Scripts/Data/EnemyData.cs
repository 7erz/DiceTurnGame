// __Scripts/Data/EnemyData.cs (새로운 폴더 및 파일 생성 추천)
using UnityEngine;
using System.Collections.Generic; // List 사용

[System.Serializable]
public class EnemyActionPattern
{
    public EnemyActionType actionType; // 행동 타입
    [Range(0, 100)]
    public int probabilityWeight; // 행동 확률 가중치 (절대 확률이 아닌, 다른 행동들과의 상대적 비율)
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "New Enemy";
    public int maxHp = 100;
    // public Sprite enemySprite; // 나중에 스프라이트 추가 시 사용

    [Header("Behavior Patterns")]
    public List<EnemyActionPattern> actionPatterns = new List<EnemyActionPattern>();

    // 여기에 각 행동별 구체적인 파라미터 추가 가능
    // 예: Attack 시 기본 데미지, Heal 시 회복량 등
    [Header("Action Parameters")]
    public int attackDamage = 10; // Attack 행동 시 기본 데미지
    public int healAmount = 20;   // Heal 행동 시 회복량
    public int defenseAmount = 5; // Defend 행동 시 얻는 방어력 (임시)
    // 디버프/버프 종류 및 효과 정의는 더 복잡하므로 일단 기본 파라미터만 예시
}