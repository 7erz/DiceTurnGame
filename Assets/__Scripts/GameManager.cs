// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq ����� ���� �߰� (Take �Լ� ��)
using TMPro; // TextMeshProUGUI ����� ���� �߰�
using UnityEngine.UI; // UI ���� Ŭ���� ����� ���� �߰�

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // �̱��� �ν��Ͻ�

    public SkillPanel skillPanel_UI; // �ν����Ϳ��� SkillPanel �Ҵ�
    public List<SkillData> allGameSkills; // ���� �� ��� ��ų ������ (�ε��)
    public int maxSkillsToDisplay = 5; // ǥ���� �ִ� ��ų ����

    [Header("�� ����")]
    public GameObject enemyPrefab; // �� ������ (�ν����Ϳ��� �Ҵ�)
    public Transform[] enemySpawnPoints; // �� ���� ��ġ (�ν����Ϳ��� �Ҵ�, �� ������Ʈ�� ����)
    public int minEnemiesPerStage = 1; // �ּ� �� ���� ��
    public int maxEnemiesPerStage = 5; // �ִ� �� ���� ��

    private List<Enemy> activeEnemies = new List<Enemy>(); // ���� Ȱ��ȭ�� �� ���
    public Enemy currentTargetEnemy; // ���� ���õ� �� (���� ���)

    [Header("���� ����")]
    public SkillData currentSelectedSkillForAttack; // ���� ���õ� ��ų (���� �� ���)

    [Header("�׼� ������ UI ����")]
    public Image[] gaugeBars;
    public TextMeshProUGUI gaugeNumberText;

    [Header("������ �� �÷�")] // ���� ������ ���� �߰�
    public Color gaugeEmptyColor = Color.white; // ����� ĭ �⺻ ����
    public Color gaugePoint0Color = Color.green; // 0 ����Ʈ�� �� ä���� ĭ ����
    public Color gaugePoint1Color = Color.yellow; // 1 ����Ʈ�� �� ä���� ĭ ����
    public Color gaugePoint2Color = Color.red;    // 2 ����Ʈ�� �� ä���� ĭ ����
    public Color gaugePoint3Color = new Color(0.8f, 0, 0); // 3 ����Ʈ�� �� ä���� ĭ ���� (��: ��ο� ����)

    [Header("�׼� ������ ����")]
    private int currentGaugeFillCount;      //���� ������ ä�� Ƚ�� (0~9)
    private int currentGaugePoints; // ���� ������ ����Ʈ
    public int maxGaugePoints = 3; // �ִ� ������ ����Ʈ (0~3)
    public int gaugeBarsPerPoint = 10; // ������ ����Ʈ�� ä���� �ϴ� ������ �� ��



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �ÿ��� ����
        }
        else
        {
            Destroy(gameObject); // �̹� �ν��Ͻ��� �����ϸ� �ߺ� ���� ����
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

        // DiceManager �̺�Ʈ ���� (���� �亯 ����)
        if (DiceManager.Instance != null && skillPanel_UI != null)
        {
            DiceManager.Instance.OnDiceSelected.AddListener(HandleDiceSelectionChanged);
        }

        // �������� ���� �� �� ����
        StartNewStage();

        //������ ����
        InitializeGauge(); // ������ �ý��� �ʱ�ȭ
        UpdateGaugeUI();   // UI �ʱ� ������Ʈ
    }

    void OnEnable()
    {
        Enemy.OnEnemyDiedManager += HandleEnemyDeath; // �� ���� �̺�Ʈ ����
    }

    void OnDisable()
    {
        Enemy.OnEnemyDiedManager -= HandleEnemyDeath; // �� ���� �̺�Ʈ ���� ����
    }

    void LoadAllSkillData()
    {
        // Resources ���� ������ "Data" �������� ��� SkillData ������ �ҷ��ɴϴ�.
        // �߿�: SkillData ���µ��� "Assets/Resources/Data" ������ �־�� �մϴ�.
        allGameSkills = new List<SkillData>(Resources.LoadAll<SkillData>("Data"));

        if (allGameSkills.Count == 0)
        {
            Debug.LogWarning("GameManager: Resources/Data �������� SkillData�� ã�� �� �����ϴ�.");
        }
    }



    // DiceManager.OnDiceSelected �̺�Ʈ�� �߻����� �� ȣ��� �Լ�
    void HandleDiceSelectionChanged(BodyDice selectedDice) // BodyDice �Ķ���ʹ� �̺�Ʈ �ñ״�ó�� ����
    {
        if (skillPanel_UI != null)
        {
            skillPanel_UI.RefreshSkillInfoPanelWithCurrentDiceState();
        }
    }

    

    // RollButton�� OnClick �̺�Ʈ�� ����� �Լ� (����)
    public void OnRollDiceButtonClicked()
    {
        if (DiceManager.Instance != null)
        {
            DiceManager.Instance.RollAllDices();
            // �ֻ����� ���� �� ��� ��ٷȴٰ� (�ִϸ��̼� ��) ���� �г� ������Ʈ
            // �Ǵ� RollAllDices ������ �ڷ�ƾ �������� �̺�Ʈ �߻����� ó��
            if (skillPanel_UI != null)
            {
                // �ٷ� ������Ʈ�ϰų�, �ֻ��� �� �ִϸ��̼� �Ϸ� �� ������Ʈ
                skillPanel_UI.RefreshSkillInfoPanelWithCurrentDiceState();
            }
        }
    }

    public void StartNewStage()
    {
        ClearOldEnemies(); // ���� �������� ���� ���� (�ʿ��)
        SpawnEnemies();
        InitializeGauge(); // �� �������� ���� �� ������ �ʱ�ȭ
        UpdateGaugeUI();   // ������ UI�� ������Ʈ
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
        currentTargetEnemy = null; // ���� Ÿ�ٵ� �ʱ�ȭ
        currentSelectedSkillForAttack = null; // ���õ� ��ų�� �ʱ�ȭ
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogError("Enemy Spawn Points�� �������� �ʾҽ��ϴ�!");
            // �ӽ÷� �⺻ ��ġ�� �����ϰų�, ���� ó��
            // Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
            return;
        }

        int numberOfEnemies = Random.Range(minEnemiesPerStage, maxEnemiesPerStage + 1);
        Debug.Log($"�̹� �������� �� ���� ��: {numberOfEnemies}");


        for (int i = 0; i < numberOfEnemies; i++)
        {
            // ���� ��ġ �����ϰ� ���� (�Ǵ� ����������)
            Transform spawnPoint = enemySpawnPoints[i];
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            Enemy newEnemy = enemyGO.GetComponent<Enemy>();
            if (newEnemy != null)
            {
                activeEnemies.Add(newEnemy);
                enemyGO.name = $"Enemy_{i + 1}"; // �����ϱ� ������ �̸� ����
            }
        }
        // ù ��° ���� �⺻ Ÿ������ ���� (���� ����)
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

        if (currentTargetEnemy == deadEnemy) // ���� ���� ���� Ÿ���̾��ٸ� Ÿ�� ���� �Ǵ� ����
        {
            currentTargetEnemy = null;
            if (activeEnemies.Count > 0) SelectTargetEnemy(activeEnemies[0]); // �ٸ� ���� ���� Ÿ������
        }

        CheckStageClear();
    }

    void CheckStageClear()
    {
        if (activeEnemies.Count == 0)
        {
            Debug.Log("�������� Ŭ����! ��� ���� �����ƽ��ϴ�.");
            // ���� ���������� �̵��ϰų�, ���� Ŭ���� ó��
            // ��: Invoke("StartNewStage", 2f); // 2�� �� ���� �������� ����
        }
    }

    void InitializeGauge()
    {
        currentGaugeFillCount = 0;
        currentGaugePoints = 0;
        // UpdateGaugeUI(); // Start���� ȣ���ϹǷ� �ߺ� ȣ�� ����
    }


    public void AddGauge(int amount)
    {
        if (currentGaugePoints >= maxGaugePoints && currentGaugeFillCount >= gaugeBarsPerPoint - 1)
        {
            Debug.Log("�׼� �������� �̹� �ִ�� ���� á���ϴ�.");
            return;
        }

        currentGaugeFillCount += amount;
        Debug.Log($"������ {amount} ����. ���� ĭ: {currentGaugeFillCount}");

        while (currentGaugeFillCount >= gaugeBarsPerPoint)
        {
            if (currentGaugePoints < maxGaugePoints)
            {
                currentGaugeFillCount -= gaugeBarsPerPoint;
                currentGaugePoints++;
                Debug.Log($"������ ����Ʈ 1 ����! ���� ����Ʈ: {currentGaugePoints}, ���� ĭ: {currentGaugeFillCount}");
            }
            else
            {
                currentGaugeFillCount = gaugeBarsPerPoint - 1;
                // �ִ� ����Ʈ������ ������ ĭ(9��° �ε���)������ ä���� ������ ����. 10ĭ°�� �ð������� 0���� ���ư��°� �ƴ϶� 9ĭ���� ����.
                // ���� 10ĭ�� �� ���� 0���� ���ư��鼭 ���ڰ� 1�����ϴ� ǥ���� ���ߴٸ�,
                // currentGaugeFillCount = 0; ���� �ϰ�, �ִ� ����Ʈ�϶��� currentGaugeFillCount�� gaugeBarsPerPoint-1�� ���� �ʵ��� �����ؾ���.
                // ���� ������ �ִ� ����Ʈ�϶� 10ĭ°�� ���� �׳� 9ĭ���� ���ߴ� ���.
                Debug.Log("�ִ� ������ ����Ʈ�� �����߽��ϴ�. �� �̻� ����Ʈ�� �������� �ʽ��ϴ�.");
                break;
            }
        }
        // currentGaugeFillCount�� ������ ���� �ʵ��� ���� (�ʿ��� ���)
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
            // currentGaugeFillCount�� �������� ����. ���� AddGauge �� ���� ����Ʈ �������� �̾ ��.
            // �Ǵ�, ����Ʈ�� �پ��� currentGaugeFillCount�� 10ĭ(gaugeBarsPerPoint-1)���� �� ���� ������ ������ ���� ����.
            // ���� �䱸����("1����Ʈ 3������ĭ�� ä�����°ɷ� ���̴°� �ٲ��߰���?")�� fillCount�� �����Ǵ� ���� �ǹ�.
            Debug.Log($"������ ����Ʈ {pointsToUse} ���. ���� ����Ʈ: {currentGaugePoints}, ���� ĭ: {currentGaugeFillCount}");
            UpdateGaugeUI();
            return true;
        }
        Debug.LogWarning("������ ����Ʈ�� �����մϴ�!");
        return false;
    }

    void UpdateGaugeUI()
    {
        if (gaugeBars == null || gaugeBars.Length == 0)
        {
            Debug.LogWarning("GaugeBars �迭�� �Ҵ���� �ʾҰų� ����ֽ��ϴ�.");
            return;
        }
        if (gaugeBars.Length != gaugeBarsPerPoint)
        {
            // �� ���� UI ������ ���� �޶��� �� �����Ƿ�, �ϴ� �ּ� ó���ϰų� �ʿ信 �°� �����մϴ�.
            // Debug.LogWarning($"GaugeBars �迭�� ũ��({gaugeBars.Length})�� gaugeBarsPerPoint({gaugeBarsPerPoint})�� ��ġ���� �ʽ��ϴ�.");
        }


        for (int i = 0; i < gaugeBars.Length; i++) // UI ������ �ٴ� 0~9 �ε���
        {
            if (gaugeBars[i] == null) continue;

            if (i < currentGaugeFillCount) // ���� ����Ʈ �������� ä������ �ִ� ĭ��
            {
                switch (currentGaugePoints)
                {
                    case 0: gaugeBars[i].color = gaugePoint0Color; break;
                    case 1: gaugeBars[i].color = gaugePoint1Color; break;
                    case 2: gaugeBars[i].color = gaugePoint2Color; break;
                    case 3: gaugeBars[i].color = gaugePoint3Color; break;
                    default: gaugeBars[i].color = gaugeEmptyColor; break; // Ȥ�� �� ��Ȳ
                }
            }
            else // ���� ����Ʈ �������� ���� ä������ ���� ĭ��
            {
                // �� ĭ���� ���� ����Ʈ ������ �������� ä���� �־�� �� (���� ���� ����Ʈ�� �����Ѵٸ�)
                // �Ǵ� ������ ����־�� �� (currentGaugePoints == 0 �� ��)
                if (currentGaugePoints == 0) // 0����Ʈ�̰�, ���� fill count ���� ĭ���� �� ĭ
                {
                    gaugeBars[i].color = gaugeEmptyColor;
                }
                else if (currentGaugePoints == 1) // 1����Ʈ�̰�, ���� fill count ���� ĭ���� 0����Ʈ ��(�ʷ�)���� ä���� �־�� ��
                {
                    gaugeBars[i].color = gaugePoint0Color;
                }
                else if (currentGaugePoints == 2) // 2����Ʈ�̰�, ���� fill count ���� ĭ���� 1����Ʈ ��(���)���� ä���� �־�� ��
                {
                    gaugeBars[i].color = gaugePoint1Color;
                }
                else if (currentGaugePoints == 3) // 3����Ʈ�̰�, ���� fill count ���� ĭ���� 2����Ʈ ��(����)���� ä���� �־�� ��
                {
                    gaugeBars[i].color = gaugePoint2Color;
                }
                else // �� ���� ���� �� ĭ (�̷л� currentGaugePoints�� 0~3 ����)
                {
                    gaugeBars[i].color = gaugeEmptyColor;
                }
            }
        }

        // ����Ʈ �Ҹ� �� ���� �ݿ�:
        // ���� 2����Ʈ 3ĭ�̾��ٸ�: currentGaugePoints = 2, currentGaugeFillCount = 3
        // �� 0,1,2�� gaugePoint2Color (����)
        // �� 3~9�� gaugePoint1Color (���) - �� ������ �̷��� �����ؾ� ��.

        // ���� ������ ���� (����� ����: "2����Ʈ 3ĭ�̸� ������ 3��, ����� 7��" �ݿ�)
        for (int i = 0; i < gaugeBars.Length; i++)
        {
            if (gaugeBars[i] == null) continue;

            if (i < currentGaugeFillCount) // ���� ����Ʈ �������� ä������ �ִ� �κ�
            {
                gaugeBars[i].color = GetColorForPointLevel(currentGaugePoints);
            }
            else // ���� ����Ʈ �������� ���������, ���� ����Ʈ ������ ä������ �ϴ� �κ�
            {
                if (currentGaugePoints > 0) // ���� ����Ʈ�� 0���� Ŀ�� ���� ������ ����
                {
                    gaugeBars[i].color = GetColorForPointLevel(currentGaugePoints - 1);
                }
                else // ���� ����Ʈ�� 0�̰�, currentGaugeFillCount ���� ĭ���� �� ĭ
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

    // Ư�� ����Ʈ ������ �´� ������ ��ȯ�ϴ� ���� �Լ�
    private Color GetColorForPointLevel(int pointLevel)
    {
        switch (pointLevel)
        {
            case 0: return gaugePoint0Color;
            case 1: return gaugePoint1Color;
            case 2: return gaugePoint2Color;
            case 3: return gaugePoint3Color; // 3����Ʈ �Ǵ� �� �̻� (�ִ� 3������)
            default: return gaugeEmptyColor; // ���� ����Ʈ �Ǵ� ���� ��Ȳ
        }
    }



    // SkillPanel���� ȣ���Ͽ� �÷��̾ ����� ��ų�� ����
    public void SetSelectedSkillForAttack(SkillData skill)
    {
        currentSelectedSkillForAttack = skill;
        Debug.Log($"���� ��ų �غ�: {skill?.skillName ?? "����"}");

        // ��ų ���� �� ��ų ���� �гε� ������Ʈ (SkillPanel�� �̹� ���� ������ �ִٸ� �ߺ� ȣ�� ����)
        // skillPanel_UI?.UpdateSkillInfoPanelWithDice(skill);
        // �Ǵ� SkillPanel�� SetCurrentSkillForInfoPanel�� ȣ���Ͽ� �ϰ��� ����
        skillPanel_UI?.SetCurrentSkillForInfoPanel(skill);

        // ���� ��ų ���� �� �ٷ� Ÿ���� �����ϴ� ����̶�� ���⼭ �߰� ����
    }
    // Enemy�� OnMouseDown���� ȣ��Ǿ� Ÿ���� �����ϰ�, ������ ������ ���� ����
    public void EnemyClicked(Enemy clickedEnemy)
    {
        currentTargetEnemy = clickedEnemy; // �׻� Ŭ���� ���� ���� Ÿ������ ����
        Debug.Log($"Ÿ�� ����: {clickedEnemy.gameObject.name}");

        // (���û���) Ÿ�� �ð��� �ǵ�� (��: ��� ���� ���̶���Ʈ ���� ��, ���� Ÿ�ٸ� ���̶���Ʈ)
        // foreach(var enemy in activeEnemies) { enemy.SetHighlight(false); }
        // clickedEnemy.SetHighlight(true);


        if (currentSelectedSkillForAttack != null)
        {
            // �ֻ����� ��������, ���� �ֻ����� ���õǾ����� Ȯ��
            if (DiceManager.Instance != null && DiceManager.Instance.GetSelectedNumberDice() != null)
            {
                Debug.Log($"{clickedEnemy.gameObject.name}���� {currentSelectedSkillForAttack.skillName} ��ų ��� �õ�.");
                AttemptAttackOnEnemy(clickedEnemy);
            }
            else
            {
                Debug.LogWarning("��ų�� ����Ϸ��� ���� �ֻ����� ������ ���� �ֻ����� �����ؾ� �մϴ�.");
                // ���⿡ ����ڿ��� �˸� UI ǥ�� ���� �߰� ����
            }
        }
        else
        {
            Debug.Log("���õ� ���� ��ų�� �����ϴ�. ���� ��ų�� �������ּ���.");
            // ���⿡ ����ڿ��� �˸� UI ǥ�� ���� �߰� ����
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
                        // ���ϱ�, ������ ��
                }
            }
            else
            {
                Debug.LogWarning("���õ� ���� �ֻ��� ���� �����ϴ�. �⺻ �������� ����մϴ�.");
                // calculationDetailsForUI�� �̹� baseDamage�� ������
            }
        }
        else
        {
            Debug.LogWarning("DiceManager �ν��Ͻ��� ã�� �� �����ϴ�. �⺻ �������� ����մϴ�.");
        }
        finalDamage = Mathf.Max(0, finalDamage); // �������� ������ ���� �ʵ���
        return true;
    }

    private void AttemptAttackOnEnemy(Enemy targetEnemy)
    {
        if (currentSelectedSkillForAttack == null || targetEnemy == null)
        {
            Debug.LogError("��ų �Ǵ� Ÿ���� �������� �ʾҽ��ϴ�.");
            return;
        }

        if (currentSelectedSkillForAttack.gaugePointCost > 0) // ������ ����Ʈ�� �Ҹ��ϴ� ��ų�̶��
        {
            if (!TryUseGaugePoints(currentSelectedSkillForAttack.gaugePointCost))
            {
                Debug.LogWarning($"{currentSelectedSkillForAttack.skillName} ��� ����: ������ ����Ʈ ����!");
                // ����ڿ��� �˸� (��: UI �޽���)
                return; // ���� �ߴ�
            }
        }

        int calculatedDamage;
        string calcDetails; // UI ǥ�ÿ� ��� ���� (SkillPanel���� �̹� �����ϰ� ó�� ���̹Ƿ� ���⼱ �α׿����θ� ����ϰų� UI�� ����)

        if (TryCalculateFinalDamage(currentSelectedSkillForAttack, out calculatedDamage, out calcDetails))
        {
            Debug.Log($"{targetEnemy.name}���� {currentSelectedSkillForAttack.skillName} ���! ���� ������: {calculatedDamage} (���: {calcDetails.Replace("<color=green>", "").Replace("<color=red>", "").Replace("</color>", "")})"); // �α׿��� ���� �ڵ� ����
            targetEnemy.TakeDamage(calculatedDamage);
            if (currentSelectedSkillForAttack.isBasicAttack)
            {
                AddGauge(currentSelectedSkillForAttack.gaugeChargeAmount);
                Debug.Log("AddGauge ȣ���. ������: " + currentSelectedSkillForAttack.gaugeChargeAmount + ", ���� ä���� ĭ: " + currentGaugeFillCount + ", ���� ����Ʈ: " + currentGaugePoints); // �α� �߰�
            }

            // ���� �� ó��
            Debug.Log($"{currentSelectedSkillForAttack.skillName} ��� �Ϸ�.");
            currentSelectedSkillForAttack = null; // ����� ��ų ���� ����

            // ����� �ֻ��� ��Ȱ��ȭ �Ǵ� ���� ���� (DiceManager�� ���� ��� �߰� �ʿ� ��)
            // DiceManager.Instance.GetSelectedNumberDice()?.LockDice(); // ����: �ֻ��� ���
            DiceManager.Instance.DeselectAll(); // ���� �ֻ��� ���� ����

            // ��ų ���� �гο��� ���õ� ��ų ���� �ʱ�ȭ (�Ǵ� ���� �ൿ �ȳ�)
            skillPanel_UI?.SetCurrentSkillForInfoPanel(null); // ���� �г� ���� �Ǵ� �⺻ ���·�
                                                              // �Ǵ� skillPanel_UI.RefreshSkillInfoPanelWithCurrentDiceState(); ȣ���Ͽ� �ֻ��� ���� �����

            // �� ���� ���� �Ǵ� ���� �ൿ ��� ���·� ��ȯ
            // ��: EndPlayerTurn();
        }
        else
        {
            Debug.LogError("������ ��꿡 �����߽��ϴ�.");
        }



    }
    // GameManager.ExecuteAttackOnTarget(int damage) �Լ��� ���� AttemptAttackOnEnemy ���� �������� ���յǾ����Ƿ�,
    // �������� public ExecuteAttackOnTarget�� �ʿ� ���� �� �ֽ��ϴ�. ���� �ٸ� ������ ���� ��������ŭ �����ϴ� ����� �ʿ��ϴٸ� �����մϴ�.

    // �÷��̾ ���� �����ϴ� ���� (Enemy.cs�� OnMouseDown ��� ȣ�� ����)
    public void SelectTargetEnemy(Enemy enemy)
    {
        // ���� Ÿ�� �ܰ��� ���� (�ʿ��)
        // if (currentTargetEnemy != null) currentTargetEnemy.GetComponent<Outline>()?.enabled = false;

        currentTargetEnemy = enemy;
        Debug.Log($"���� Ÿ��: {currentTargetEnemy.gameObject.name}");

        // �� Ÿ�� �ܰ��� ǥ�� (�ʿ��)
        // if (currentTargetEnemy != null) currentTargetEnemy.GetComponent<Outline>()?.enabled = true;

        // UI�� ���� Ÿ�� ���� ������Ʈ ��
    }

    // ���� ���� ���� �Լ� (Skill ��� �� ȣ��)
    public void ExecuteAttackOnTarget(int damage)
    {
        if (currentTargetEnemy != null && currentTargetEnemy.currentHp > 0)
        {
            Debug.Log($"{currentTargetEnemy.name}���� {damage} ������ ����!");
            currentTargetEnemy.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning("���� ����� ���ų� �̹� �׾����ϴ�.");
            // ���� ����� �ٽ� �����ϵ��� ����
            if (activeEnemies.Count > 0) SelectTargetEnemy(activeEnemies[0]);
        }
    }


}