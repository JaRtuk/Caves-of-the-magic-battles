using UnityEngine;

public class EnemyArcher3D : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player;
    public float detectionRange = 15f;
    public LayerMask obstacleMask; // слои препятствий (стены). НЕ включай слой игрока сюда.

    [Header("Shooting Settings")]
    public GameObject arrowPrefab;
    public Transform shootPoint;
    public float shootCooldown = 1.5f;
    private float shootTimer = 0f;
    public float arrowSpeed = 20f;

    [Header("Accuracy")]
    [Range(0f,1f)] public float hitChance = 0.30f; // ~1 из 3-4

    [Header("Animation")]
    public Animator animator;
    private bool isAiming = false;

    [Header("Debug")]
    public bool debugRays = true;

    void Start()
    {
        if (player == null) Debug.LogWarning("[EnemyArcherVR] Player not assigned on " + name);
        if (shootPoint == null) Debug.LogWarning("[EnemyArcherVR] ShootPoint not assigned on " + name);
        if (arrowPrefab == null) Debug.LogWarning("[EnemyArcherVR] ArrowPrefab not assigned on " + name);
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > detectionRange)
        {
            StopAiming();
            return;
        }

        if (!CanSeePlayer())
        {
            StopAiming();
            return;
        }

        StartAiming();

        if (shootTimer >= shootCooldown)
        {
            TryShoot();
            shootTimer = 0f;
        }
    }

    bool CanSeePlayer()
    {
        if (shootPoint == null || player == null) return false;

        Vector3 origin = shootPoint.position;
        Vector3 dir = (player.position - origin).normalized;

        if (debugRays)
            Debug.DrawRay(origin, dir * detectionRange, Color.yellow, 0.2f);

        RaycastHit hit;
        // Raycast, проверяем первый попавшийся коллайдер на линии
        if (Physics.Raycast(origin, dir, out hit, detectionRange))
        {
            // Если первым коллайдером оказался игрок — видим
            if (hit.transform == player || hit.collider.CompareTag("Player"))
            {
                if (debugRays) Debug.DrawLine(origin, hit.point, Color.green, 0.2f);
                return true;
            }
            else
            {
                if (debugRays) Debug.DrawLine(origin, hit.point, Color.red, 0.2f);
                // попал во что-то другое — это препятствие
                return false;
            }
        }

        // ничем не попал — не видим
        return false;
    }

    void StartAiming()
    {
        if (!isAiming)
        {
            isAiming = true;
            if (animator) animator.SetBool("isAiming", true);
        }
    }

    void StopAiming()
    {
        if (isAiming)
        {
            isAiming = false;
            if (animator) animator.SetBool("isAiming", false);
        }
    }

    void TryShoot()
    {
        // На всякий случай защита
        if (arrowPrefab == null || shootPoint == null || player == null)
        {
            Debug.LogWarning("[EnemyArcherVR] Missing references - cannot shoot");
            return;
        }

        // Перед созданием стрелы можно проверить ещё раз линию (устойчивее)
        Vector3 origin = shootPoint.position;
        Vector3 baseDir = (player.position - origin).normalized;

        bool shouldHit = Random.value <= hitChance;

        Vector3 finalDir = baseDir;

        if (!shouldHit)
        {
            // добавляем случайное отклонение (в градусах)
            float angleOffsetY = Random.Range(-25f, 25f);
            float angleOffsetX = Random.Range(-10f, 10f); // небольшое вертикальное смещение
            Quaternion rot = Quaternion.Euler(angleOffsetX, angleOffsetY, 0f);
            finalDir = rot * baseDir;
        }

        // Instantiate стрелы
        GameObject arrowGO = Instantiate(arrowPrefab, shootPoint.position, Quaternion.LookRotation(finalDir));
        Arrow arrowScript = arrowGO.GetComponent<Arrow>();
        Rigidbody rb = arrowGO.GetComponent<Rigidbody>();

        if (arrowScript != null)
        {
            arrowScript.Initialize(finalDir, arrowSpeed);
        }
        else if (rb != null)
        {
            // fallback: если нет скрипта, выдаём скорость rigidbody
            rb.velocity = finalDir * arrowSpeed;
        }
        else
        {
            Debug.LogWarning("[EnemyArcherVR] Arrow prefab has neither Arrow3D script nor Rigidbody. It won't move.");
        }

        if (animator) animator.SetTrigger("shoot");
    }
}
