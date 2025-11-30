using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 6f;
    public int damage = 1;
    public bool useRigidbody = true;

    private Vector3 direction;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 dir, float overrideSpeed = -1f)
    {
        direction = dir.normalized;
        if (overrideSpeed > 0) speed = overrideSpeed;

        if (useRigidbody && rb != null)
        {
            rb.velocity = direction * speed;
            rb.isKinematic = false;
        }
        else
        {
            // если Rigidbody нет или не хотим его использовать, то просто двигаем вручную
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if ((!useRigidbody) || rb == null)
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        // поворачиваем по направлению движения (если скорость ненулевая)
        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnCollisionEnter(Collision collision)
    {
        // попадание в игрока
        if (collision.collider.CompareTag("Player"))
        {
            var hp = collision.collider.GetComponent<PlayerHalth>();
            if (hp != null) hp.TakeDamage(damage);
        }

        // можно зафиксировать стрелу в объекте:
        // rb.isKinematic = true;
        // rb.velocity = Vector3.zero;
        Destroy(gameObject);
    }

    // если используешь триггер-коллайдер:
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var hp = other.GetComponent<PlayerHalth>();
            if (hp != null) hp.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
