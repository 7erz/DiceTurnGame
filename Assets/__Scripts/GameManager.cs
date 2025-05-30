// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq ����� ���� �߰� (Take �Լ� ��)

public class GameManager : MonoBehaviour
{
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