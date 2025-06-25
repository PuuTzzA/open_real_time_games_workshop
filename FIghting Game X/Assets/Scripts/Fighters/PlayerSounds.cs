using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip dashClip;
    public AudioClip heavyClip;
    public AudioClip heavyHitClip;
    public AudioClip jabClip;
    public AudioClip jabHitClip;
    public AudioClip jumpClip;
    public AudioClip punchHitClip;

    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayDash() => Play(dashClip);
    public void PlayHeavy() => Play(heavyClip);
    public void PlayHammerHit() => Play(heavyHitClip);
    public void PlayJab() => Play(jabClip);
    public void PlayJabHit() => Play(jabHitClip);
    public void PlayJump() => Play(jumpClip);
    public void PlayPunchHit() => Play(punchHitClip);

    private void Play(AudioClip clip)
    {
        if (clip != null)
        {
            source.PlayOneShot(clip);
        }
    }
}