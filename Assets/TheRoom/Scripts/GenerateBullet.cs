using UnityEngine;
using UnityEngine.InputSystem;

public class GenerateBullet : MonoBehaviour
{
    public InputActionReference inputAction;
    public GameObject bulletPrefab;
    public Transform initPos;
    public Transform planet;
    public float fireInterval = 0.2f;
    public bool assist = false;
    public Transform outline;
    public ParticleSystem shotParticles;
    public AudioSource shotAudio;
    private float next;

    void Start()
    {
        inputAction.action.Enable();
    }

    void Update()
    {
        if(inputAction.action.IsPressed() && Time.time >= next)
        {
            if (assist) assistShoot();
            else shoot();
            next = Time.time + fireInterval;
        }
    }


    GameObject createBullet()
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            initPos.position,
            initPos.rotation
        );
        bullet.GetComponent<Bullet>().planet = planet;
        shotParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        shotParticles.Play();
        shotAudio.transform.position = initPos.position;
        shotAudio.PlayOneShot(shotAudio.clip);
        Destroy(bullet, 30f);

        return bullet;
    }

    void shoot()
    {
        GameObject bullet = createBullet();
        Destroy(bullet, 5f);
    }


    void assistShoot()
    {
        GameObject bullet = createBullet();
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        Vector3 vector = bullet.transform.position - bulletScript.planet.position;
        float distance = Mathf.Sqrt( Mathf.Pow(vector.x, 2) + Mathf.Pow(vector.y, 2) + Mathf.Pow(vector.z, 2) );
        if (distance <= 0.3f) return;
        Vector3 unitVector = vector / distance;
        Vector3 tangent = Vector3.ProjectOnPlane(bullet.transform.forward, unitVector);
        bullet.transform.rotation = Quaternion.LookRotation(tangent.normalized, unitVector);
        bulletScript.speed = Mathf.Sqrt(bulletScript.gravity / distance);
    }


    public void switchMode()
    {
        assist = !assist;
        outline.gameObject.SetActive(!assist);
    }
}
