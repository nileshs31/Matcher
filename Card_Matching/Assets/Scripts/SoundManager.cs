using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioClip flipClip;
    [SerializeField] private AudioClip matchClip;
    [SerializeField] private AudioClip mismatchClip;
    [SerializeField] private AudioClip gameOverClip;

    private AudioSource _source;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
    }

    public void PlayFlip() => Play(flipClip);
    public void PlayMatch() => Play(matchClip);
    public void PlayMismatch() => Play(mismatchClip, 0.15f);
    public void PlayGameOver() => Play(gameOverClip);

    private void Play(AudioClip clip, float sfxVolume = 1)
    {
        if (clip == null) return;
        _source.PlayOneShot(clip, sfxVolume);
    }
}
