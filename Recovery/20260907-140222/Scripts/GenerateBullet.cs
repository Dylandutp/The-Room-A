using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GenerateBullet : MonoBehaviour
{
    public InputActionReference inputAction;
    public GameObject bulletPrefab;
    public Transform initPos;
    public Transform planet;
    public Text modeLabel;
    public float fireInterval = 0.2f;
    public float gravity = 1.5f;
    public float bulletLifetime = 60f;
    public bool orbitAssist;
    private float next;
    private bool enabledHere;

    void OnEnable()
    {
        if (inputAction != null) { enabledHere = !inputAction.action.enabled; inputAction.action.Enable(); }
        RefreshLabel();
    }
    void OnDisable()
    {
        if (enabledHere && inputAction != null) inputAction.action.Disable();
    }
    void Update()
    {
        if (inputAction != null && inputAction.action.IsPressed() && Time.time >= next)
        {
            shoot();
            next = Time.time + Mathf.Max(0.05f, fireInterval);
        }
    }
    public void ToggleOrbitAssist()
    {
        orbitAssist = !orbitAssist;
        RefreshLabel();
    }
    public void RefreshLabel()
    {
        if (modeLabel != null) modeLabel.text = orbitAssist ? "MODE: ORBIT ASSIST" : "MODE: NORMAL AIM";
    }
    public static Vector3 OrbitVelocity(Vector3 offset, Vector3 forward, Vector3 fallback, float strength)
    {
        Vector3 radial = offset.normalized;
        Vector3 tangent = Vector3.ProjectOnPlane(forward, radial);
        if (tangent.sqrMagnitude < 0.0001f) tangent = Vector3.ProjectOnPlane(fallback, radial);
        if (tangent.sqrMagnitude < 0.0001f) tangent = Vector3.Cross(radial, Mathf.Abs(radial.y) < 0.9f ? Vector3.up : Vector3.right);
        return tangent.normalized * Mathf.Sqrt(strength / offset.magnitude);
    }
    public void shoot()
    {
        if (bulletPrefab == null || initPos == null || planet == null || gravity <= 0) return;
        Vector3 offset = initPos.position - planet.position;
        if (offset.magnitude < 0.3f) return;
        var instance = Instantiate(bulletPrefab, initPos.position, initPos.rotation);
        var bullet = instance.GetComponent<Bullet>();
        if (bullet == null) { Debug.LogError("Bullet prefab needs a Bullet component.", this); Destroy(instance); return; }
        Vector3 initialVelocity = orbitAssist
            ? OrbitVelocity(offset, initPos.forward, initPos.up, gravity)
            : initPos.forward * bullet.speed;
        bullet.Initialize(planet, gravity, initialVelocity);
        float period = 2f * Mathf.PI * Mathf.Sqrt(Mathf.Pow(offset.magnitude, 3) / gravity);
        Destroy(instance, orbitAssist ? Mathf.Max(bulletLifetime, period * 1.2f) : bulletLifetime);
    }
}
