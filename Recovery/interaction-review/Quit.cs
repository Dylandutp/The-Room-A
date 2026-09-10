using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Quit : MonoBehaviour
{
    public InputActionReference action;
    public AudioSource commandAudio;

    void Start()
    {
        action.action.Enable();
        action.action.performed += OnQuitPressed;
    }

    void OnDestroy()
    {
        if (action != null) action.action.performed -= OnQuitPressed;
    }

    void OnQuitPressed(InputAction.CallbackContext context)
    {
        QuitGame();
    }

    public void QuitGame()
    {
        commandAudio.PlayOneShot(commandAudio.clip);
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
