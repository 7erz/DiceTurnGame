// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq ����� ���� �߰� (Take �Լ� ��)

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
        // ��Ÿ �������� ���� ���� (UI ������Ʈ ��)
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

        int calculatedDamage;
        string calcDetails; // UI ǥ�ÿ� ��� ���� (SkillPanel���� �̹� �����ϰ� ó�� ���̹Ƿ� ���⼱ �α׿����θ� ����ϰų� UI�� ����)

        if (TryCalculateFinalDamage(currentSelectedSkillForAttack, out calculatedDamage, out calcDetails))
        {
            Debug.Log($"{targetEnemy.name}���� {currentSelectedSkillForAttack.skillName} ���! ���� ������: {calculatedDamage} (���: {calcDetails.Replace("<color=green>", "").Replace("<color=red>", "").Replace("</color>", "")})"); // �α׿��� ���� �ڵ� ����
            targetEnemy.TakeDamage(calculatedDamage);

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