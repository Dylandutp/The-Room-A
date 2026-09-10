using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    public AudioSource commandAudio;
    public InputActionReference cameraAction;
    public Transform xrOrigin;
    public Transform arrival;

    private Vector3 returnPoint;
    private Quaternion returnRotation;
    private bool inNature;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraAction.action.Enable();
        cameraAction.action.performed += onPressed;
    }

    void onPressed(InputAction.CallbackContext context)
    {
        teleport();
        commandAudio.PlayOneShot(commandAudio.clip);
    }

    public void teleport()
    {
        if (!inNature)
        {
            returnPoint = xrOrigin.position;
            returnRotation = xrOrigin.rotation;

            xrOrigin.SetPositionAndRotation(arrival.position, arrival.rotation);
        } else
        {
            xrOrigin.SetPositionAndRotation(returnPoint, returnRotation);
        }

        inNature = !inNature;

    }
}
