using UnityEngine;

[CreateAssetMenu(menuName = "TheRoom/Interaction Feedback Settings")]
public class RoomFeedbackSettings : ScriptableObject
{
    public Material particleMaterial;
    public Color buttonColor = new Color(0.15f, 0.7f, 1f, 1f);
    public Color shotColor = new Color(0.4f, 0.85f, 1f, 1f);
    [Range(1, 32)] public int buttonParticles = 12;
    [Range(1, 32)] public int shotParticles = 8;
    public float particleLifetime = 0.3f;
    public float particleSize = 0.025f;
    public float particleSpeed = 0.35f;
    [Range(0f, 1f)] public float clickVolume = 0.45f;
    [Range(0f, 1f)] public float shotVolume = 0.3f;
    public float minDistance = 1f;
    public float maxDistance = 18f;
}
