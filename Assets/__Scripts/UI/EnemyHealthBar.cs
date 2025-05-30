// EnemyHealthBar.cs (������ ����)
using UnityEngine;
using UnityEngine.UI; // Slider ���

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    private Enemy enemyData; // HealthSystem ��� Enemy ����
    private Camera mainCamera;
    // private Transform parentTransform; // Enemy�� Transform�� enemyData.transform���� ���� ����

    [Tooltip("ü�� �ٰ� �׻� ������, �ƴϸ� ���ظ� �Ծ��� ���� ������")]
    public bool alwaysVisible = false;
    [Tooltip("���� �� ǥ�õ� �ð� (0 ���ϸ� �׻� ǥ��)")]
    public float visibleTime = 3f;
    private float lastHitTime = -10f;

    void Awake()
    {
        enemyData = GetComponentInParent<Enemy>(); // �θ𿡼� Enemy ������Ʈ ã��
        if (enemyData == null)
        {
            Debug.LogError("EnemyHealthBar: �θ𿡼� Enemy ������Ʈ�� ã�� �� �����ϴ�!", gameObject);
            gameObject.SetActive(false);
            return;
        }

        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
            if (healthSlider == null)
            {
                Debug.LogError("EnemyHealthBar: Health Slider�� ã�ų� �Ҵ��ؾ� �մϴ�!", gameObject);
                gameObject.SetActive(false);
                return;
            }
        }
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        if (enemyData != null)
        {
            enemyData.OnHealthChanged += UpdateHealthBar; // Enemy�� ü�� ���� �̺�Ʈ ����
            enemyData.OnDiedInstance += HandleDeath;      // Enemy�� �ν��Ͻ� ��� �̺�Ʈ ����

            // �ʱ� ���� ������Ʈ (Enemy�� Start ���� ȣ��ǵ��� ���� ���� �ʿ�, �Ǵ� Enemy���� ���� �ʱⰪ ����)
            UpdateHealthBar(enemyData.currentHp, enemyData.maxHp);
        }
        lastHitTime = -10f;
        UpdateVisibility();
    }

    void OnDisable()
    {
        if (enemyData != null)
        {
            enemyData.OnHealthChanged -= UpdateHealthBar;
            enemyData.OnDiedInstance -= HandleDeath;
        }
    }

    void UpdateHealthBar(int currentHealth, int maxHealth) // �Ķ���� Ÿ���� int�� ����
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
            if (currentHealth < maxHealth && currentHealth > 0) // ���ظ� �Ծ��� ������� ��
            {
                lastHitTime = Time.time;
            }
            UpdateVisibility();
        }
    }

    void HandleDeath()
    {
        gameObject.SetActive(false); // ü�� �� �����
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
        // �θ�(��)�� ��ġ�� ����ٴϴ� ���� EnemyHealthBar ������Ʈ�� Enemy�� �ڽ����� �ùٸ��� ��ġ�Ǿ��ٸ� �ڵ����� ó���� �� �ֽ��ϴ�.
        // �Ǵ� Canvas�� World Space�̰�, EnemyHealthBar�� Enemy�� �ڽ��� �ƴ϶�� ���⼭ ��ġ�� �������� ������Ʈ�ؾ� �մϴ�.
        // ���� �ڵ�� �θ� Transform�� ���� ���󰡴� �κ��� �ּ� ó���Ǿ� �ֽ��ϴ�.

        UpdateVisibility();
    }

    void UpdateVisibility()
    {
        if (healthSlider == null || enemyData == null) return;

        if (alwaysVisible)
        {
            if (!healthSlider.gameObject.activeSelf) healthSlider.gameObject.SetActive(true);
            return;
        }

        bool shouldBeVisible = enemyData.currentHp < enemyData.maxHp && enemyData.currentHp > 0 && Time.time < lastHitTime + visibleTime;

        if (healthSlider.gameObject.activeSelf != shouldBeVisible)
        {
            healthSlider.gameObject.SetActive(shouldBeVisible);
        }
    }
}