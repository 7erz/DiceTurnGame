// EnemyHealthBar.cs (수정된 버전)
using UnityEngine;
using UnityEngine.UI; // Slider 사용

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    private Enemy enemyData; // HealthSystem 대신 Enemy 참조
    private Camera mainCamera;
    // private Transform parentTransform; // Enemy의 Transform은 enemyData.transform으로 접근 가능

    [Tooltip("체력 바가 항상 보일지, 아니면 피해를 입었을 때만 보일지")]
    public bool alwaysVisible = false;
    [Tooltip("피해 시 표시될 시간 (0 이하면 항상 표시)")]
    public float visibleTime = 3f;
    private float lastHitTime = -10f;

    void Awake()
    {
        enemyData = GetComponentInParent<Enemy>(); // 부모에서 Enemy 컴포넌트 찾기
        if (enemyData == null)
        {
            Debug.LogError("EnemyHealthBar: 부모에서 Enemy 컴포넌트를 찾을 수 없습니다!", gameObject);
            gameObject.SetActive(false);
            return;
        }

        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
            if (healthSlider == null)
            {
                Debug.LogError("EnemyHealthBar: Health Slider를 찾거나 할당해야 합니다!", gameObject);
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
            enemyData.OnHealthChanged += UpdateHealthBar; // Enemy의 체력 변경 이벤트 구독
            enemyData.OnDiedInstance += HandleDeath;      // Enemy의 인스턴스 사망 이벤트 구독

            // 초기 상태 업데이트 (Enemy의 Start 이후 호출되도록 순서 주의 필요, 또는 Enemy에서 직접 초기값 전달)
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

    void UpdateHealthBar(int currentHealth, int maxHealth) // 파라미터 타입을 int로 변경
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
            if (currentHealth < maxHealth && currentHealth > 0) // 피해를 입었고 살아있을 때
            {
                lastHitTime = Time.time;
            }
            UpdateVisibility();
        }
    }

    void HandleDeath()
    {
        gameObject.SetActive(false); // 체력 바 숨기기
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
        // 부모(적)의 위치를 따라다니는 것은 EnemyHealthBar 오브젝트가 Enemy의 자식으로 올바르게 배치되었다면 자동으로 처리될 수 있습니다.
        // 또는 Canvas가 World Space이고, EnemyHealthBar가 Enemy의 자식이 아니라면 여기서 위치를 수동으로 업데이트해야 합니다.
        // 현재 코드는 부모 Transform을 직접 따라가는 부분이 주석 처리되어 있습니다.

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