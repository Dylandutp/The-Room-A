using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RoomButtonFeedback : MonoBehaviour
{
    private Button button;
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayFeedback);
    }
    void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(PlayFeedback);
    }
    public void PlayFeedback()
    {
        if (!button.IsActive() || !button.IsInteractable()) return;
        Vector3 position = transform.position;
        var rect = transform as RectTransform;
        if (rect != null) position = rect.TransformPoint(rect.rect.center);
        RoomFeedback.Play(position - transform.forward * 0.035f,
            Quaternion.LookRotation(-transform.forward, transform.up), false);
    }
}
