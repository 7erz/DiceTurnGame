// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq 사용을 위해 추가 (Take 함수 등)
using TMPro; // TextMeshProUGUI 사용을 위해 추가
using UnityEngine.UI; // UI 관련 클래스 사용을 위해 추가
using System.Collections; // 코루틴 사용을 위해 추가

public enum GamePhase
{
    PlayerTurn_Roll,
    PlayerTurn_Action,
    EnemyTurn,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 인스턴스

    public SkillPanel skillPanel_UI; // 인스펙터에서 SkillPanel 할당
    public List<SkillData> allGameSkills; // 게임 내 모든 스킬 데이터 (로드용)
    public int maxSkillsToDisplay = 5; // 표시할 최대 스킬 개수

    [Header("적 설정")]
    public GameObject[] enemyPrefabs; // 적 프리팹 (인스펙터에서 할당)
    public Transform[] enemySpawnPoints; // 적 생성 위치 (인스펙터에서 할당, 빈 오브젝트로 설정)
    public int minEnemiesPerStage = 1; // 최소 적 생성 수
    public int maxEnemiesPerStage = 5; // 최대 적 생성 수

    private List<Enemy> activeEnemies = new List<Enemy>(); // 현재 활성화된 적 목록
    public Enemy currentTargetEnemy; // 현재 선택된 적 (공격 대상)

    [Header("전투 상태")]
    public SkillData currentSelectedSkillForAttack; // 현재 선택된 스킬 (공격 시 사용)

    [Header("액션 게이지 UI 관련")]
    public Image[] gaugeBars;
    public TextMeshProUGUI gaugeNumberText;

    [Header("게이지 바 컬러")] // 색상 설정용 변수 추가
    public Color gaugeEmptyColor = Color.white; // 비워진 칸 기본 색상
    public Color gaugePoint0Color = Color.green; // 0 포인트일 때 채워진 칸 색상
    public Color gaugePoint1Color = Color.yellow; // 1 포인트일 때 채워진 칸 색상
    public Color gaugePoint2Color = Color.red;    // 2 포인트일 때 채워진 칸 색상
    public Color gaugePoint3Color = new Color(0.8f, 0, 0); // 3 포인트일 때 채워진 칸 색상 (예: 어두운 빨강)

    [Header("액션 게이지 로직")]
    private int currentGaugeFillCount;      //현재 게이지 채움 횟수 (0~9)
    private int currentGaugePoints; // 현재 게이지 포인트
    public int maxGaugePoints = 3; // 최대 게이지 포인트 (0~3)
    public int gaugeBarsPerPoint = 10; // 게이지 포인트당 채워야 하는 게이지 바 수

    [Header("턴 상태")]
    public GamePhase currentPhase;
    public Button rollDiceButton;
    public Button endTurnButton;

    [Header("Turn Notification UI")] // 알림 UI 참조 추가
    public GameObject turnNotificationPanel; // 인스펙터에서 "TurnNotificationPanel" 할당
    public TextMeshProUGUI turnNotificationText; // 인스펙터에서 "NotificationText" 할당
    public float notificationDisplayTime = 2.0f; // 알림 표시 시간 (초)

    private Coroutine notificationCoroutine; // 실행 중인 알림 코루틴 참조


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

        // 알림 패널 초기 비활성화
        if (turnNotificationPanel != null)
        {
            turnNotificationPanel.SetActive(false);
        }


        // 스테이지 시작 시 적 생성
        StartNewGame();

        //게이지 로직
        InitializeGauge(); // 게이지 시스템 초기화
        //UpdateGaugeUI();   // UI 초기 업데이트
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
        allGameSkills = new List<SkillData>(Resources.LoadAll<SkillData>("SkillData"));

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
        if (currentPhase != GamePhase.PlayerTurn_Roll)
        {
            Debug.LogWarning("지금은 주사위를 굴릴 수 있는 단계가 아닙니다.");
            return;
        }

