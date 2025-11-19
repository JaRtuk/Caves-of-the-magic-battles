using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Настройки мерцания")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 10f;
    public float movementAmount = 0.1f;
    
    [Header("Случайность")]
    public float noiseScale = 1f;
    public bool smoothFlicker = true;
    
    private Light candleLight;
    private Vector3 initialPosition;
    private float randomOffset;

    void Start()
    {
        candleLight = GetComponent<Light>();
        initialPosition = transform.localPosition;
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // Мерцание интенсивности
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + randomOffset, randomOffset);
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
        
        if (smoothFlicker)
        {
            candleLight.intensity = Mathf.Lerp(candleLight.intensity, targetIntensity, Time.deltaTime * flickerSpeed);
        }
        else
        {
            candleLight.intensity = targetIntensity;
        }

        // Легкое движение света
        Vector3 movement = new Vector3(
            Mathf.PerlinNoise(Time.time * flickerSpeed * 0.7f + randomOffset, randomOffset + 1) - 0.5f,
            Mathf.PerlinNoise(Time.time * flickerSpeed * 0.8f + randomOffset, randomOffset + 2) - 0.5f,
            Mathf.PerlinNoise(Time.time * flickerSpeed * 0.6f + randomOffset, randomOffset + 3) - 0.5f
        ) * movementAmount;
        
        transform.localPosition = initialPosition + movement;
    }
}