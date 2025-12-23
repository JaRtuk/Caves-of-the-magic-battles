using UnityEngine;

public class EnemyArcher : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player;
    public float detectionRange = 15f;
    public LayerMask obstacleMask; 

    [Header("Shooting Settings")]
    public GameObject arrowPrefab;
    public Transform shootPoint;
    public float shootCooldown = 1.5f;
    private float shootTimer = 0f;
    public float arrowSpeed = 20f;
    [Tooltip("Смещение при спавне стрелы вперёд, чтобы не пересекаться с коллайдером стрелка")]
    public float spawnForwardOffset = 0.5f;

    [Header("Accuracy")]
    [Range(0f, 1f)] public float hitChance = 0.30f;

    [Header("Animation")]
    public Animator animator;
    private bool isAiming = false;

    [Header("Debug")]
    public bool debugRays = true;

    private float health = 100;

    void Start()
    {
        if (player == null) Debug.LogWarning("[EnemyArcherVR] Player not assigned on " + name);
        if (shootPoint == null) Debug.LogWarning("[EnemyArcherVR] ShootPoint not assigned on " + name);
        if (arrowPrefab == null) Debug.LogWarning("[EnemyArcherVR] ArrowPrefab not assigned on " + name);
    }

    void Update()
    {
        if (health <= 0)
            Destroy(animator.gameObject);

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
        Vector3 dir = (player.position - origin);
        float distToPlayer = dir.magnitude;

        if (distToPlayer > detectionRange) return false;

        Vector3 dirNorm = dir.normalized;

        if (debugRays)
            Debug.DrawRay(origin, dirNorm * distToPlayer, Color.yellow, 0.1f);

        RaycastHit hit;
        if (Physics.Raycast(origin, dirNorm, out hit, distToPlayer, obstacleMask))
        {
            if (debugRays) Debug.DrawLine(origin, hit.point, Color.red, 0.15f);
            return false; 
        }

        if (debugRays) Debug.DrawLine(origin, origin + dirNorm * distToPlayer, Color.green, 0.15f);
        return true;
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
        if (arrowPrefab == null || shootPoint == null || player == null)
        {
            Debug.LogWarning("[EnemyArcherVR] Missing references - cannot shoot");
            return;
        }

        if (!CanSeePlayer()) return;

        Vector3 origin = shootPoint.position;
        Vector3 baseDir = (player.position - origin).normalized;

        bool shouldHit = Random.value <= hitChance;

        Vector3 finalDir = baseDir;

        if (!shouldHit)
        {
            float angleOffsetY = Random.Range(-25f, 25f);
            float angleOffsetX = Random.Range(-10f, 10f);
            Quaternion rot = Quaternion.Euler(angleOffsetX, angleOffsetY, 0f);
            finalDir = rot * baseDir;
        }

        Vector3 spawnPos = shootPoint.position + finalDir * spawnForwardOffset;

        GameObject arrowGO = Instantiate(arrowPrefab, spawnPos, Quaternion.LookRotation(finalDir));


        Collider[] ownerCols = GetComponentsInChildren<Collider>();
        Collider[] arrowCols = arrowGO.GetComponentsInChildren<Collider>();
        foreach (var a in arrowCols)
        {
            foreach (var c in ownerCols)
            {
                if (a != null && c != null)
                {
                    Physics.IgnoreCollision(a, c, true);
                }
            }
        }

        Rigidbody rb = arrowGO.GetComponent<Rigidbody>();
        Arrow arrowScript = arrowGO.GetComponent<Arrow>();

        if (arrowScript != null)
        {
            arrowScript.Initialize(finalDir, arrowSpeed);
        }
        else if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.velocity = finalDir * arrowSpeed;
        }
        else
        {
            Debug.LogWarning("[EnemyArcherVR] Arrow prefab has neither Arrow script nor Rigidbody. It won't move.");
        }

        if (animator) animator.SetTrigger("shoot");
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("Take Damage: " + damage);
    }
}