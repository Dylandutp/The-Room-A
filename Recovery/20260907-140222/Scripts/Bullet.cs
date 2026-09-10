using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public Vector3 velocity;
    private Transform planet;
    private float gravity;
    private bool initialized;

    public void Initialize(Transform attractor, float strength, Vector3 initialVelocity)
    {
        planet = attractor;
        gravity = strength;
        velocity = initialVelocity;
        initialized = true;
    }
    void Start()
    {
        if (!initialized) velocity = transform.forward * speed;
    }
    void Update()
    {
        // Small integration steps reduce orbit drift when rendering frames vary.
        int steps = Mathf.Max(1, Mathf.CeilToInt(Time.deltaTime / 0.005f));
        float dt = Time.deltaTime / steps;
        for (int i = 0; i < steps; i++)
        {
            if (planet != null)
            {
                Vector3 offset = transform.position - planet.position;
                float distance = offset.magnitude;
                if (distance < 0.3f) { Destroy(gameObject); return; }
                velocity += -gravity * offset / (distance * distance * distance) * dt;
            }
            transform.position += velocity * dt;
        }
    }
}
