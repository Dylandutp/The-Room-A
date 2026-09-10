using UnityEngine;

public class Comet : MonoBehaviour
{
    const float gravity = 1.5f;
    public Transform planet;
    public Vector3 initVelo = new Vector3(0f, 0f, 0.5f);
    private Vector3 velocity;
    private Vector3 initPos;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initPos = transform.position;
        velocity = initVelo;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position - planet.position;
        // Geometrical distance
        float distance = Mathf.Sqrt( Mathf.Pow(position.x, 2) + Mathf.Pow(position.y, 2) + Mathf.Pow(position.z, 2) );
        if (distance < 0.3f || distance > 4f)  // too close to the planet
        {
            transform.position = initPos;
            velocity = initVelo;
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
