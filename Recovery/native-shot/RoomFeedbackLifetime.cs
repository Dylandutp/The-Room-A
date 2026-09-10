using UnityEngine;

public class RoomFeedbackLifetime : MonoBehaviour
{
    public float seconds = 1f;
    void Update()
    {
        seconds -= Time.unscaledDeltaTime;
        if (seconds <= 0f) Destroy(gameObject);
    }
}
