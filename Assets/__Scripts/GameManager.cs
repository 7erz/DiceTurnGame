// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq 사용을 위해 추가 (Take 함수 등)

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 인스턴스

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

    [Header("전투 상태")]
    public SkillData currentSelectedSkillForAttack; // 현재 선택된 스킬 (공격 시 사용)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 중복 생성 방지
        }
    }

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
        currentTargetEnemy = null; // 현재 타겟도 초기화
        currentSelectedSkillForAttack = null; // 선택된 스킬도 초기화
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

    // SkillPanel에서 호출하여 플레이어가 사용할 스킬을 설정
    public void SetSelectedSkillForAttack(SkillData skill)
    {
        currentSelectedSkillForAttack = skill;
        Debug.Log($"공격 스킬 준비: {skill?.skillName ?? "없음"}");

        // 스킬 선택 시 스킬 정보 패널도 업데이트 (SkillPanel에 이미 관련 로직이 있다면 중복 호출 주의)
        // skillPanel_UI?.UpdateSkillInfoPanelWithDice(skill);
        // 또는 SkillPanel의 SetCurrentSkillForInfoPanel를 호출하여 일관성 유지
        skillPanel_UI?.SetCurrentSkillForInfoPanel(skill);

        // 만약 스킬 선택 후 바로 타겟을 지정하는 방식이라면 여기서 추가 로직
    }
    // Enemy의 OnMouseDown에서 호출되어 타겟을 설정하고, 조건이 맞으면 공격 실행
    public void EnemyClicked(Enemy clickedEnemy)
    {
        currentTargetEnemy = clickedEnemy; // 항상 클릭된 적을 현재 타겟으로 설정
        Debug.Log($"타겟 지정: {clickedEnemy.gameObject.name}");

        // (선택사항) 타겟 시각적 피드백 (예: 모든 적의 하이라이트 제거 후, 현재 타겟만 하이라이트)
        // foreach(var enemy in activeEnemies) { enemy.SetHighlight(false); }
        // clickedEnemy.SetHighlight(true);


        if (currentSelectedSkillForAttack != null)
        {
            // 주사위가 굴려졌고, 숫자 주사위가 선택되었는지 확인
            if (DiceManager.Instance != null && DiceManager.Instance.GetSelectedNumberDice() != null)
            {
                Debug.Log($"{clickedEnemy.gameObject.name}에게 {currentSelectedSkillForAttack.skillName} 스킬 사용 시도.");
                AttemptAttackOnEnemy(clickedEnemy);
            }
            else
            {
                Debug.LogWarning("스킬을 사용하려면 먼저 주사위를 굴리고 숫자 주사위를 선택해야 합니다.");
                // 여기에 사용자에게 알림 UI 표시 로직 추가 가능
            }
        }
        else
        {
            Debug.Log("선택된 공격 스킬이 없습니다. 먼저 스킬을 선택해주세요.");
            // 여기에 사용자에게 알림 UI 표시 로직 추가 가능
        }
    }

    private bool TryCalculateFinalDamage(SkillData skill, out int finalDamage, out string calculationDetailsForUI)
    {
        finalDamage = 0;
        calculationDetailsForUI = "";
        if (skill == null) return false;

        int baseDamage = skill.baseDamage;
        finalDamage = baseDamage;
        calculationDetailsForUI = baseDamage.ToString();

        if (DiceManager.Instance != null)
        {
            string operation = DiceManager.Instance.GetSignOperation();
            int numberValue;

            if (DiceManager.Instance.TryGetSelectedNumberValue(out numberValue))
            {
                string colorTag = operation == "+" ? "green" : (operation == "-" ? "red" : "blue");
                calculationDetailsForUI = $"{baseDamage} <color={colorTag}>{operation} {numberValue}</color>";

                switch (operation)
                {
                    case "+": finalDamage = baseDamage + numberValue; break;
                    case "-": finalDamage = baseDamage - numberValue; break;
                        // 곱하기, 나누기 등
                }
            }
            else
            {
                Debug.LogWarning("선택된 숫자 주사위 값이 없습니다. 기본 데미지로 계산합니다.");
                // calculationDetailsForUI는 이미 baseDamage로 설정됨
            }
        }
        else
        {
            Debug.LogWarning("DiceManager 인스턴스를 찾을 수 없습니다. 기본 데미지로 계산합니다.");
        }
        finalDamage = Mathf.Max(0, finalDamage); // 데미지가 음수가 되지 않도록
        return true;
    }

    private void AttemptAttackOnEnemy(Enemy targetEnemy)
    {
        if (currentSelectedSkillForAttack == null || targetEnemy == null)
        {
            Debug.LogError("스킬 또는 타겟이 지정되지 않았습니다.");
            return;
        }

        int calculatedDamage;
        string calcDetails; // UI 표시용 계산 과정 (SkillPanel에서 이미 유사하게 처리 중이므로 여기선 로그용으로만 사용하거나 UI와 연동)

        if (TryCalculateFinalDamage(currentSelectedSkillForAttack, out calculatedDamage, out calcDetails))
        {
            Debug.Log($"{targetEnemy.name}에게 {currentSelectedSkillForAttack.skillName} 사용! 최종 데미지: {calculatedDamage} (계산: {calcDetails.Replace("<color=green>", "").Replace("<color=red>", "").Replace("</color>", "")})"); // 로그에는 색상 코드 제거
            targetEnemy.TakeDamage(calculatedDamage);

            // 공격 후 처리
            Debug.Log($"{currentSelectedSkillForAttack.skillName} 사용 완료.");
            currentSelectedSkillForAttack = null; // 사용한 스킬 선택 해제

            // 사용한 주사위 비활성화 또는 선택 해제 (DiceManager에 관련 기능 추가 필요 시)
            // DiceManager.Instance.GetSelectedNumberDice()?.LockDice(); // 예시: 주사위 잠금
            DiceManager.Instance.DeselectAll(); // 숫자 주사위 선택 해제

            // 스킬 정보 패널에서 선택된 스킬 정보 초기화 (또는 다음 행동 안내)
            skillPanel_UI?.SetCurrentSkillForInfoPanel(null); // 정보 패널 비우기 또는 기본 상태로
                                                              // 또는 skillPanel_UI.RefreshSkillInfoPanelWithCurrentDiceState(); 호출하여 주사위 값만 남기기

            // 턴 종료 로직 또는 다음 행동 대기 상태로 전환
            // 예: EndPlayerTurn();
        }
        else
        {
            Debug.LogError("데미지 계산에 실패했습니다.");
        }
    }
    // GameManager.ExecuteAttackOnTarget(int damage) 함수는 이제 AttemptAttackOnEnemy 내부 로직으로 통합되었으므로,
    // 직접적인 public ExecuteAttackOnTarget는 필요 없을 수 있습니다. 만약 다른 곳에서 순수 데미지만큼 공격하는 기능이 필요하다면 유지합니다.

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