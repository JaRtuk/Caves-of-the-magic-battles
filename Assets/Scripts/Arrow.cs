using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    public float speed = 20f;
    public float lifeTime = 10f;
    public int damage = 10;
    public bool useRigidbody = true;

    private Vector3 direction;
    private Rigidbody rb;
    private bool hasDealtDamage = false;

    // Фиксированный поворот модели стрелы
    private readonly Quaternion modelRotationFix = Quaternion.Euler(-90f, 0f, 0f);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    public void Initialize(Vector3 dir, float overrideSpeed = -1f)
    {
        direction = dir.normalized;
        if (overrideSpeed > 0) speed = overrideSpeed;

        if (useRigidbody && rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.velocity = direction * speed;
            rb.angularVelocity = Vector3.zero;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if ((!useRigidbody) || rb == null)
        {
            transform.position += direction * speed * Time.deltaTime;

            transform.rotation =
                Quaternion.LookRotation(direction) *
                modelRotationFix;
        }
        else
        {
            if (rb.velocity.sqrMagnitude > 0.01f)
            {
                transform.rotation =
                    Quaternion.LookRotation(rb.velocity) *
                    modelRotationFix;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasDealtDamage) return;
        hasDealtDamage = true;

        if (collision.collider.CompareTag("Player"))
        {
            var hp = collision.collider.GetComponent<PlayerHalth>();
            if (hp != null) hp.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamage) return;
        hasDealtDamage = true;

        if (other.CompareTag("Player"))
        {
            var hp = other.GetComponent<PlayerHalth>();
            if (hp != null) hp.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