        if (DiceManager.Instance != null)
        {
            DiceManager.Instance.ResetAllDicesForNewTurn();
            Debug.Log("주사위 상태 리셋됨 (OnRollDiceButtonClicked).");

            DiceManager.Instance.RollAllDices();
            Debug.Log("주사위 굴림!");

            // 주사위 굴린 후 스킬 정보 패널 업데이트 (주사위 값 반영 위해)
            skillPanel_UI?.RefreshSkillInfoPanelWithCurrentDiceState();
        }

        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = false; // 굴리기 버튼 비활성화
            Debug.Log("굴리기 버튼 비활성화됨.");
        }
        ChangePhase(GamePhase.PlayerTurn_Action); // 행동 단계로 전환
    }

    // EndTurn 버튼의 OnClick 이벤트에 이 함수를 연결합니다.
    public void OnEndTurnButtonClicked()
    {
        if (currentPhase != GamePhase.PlayerTurn_Roll && currentPhase != GamePhase.PlayerTurn_Action)
        {
            Debug.LogWarning("지금은 턴을 종료할 수 없습니다.");
            return;
        }

        Debug.Log("플레이어 턴 종료.");
        ChangePhase(GamePhase.EnemyTurn);
    }

    public void StartNewStage()
    {
        ClearOldEnemies(); // 이전 스테이지 적들 정리 (필요시)
        SpawnEnemies();
        InitializeGauge(); // 새 스테이지 시작 시 게이지 초기화
        UpdateGaugeUI();   // 게이지 UI도 업데이트
    }

    public void StartNewGame()
    {
        StartNewStage(); // 새 스테이지 시작 (적 생성 등)
        ChangePhase(GamePhase.PlayerTurn_Roll); // 초기 페이즈 설정
    }

    public void ChangePhase(GamePhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log($"페이즈 변경: {currentPhase}");

        switch (currentPhase)
        {
            case GamePhase.PlayerTurn_Roll:
                ShowTurnNotification("플레이어 턴");
                StartPlayerRollPhase();
                break;
            case GamePhase.PlayerTurn_Action:
                StartPlayerActionPhase();
                break;
            case GamePhase.EnemyTurn:
                ShowTurnNotification("적 턴"); // 알림 표시
                StartEnemyTurn();
                break;
            case GamePhase.GameOver:
                ShowTurnNotification("게임 오버");
                // 게임 오버 처리
                break;
        }
    }

    void StartPlayerRollPhase()
    {
        Debug.Log("플레이어 턴: 주사위 굴리기 단계");

        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = true; // 굴리기 버튼 활성화
        }
        if (endTurnButton != null)
        {
            endTurnButton.interactable = true; // 턴 종료 버튼도 이 시점에 활성화 (또는 액션 후)
        }
        // 이전 턴에 사용한 주사위/스킬 선택 상태 초기화
        DiceManager.Instance?.DeselectAll();
        currentSelectedSkillForAttack = null;
        skillPanel_UI?.SetCurrentSkillForInfoPanel(null); // 스킬 정보창도 초기화
    }

    void StartPlayerActionPhase()
    {
        Debug.Log("플레이어 턴: 행동 단계");

    }

    void StartEnemyTurn()
    {
        Debug.Log("적 턴 시작");
        // 적 턴에는 플레이어의 모든 행동 UI 비활성화 (선택적)
        if (rollDiceButton != null) rollDiceButton.interactable = false;
        if (endTurnButton != null) endTurnButton.interactable = false;
        // skillPanel_UI 등도 비활성화 고려

        // 여기에 적의 행동 로직 (AI)을 구현합니다.
        // 예시: 모든 적이 순서대로 또는 랜덤하게 행동
        StartCoroutine(EnemyActionsCoroutine());
    }

    IEnumerator EnemyActionsCoroutine()
    {
        Debug.Log("적들의 행동 시작...");
        if (activeEnemies.Count > 0)
        {
            // 중요: activeEnemies 리스트를 직접 순회하면서 요소가 제거(적이 죽음)되면 오류 발생 가능
            // 따라서 ToList()로 복사본을 만들어 순회하거나, 역순으로 순회하는 등의 처리가 필요.
            // 여기서는 ToList()를 사용하여 현재 살아있는 적 목록의 복사본으로 행동 순서를 정함.
            List<Enemy> enemiesToAct = activeEnemies.Where(e => e != null && e.CurrentHp > 0).ToList();

            foreach (Enemy enemy in enemiesToAct)
            {
                if (enemy != null && enemy.CurrentHp > 0) // 행동 시점에도 살아있는지 다시 확인
                {
                    Debug.Log($"--- {enemy.gameObject.name} 행동 ---");
                    enemy.PerformAction(); // 각 적의 행동 실행
                    yield return new WaitForSeconds(1.5f); // 각 적의 행동 사이 딜레이 (애니메이션 시간 등 고려)
                }
            }
        }
        else
        {
            Debug.Log("행동할 적이 없습니다.");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("모든 적 행동 완료. 적 턴 종료.");
        ChangePhase(GamePhase.PlayerTurn_Roll); // 다시 플레이어 턴 (주사위 굴리기 단계)으로
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
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
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
            GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemyGO = Instantiate(randomEnemyPrefab, spawnPoint.position, Quaternion.identity);
            Enemy newEnemy = enemyGO.GetComponent<Enemy>();
            if (newEnemy != null)
            {
                activeEnemies.Add(newEnemy);
                if (newEnemy.enemyData != null)
                {
                    enemyGO.name = $"{newEnemy.enemyData.enemyName}_{i + 1}";
                }
                else
                {
                    enemyGO.name = $"Enemy_{i + 1}";
                }
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

    void ShowTurnNotification(string message)
    {
        if (turnNotificationPanel == null || turnNotificationText == null)
        {
            Debug.LogWarning("턴 알림 UI가 설정되지 않았습니다.");
            return;
        }

        // 이미 실행 중인 알림 코루틴이 있다면 중지 (새 알림으로 대체)
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message));
    }

    IEnumerator ShowNotificationCoroutine(string message)
    {
        turnNotificationText.text = message;
        turnNotificationPanel.SetActive(true);

        yield return new WaitForSeconds(notificationDisplayTime);

        turnNotificationPanel.SetActive(false);
        notificationCoroutine = null; // 코루틴 완료 후 참조 초기화
    }

    void InitializeGauge()
    {
        currentGaugeFillCount = 0;
        currentGaugePoints = 0;
        // UpdateGaugeUI(); // Start에서 호출하므로 중복 호출 방지
    }


    public void AddGauge(int amount)
    {
        if (currentGaugePoints >= maxGaugePoints && currentGaugeFillCount >= gaugeBarsPerPoint - 1)
        {
            Debug.Log("액션 게이지가 이미 최대로 가득 찼습니다.");
            return;
        }

        currentGaugeFillCount += amount;
        Debug.Log($"게이지 {amount} 증가. 현재 칸: {currentGaugeFillCount}");

        while (currentGaugeFillCount >= gaugeBarsPerPoint)
        {
            if (currentGaugePoints < maxGaugePoints)
            {
                currentGaugeFillCount -= gaugeBarsPerPoint;
                currentGaugePoints++;
                Debug.Log($"게이지 포인트 1 증가! 현재 포인트: {currentGaugePoints}, 남은 칸: {currentGaugeFillCount}");
            }
            else
            {
                currentGaugeFillCount = gaugeBarsPerPoint - 1;
                // 최대 포인트에서는 마지막 칸(9번째 인덱스)까지만 채워진 것으로 유지. 10칸째는 시각적으로 0으로 돌아가는게 아니라 9칸에서 멈춤.
                // 만약 10칸이 다 차면 0으로 돌아가면서 숫자가 1증가하는 표현을 원했다면,
                // currentGaugeFillCount = 0; 으로 하고, 최대 포인트일때는 currentGaugeFillCount가 gaugeBarsPerPoint-1을 넘지 않도록 조정해야함.
                // 현재 로직은 최대 포인트일때 10칸째가 차면 그냥 9칸에서 멈추는 방식.
                Debug.Log("최대 게이지 포인트에 도달했습니다. 더 이상 포인트는 증가하지 않습니다.");
                break;
            }
        }
        // currentGaugeFillCount가 음수가 되지 않도록 보정 (필요한 경우)
        currentGaugeFillCount = Mathf.Max(0, currentGaugeFillCount);

        UpdateGaugeUI();
    }

    public bool CanUseGaugePoints(int pointsToUse)
    {
        return currentGaugePoints >= pointsToUse;
    }

    public bool TryUseGaugePoints(int pointsToUse)
    {
        if (CanUseGaugePoints(pointsToUse))
        {
            currentGaugePoints -= pointsToUse;
            // currentGaugeFillCount는 변경하지 않음. 다음 AddGauge 시 현재 포인트 레벨에서 이어서 참.
            // 또는, 포인트가 줄어들면 currentGaugeFillCount가 10칸(gaugeBarsPerPoint-1)으로 꽉 차는 것으로 간주할 수도 있음.
            // 현재 요구사항("1포인트 3게이지칸이 채워지는걸로 보이는게 바뀌어야겠지?")는 fillCount가 유지되는 것을 의미.
            Debug.Log($"게이지 포인트 {pointsToUse} 사용. 남은 포인트: {currentGaugePoints}, 현재 칸: {currentGaugeFillCount}");
            UpdateGaugeUI();
            return true;
        }
        Debug.LogWarning("게이지 포인트가 부족합니다!");
        return false;
    }

    void UpdateGaugeUI()
    {
        if (gaugeBars == null || gaugeBars.Length == 0)
        {
            Debug.LogWarning("GaugeBars 배열이 할당되지 않았거나 비어있습니다.");
            return;
        }
        if (gaugeBars.Length != gaugeBarsPerPoint)
        {
            // 이 경고는 UI 구성에 따라 달라질 수 있으므로, 일단 주석 처리하거나 필요에 맞게 조정합니다.
            // Debug.LogWarning($"GaugeBars 배열의 크기({gaugeBars.Length})가 gaugeBarsPerPoint({gaugeBarsPerPoint})와 일치하지 않습니다.");
        }


        for (int i = 0; i < gaugeBars.Length; i++) // UI 게이지 바는 0~9 인덱스
        {
            if (gaugeBars[i] == null) continue;

            if (i < currentGaugeFillCount) // 현재 포인트 레벨에서 채워지고 있는 칸들
            {
                switch (currentGaugePoints)
                {
                    case 0: gaugeBars[i].color = gaugePoint0Color; break;
                    case 1: gaugeBars[i].color = gaugePoint1Color; break;
                    case 2: gaugeBars[i].color = gaugePoint2Color; break;
                    case 3: gaugeBars[i].color = gaugePoint3Color; break;
                    default: gaugeBars[i].color = gaugeEmptyColor; break; // 혹시 모를 상황
                }
            }
            else // 현재 포인트 레벨에서 아직 채워지지 않은 칸들
            {
                // 이 칸들은 이전 포인트 레벨의 색상으로 채워져 있어야 함 (만약 이전 포인트가 존재한다면)
                // 또는 완전히 비어있어야 함 (currentGaugePoints == 0 일 때)
                if (currentGaugePoints == 0) // 0포인트이고, 현재 fill count 뒤의 칸들은 빈 칸
                {
                    gaugeBars[i].color = gaugeEmptyColor;
                }
                else if (currentGaugePoints == 1) // 1포인트이고, 현재 fill count 뒤의 칸들은 0포인트 색(초록)으로 채워져 있어야 함
                {
                    gaugeBars[i].color = gaugePoint0Color;
                }
                else if (currentGaugePoints == 2) // 2포인트이고, 현재 fill count 뒤의 칸들은 1포인트 색(노랑)으로 채워져 있어야 함
                {
                    gaugeBars[i].color = gaugePoint1Color;
                }
                else if (currentGaugePoints == 3) // 3포인트이고, 현재 fill count 뒤의 칸들은 2포인트 색(빨강)으로 채워져 있어야 함
                {
                    gaugeBars[i].color = gaugePoint2Color;
                }
                else // 그 외의 경우는 빈 칸 (이론상 currentGaugePoints는 0~3 사이)
                {
                    gaugeBars[i].color = gaugeEmptyColor;
                }
            }
        }

        // 포인트 소모 시 예시 반영:
        // 만약 2포인트 3칸이었다면: currentGaugePoints = 2, currentGaugeFillCount = 3
        // 바 0,1,2는 gaugePoint2Color (빨강)
        // 바 3~9는 gaugePoint1Color (노랑) - 위 로직을 이렇게 수정해야 함.

        // 최종 수정된 로직 (사용자 예시: "2포인트 3칸이면 빨간색 3개, 노란색 7개" 반영)
        for (int i = 0; i < gaugeBars.Length; i++)
        {
            if (gaugeBars[i] == null) continue;

            if (i < currentGaugeFillCount) // 현재 포인트 레벨에서 채워지고 있는 부분
            {
                gaugeBars[i].color = GetColorForPointLevel(currentGaugePoints);
            }
            else // 현재 포인트 레벨에서 비어있지만, 이전 포인트 레벨로 채워져야 하는 부분
            {
                if (currentGaugePoints > 0) // 현재 포인트가 0보다 커야 이전 레벨이 존재
                {
                    gaugeBars[i].color = GetColorForPointLevel(currentGaugePoints - 1);
                }
                else // 현재 포인트가 0이고, currentGaugeFillCount 뒤의 칸들은 빈 칸
                {
                    gaugeBars[i].color = gaugeEmptyColor;
                }
            }
        }


        if (gaugeNumberText != null)
        {
            gaugeNumberText.text = currentGaugePoints.ToString();
        }
    }

    // 특정 포인트 레벨에 맞는 색상을 반환하는 헬퍼 함수
    private Color GetColorForPointLevel(int pointLevel)
    {
        switch (pointLevel)
        {
            case 0: return gaugePoint0Color;
            case 1: return gaugePoint1Color;
            case 2: return gaugePoint2Color;
            case 3: return gaugePoint3Color; // 3포인트 또는 그 이상 (최대 3이지만)
            default: return gaugeEmptyColor; // 음수 포인트 또는 예외 상황
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
                    case "*": finalDamage = baseDamage * numberValue; break;
                    case "/": finalDamage = baseDamage / numberValue; break;
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
            return;
        }

        if (currentSelectedSkillForAttack.gaugePointCost > 0) // 게이지 포인트를 소모하는 스킬이라면
        {
            if (!TryUseGaugePoints(currentSelectedSkillForAttack.gaugePointCost))
            {
                Debug.LogWarning($"{currentSelectedSkillForAttack.skillName} 사용 실패: 게이지 포인트 부족!");
                return; // 공격 중단
            }
        }

        int calculatedDamage;
        string calcDetails; // UI 표시용 계산 과정 (SkillPanel에서 이미 유사하게 처리 중이므로 여기선 로그용으로만 사용하거나 UI와 연동)
        BodyDice usedDice = DiceManager.Instance.GetSelectedNumberDice(); // 사용될 주사위 미리 참조

        if (TryCalculateFinalDamage(currentSelectedSkillForAttack, out calculatedDamage, out calcDetails))
        {
            Debug.Log($"{targetEnemy.name}에게 {currentSelectedSkillForAttack.skillName} 사용! 최종 데미지: {calculatedDamage}");
            targetEnemy.TakeDamage(calculatedDamage);

            // 사용한 숫자 주사위 비활성화
            if (usedDice != null)
            {
                usedDice.SetUsed(true);
                Debug.Log($"{usedDice.gameObject.name} 주사위 사용됨 처리.");
            }

            if (currentSelectedSkillForAttack.isBasicAttack)
            {
                AddGauge(currentSelectedSkillForAttack.gaugeChargeAmount);
            }

            Debug.Log($"{currentSelectedSkillForAttack.skillName} 사용 완료.");
            currentSelectedSkillForAttack = null;
            // DiceManager.Instance?.DeselectAll(); // 선택 해제는 ResetAllDicesForNewTurn에서 이미 처리 또는 SetUsed(true)에서 outline 해제됨.
            // 공격 후에는 현재 선택된 주사위가 없어져야 하므로 호출하는 것이 좋을 수 있음.
            DiceManager.Instance?.DeselectAll(); // 공격 후 현재 선택된 주사위 해제

            skillPanel_UI?.SetCurrentSkillForInfoPanel(null);
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
        if (currentTargetEnemy != null && currentTargetEnemy.CurrentHp > 0)
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