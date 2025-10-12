using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("AudioSources")]
    public AudioSource dashClip;
    public AudioSource jumpClip;
    public AudioSource landClip;
    public AudioSource teleportInClip;
    public AudioSource teleportOutClip;
    public AudioSource deathClip;
    public AudioSource orbCollectClip;
    public AudioSource grappleClip;
    public AudioSource shrinkClip;
    public AudioSource unshrinkClip;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void Play(AudioSource source, float volume = 1f, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (source == null || source.clip == null) return;

        source.pitch = Random.Range(pitchMin, pitchMax);
        source.PlayOneShot(source.clip, volume);
    }
}
