using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float gravity = 400f;
    public float speed = 10f;
    public Transform planet;
    private Vector3 velocity;
    private Vector3 initVelocity;

    void Start()
    {
        initVelocity = transform.forward * speed;
        velocity = initVelocity;
    }

    void Update()
    {
        Vector3 position = transform.position - planet.position;
        // Geometrical distance
        float distance = Mathf.Sqrt( Mathf.Pow(position.x, 2) + Mathf.Pow(position.y, 2) + Mathf.Pow(position.z, 2) );

        if (distance > 4)
        {
            transform.position += velocity * Time.deltaTime;
            return;
        } else if (distance < 0.3)
        {
            Destroy(gameObject);
            return;
        }

        // calculate acceleration (a)
        float ax = - gravity * position.x / Mathf.Pow(distance, 3);
        float ay = - gravity * position.y / Mathf.Pow(distance, 3);
        float az = - gravity * position.z / Mathf.Pow(distance, 3);

        // V = V0 + at
        velocity.x = velocity.x + ax * Time.deltaTime;
        velocity.y = velocity.y + ay * Time.deltaTime;
        velocity.z = velocity.z + az * Time.deltaTime;

        
        transform.position += velocity * Time.deltaTime;
    }
}
