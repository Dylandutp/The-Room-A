using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RoomFeedback
{
    private static RoomFeedbackSettings settings;
    private static AudioClip clickClip, shotClip;

    public static void PlayShot(Transform muzzle)
    {
        if (muzzle != null) Play(muzzle.position, muzzle.rotation, true);
    }

    public static void Play(Vector3 position, Quaternion rotation, bool shot)
    {
        if (settings == null) settings = Resources.Load<RoomFeedbackSettings>("Feedback/Settings");
        if (settings == null) { Debug.LogError("Missing Feedback/Settings asset"); return; }
        if (clickClip == null) clickClip = Resources.Load<AudioClip>("Feedback/Click");
        if (shotClip == null) shotClip = Resources.Load<AudioClip>("Feedback/Shot");
        GameObject effect = new GameObject(shot ? "Shot Feedback" : "Button Feedback");
        // World-space root avoids the very small scale of world-space UI canvases.
        effect.transform.SetPositionAndRotation(position, rotation);
        var ps = effect.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false; main.loop = false; main.duration = 0.5f;
        main.startLifetime = Mathf.Max(0.05f, settings.particleLifetime);
        main.startSpeed = settings.particleSpeed * (shot ? 2f : 1f);
        main.startSize = settings.particleSize;
        main.startColor = shot ? settings.shotColor : settings.buttonColor;
        main.maxParticles = 32;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;
        var emission = ps.emission; emission.enabled = false;
        var shape = ps.shape; shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = shot ? 12f : 65f; shape.radius = shot ? 0.015f : 0.035f;
        var color = ps.colorOverLifetime; color.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        color.color = fade;
        var size = ps.sizeOverLifetime; size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.1f));
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = settings.particleMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        ps.Play(); ps.Emit(Mathf.Clamp(shot ? settings.shotParticles : settings.buttonParticles, 1, 32));
        var source = effect.AddComponent<AudioSource>();
        source.playOnAwake = false; source.loop = false;
        source.spatialBlend = 1f; source.dopplerLevel = 0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = Mathf.Max(0.1f, settings.minDistance);
        source.maxDistance = Mathf.Max(source.minDistance + 0.1f, settings.maxDistance);
        source.volume = shot ? settings.shotVolume : settings.clickVolume;
        source.clip = shot ? shotClip : clickClip;
        if (source.clip != null) source.Play();
        effect.AddComponent<RoomFeedbackLifetime>().seconds = Mathf.Max(
            Mathf.Max(0.05f, settings.particleLifetime), source.clip != null ? source.clip.length : 0f) + 0.15f;
    }
}
