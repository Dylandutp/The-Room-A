using UnityEngine;
using UnityEngine.InputSystem;

public class GenerateBullet : MonoBehaviour
{
    public InputActionReference inputAction;
    public GameObject bulletPrefab;
    public Transform initPos;
    public Transform planet;
    public float fireInterval = 0.2f;
    private float next;

    void Start()
    {
        inputAction.action.Enable();
    }

    void Update()
    {
        if(inputAction.action.IsPressed() && Time.time >= next)
        {
            shoot();
            next = Time.time + fireInterval;
        }
    }

    void onPressed(InputAction.CallbackContext context)
    {
        shoot();
    }

    void shoot()
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            initPos.position,
            initPos.rotation
        );
        bullet.GetComponent<Bullet>().planet = planet;
        Destroy(bullet, 5f);
    }
}
