using UnityEngine;

[DisallowMultipleComponent]
public sealed class ComputerBootAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _bootClip;
    [SerializeField] private AudioClip _shutdownClip;
    [SerializeField, Range(0f, 1f)] private float _volume = 1f;
    [SerializeField] private bool _stopCurrentBeforePlay = true;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
            Debug.LogWarning($"{nameof(ComputerBootAudioController)} on {name} requires an {nameof(AudioSource)} reference or component.");
    }

    public void PlayBoot()
    {
        PlayOneShot(_bootClip, "boot");
    }

    public void PlayShutdown()
    {
        PlayOneShot(_shutdownClip, "shutdown");
    }

    public void Stop()
    {
        if (_audioSource != null)
            _audioSource.Stop();
    }

    private void PlayOneShot(AudioClip clip, string label)
    {
        if (_audioSource == null)
            return;

        if (clip == null)
        {
            Debug.LogWarning($"{nameof(ComputerBootAudioController)} on {name} has no {label} clip assigned.");
            return;
        }

        if (_stopCurrentBeforePlay)
            _audioSource.Stop();

        _audioSource.PlayOneShot(clip, _volume);
    }
}
