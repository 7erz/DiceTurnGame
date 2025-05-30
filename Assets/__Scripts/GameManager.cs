// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq 사용을 위해 추가 (Take 함수 등)

public class GameManager : MonoBehaviour
{
    public SkillPanel skillPanel_UI; // 인스펙터에서 SkillPanel 할당
    public List<SkillData> allGameSkills; // 게임 내 모든 스킬 데이터 (로드용)
    public int maxSkillsToDisplay = 5; // 표시할 최대 스킬 개수

    [Header("적 설정")]
    public GameObject enemyPrefab; // 적 프리팹 (인스펙터에서 할당)
    public Transform[] enemySpawnPoints; // 적 생성 위치 (인스펙터에서 할당, 빈 오브젝트로 설정)
    public int minEnemiesPerStage = 1; // 최소 적 생성 수
    public int maxEnemiesPerStage = 5; // 최대 적 생성 수

    private List<Enemy> activeEnemies = new List<Enemy>(); // 현재 활성화된 적 목록
    public Enemy currentTargetEnemy; // 현재 선택된 적 (공격 대상)

    void Start()
    {
        LoadAllSkillData();

        if (skillPanel_UI != null && allGameSkills != null && allGameSkills.Count > 0)
        {
            List<SkillData> skillsToDisplay = allGameSkills.Take(Mathf.Min(maxSkillsToDisplay, allGameSkills.Count)).ToList();
            skillPanel_UI.PopulateSkillButtons(skillsToDisplay);
        }
        else if (skillPanel_UI != null)
        {
            skillPanel_UI.PopulateSkillButtons(new List<SkillData>());
        }

        // DiceManager 이벤트 연결 (이전 답변 내용)
        if (DiceManager.Instance != null && skillPanel_UI != null)
        {
            DiceManager.Instance.OnDiceSelected.AddListener(HandleDiceSelectionChanged);
        }

        // 스테이지 시작 시 적 생성
        StartNewStage();
    }

    void OnEnable()
    {
        Enemy.OnEnemyDiedManager += HandleEnemyDeath; // 적 죽음 이벤트 구독
    }

    void OnDisable()
    {
        Enemy.OnEnemyDiedManager -= HandleEnemyDeath; // 적 죽음 이벤트 구독 해지
    }

    void LoadAllSkillData()
    {
        // Resources 폴더 하위의 "Data" 폴더에서 모든 SkillData 에셋을 불러옵니다.
        // 중요: SkillData 에셋들이 "Assets/Resources/Data" 폴더에 있어야 합니다.
        allGameSkills = new List<SkillData>(Resources.LoadAll<SkillData>("Data"));

        if (allGameSkills.Count == 0)
        {
            Debug.LogWarning("GameManager: Resources/Data 폴더에서 SkillData를 찾을 수 없습니다.");
        }
    }



    // DiceManager.OnDiceSelected 이벤트가 발생했을 때 호출될 함수
    void HandleDiceSelectionChanged(BodyDice selectedDice) // BodyDice 파라미터는 이벤트 시그니처에 따름
    {
        if (skillPanel_UI != null)
        {
            skillPanel_UI.RefreshSkillInfoPanelWithCurrentDiceState();
        }
    }

    

    // RollButton의 OnClick 이벤트에 연결될 함수 (예시)
    public void OnRollDiceButtonClicked()
    {
        if (DiceManager.Instance != null)
        {
            DiceManager.Instance.RollAllDices();
            // 주사위를 굴린 후 잠시 기다렸다가 (애니메이션 등) 정보 패널 업데이트
            // 또는 RollAllDices 내부의 코루틴 마지막에 이벤트 발생시켜 처리
            if (skillPanel_UI != null)
            {
                // 바로 업데이트하거나, 주사위 롤 애니메이션 완료 후 업데이트
                skillPanel_UI.RefreshSkillInfoPanelWithCurrentDiceState();
            }
        }
    }

    public void StartNewStage()
    {
        ClearOldEnemies(); // 이전 스테이지 적들 정리 (필요시)
        SpawnEnemies();
        // 기타 스테이지 시작 로직 (UI 업데이트 등)
    }

    void ClearOldEnemies()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        activeEnemies.Clear();
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab이 할당되지 않았습니다!");
            return;
        }
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogError("Enemy Spawn Points가 설정되지 않았습니다!");
            // 임시로 기본 위치에 생성하거나, 에러 처리
            // Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
            return;
        }

        int numberOfEnemies = Random.Range(minEnemiesPerStage, maxEnemiesPerStage + 1);
        Debug.Log($"이번 스테이지 적 생성 수: {numberOfEnemies}");


        for (int i = 0; i < numberOfEnemies; i++)
        {
            // 스폰 위치 랜덤하게 선택 (또는 순차적으로)
            Transform spawnPoint = enemySpawnPoints[i];
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            Enemy newEnemy = enemyGO.GetComponent<Enemy>();
            if (newEnemy != null)
            {
                activeEnemies.Add(newEnemy);
                enemyGO.name = $"Enemy_{i + 1}"; // 구분하기 쉽도록 이름 변경
            }
        }
        // 첫 번째 적을 기본 타겟으로 설정 (선택 사항)
        if (activeEnemies.Count > 0)
        {
            SelectTargetEnemy(activeEnemies[0]);
        }
    }

    void HandleEnemyDeath(Enemy deadEnemy)
    {
        if (activeEnemies.Contains(deadEnemy))
        {
            activeEnemies.Remove(deadEnemy);
        }

        if (currentTargetEnemy == deadEnemy) // 죽은 적이 현재 타겟이었다면 타겟 해제 또는 변경
        {
            currentTargetEnemy = null;
            if (activeEnemies.Count > 0) SelectTargetEnemy(activeEnemies[0]); // 다른 생존 적을 타겟으로
        }

        CheckStageClear();
    }

    void CheckStageClear()
    {
        if (activeEnemies.Count == 0)
        {
            Debug.Log("스테이지 클리어! 모든 적을 물리쳤습니다.");
            // 다음 스테이지로 이동하거나, 게임 클리어 처리
            // 예: Invoke("StartNewStage", 2f); // 2초 후 다음 스테이지 시작
        }
    }

    // 플레이어가 적을 선택하는 로직 (Enemy.cs의 OnMouseDown 등에서 호출 가능)
    public void SelectTargetEnemy(Enemy enemy)
    {
        // 기존 타겟 외곽선 제거 (필요시)
        // if (currentTargetEnemy != null) currentTargetEnemy.GetComponent<Outline>()?.enabled = false;

        currentTargetEnemy = enemy;
        Debug.Log($"현재 타겟: {currentTargetEnemy.gameObject.name}");

        // 새 타겟 외곽선 표시 (필요시)
        // if (currentTargetEnemy != null) currentTargetEnemy.GetComponent<Outline>()?.enabled = true;

        // UI에 현재 타겟 정보 업데이트 등
    }

    // 실제 공격 실행 함수 (Skill 사용 시 호출)
    public void ExecuteAttackOnTarget(int damage)
    {
        if (currentTargetEnemy != null && currentTargetEnemy.currentHp > 0)
        {
            Debug.Log($"{currentTargetEnemy.name}에게 {damage} 데미지 공격!");
            currentTargetEnemy.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning("공격 대상이 없거나 이미 죽었습니다.");
            // 공격 대상을 다시 선택하도록 유도
            if (activeEnemies.Count > 0) SelectTargetEnemy(activeEnemies[0]);
        }
    }


}