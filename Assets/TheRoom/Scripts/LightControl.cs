using UnityEngine;
using UnityEngine.InputSystem;

public class LightControl : MonoBehaviour
{
    public AudioSource commandAudio;
    public InputActionReference lightAction;
    public Light light;
    private int lightcolor = 0;
    void Start()
    {
        light = GetComponent<Light>();
        lightAction.action.Enable();
        lightAction.action.performed += onPressed;
    }


    void onPressed(InputAction.CallbackContext context)
    {
        switchLight();
    }

    public void switchLight()
    {
        commandAudio.PlayOneShot(commandAudio.clip);
        if (lightcolor == 0){
            light.color = new Color(0.678f, 0.847f, 0.902f);
            lightcolor = 1;
        } else
        {
            light.color = Color.white;
            lightcolor = 0;
        }
    }    
}
