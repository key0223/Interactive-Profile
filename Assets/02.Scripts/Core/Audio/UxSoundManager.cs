using System;
using System.Collections.Generic;
using UnityEngine;

public class UxSoundManager : MonoBehaviour
{
    [Serializable]
    private class UxSoundEntry
    {
        public UxSoundType Type = UxSoundType.RoomBgm;
        public AudioClip Clip = null;
        [Range(0f, 1f)] public float Volume = 1f;
    }

    [SerializeField] private AudioSource _bgmAudioSource;
    [SerializeField] private AudioSource _sfxAudioSource;
    [SerializeField, Range(0f, 1f)] private float _masterBgmVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _masterSfxVolume = 1f;
    [SerializeField] private UxSoundEntry[] _sounds;
    [SerializeField] private bool _warnWhenDuplicateType;

    private readonly Dictionary<UxSoundType, UxSoundEntry> _soundMap = new Dictionary<UxSoundType, UxSoundEntry>();

    public static UxSoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{nameof(UxSoundManager)} duplicate on {name} will be disabled. Keep one active sound manager per scene.");
            enabled = false;
            return;
        }

        Instance = this;
        RebuildSoundMap();
        ConfigureAudioSources();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void Play(UxSoundType type)
    {
        if (Instance == null)
            return;

        Instance.PlaySfx(type);
    }

    public static void PlayBgm(UxSoundType type)
    {
        if (Instance == null)
            return;

        Instance.PlayBgmInternal(type);
    }

    public static void StopBgm()
    {
        if (Instance == null)
            return;

        Instance.StopBgmInternal();
    }

    public void PlaySfx(UxSoundType type)
    {
        if (_sfxAudioSource == null)
            return;

        if (!TryGetPlayableEntry(type, out UxSoundEntry entry))
            return;

        _sfxAudioSource.PlayOneShot(entry.Clip, Mathf.Clamp01(entry.Volume) * _masterSfxVolume);
    }

    public void PlaySystemTyping()
    {
        PlaySfx(UxSoundType.SystemTyping);
    }

    public void PlayErrorBeep()
    {
        PlaySfx(UxSoundType.ErrorBeep);
    }

    public void PlayBgmInternal(UxSoundType type)
    {
        if (_bgmAudioSource == null)
            return;

        if (!TryGetPlayableEntry(type, out UxSoundEntry entry))
            return;

        if (_bgmAudioSource.clip == entry.Clip && _bgmAudioSource.isPlaying)
            return;

        _bgmAudioSource.clip = entry.Clip;
        _bgmAudioSource.loop = true;
        _bgmAudioSource.volume = Mathf.Clamp01(entry.Volume) * _masterBgmVolume;
        _bgmAudioSource.Play();
    }

    public void StopBgmInternal()
    {
        if (_bgmAudioSource == null)
            return;

        _bgmAudioSource.Stop();
    }

    private void ConfigureAudioSources()
    {
        if (_bgmAudioSource != null)
            _bgmAudioSource.loop = true;

        if (_sfxAudioSource != null)
            _sfxAudioSource.loop = false;
    }

    private void RebuildSoundMap()
    {
        _soundMap.Clear();

        if (_sounds == null)
            return;

        for (int i = 0; i < _sounds.Length; i++)
        {
            UxSoundEntry entry = _sounds[i];

            if (_soundMap.ContainsKey(entry.Type))
            {
                if (_warnWhenDuplicateType)
                    Debug.LogWarning($"{nameof(UxSoundManager)} on {name} has duplicate {entry.Type}. The first entry will be used.");

                continue;
            }

            _soundMap.Add(entry.Type, entry);
        }
    }

    private bool TryGetPlayableEntry(UxSoundType type, out UxSoundEntry entry)
    {
        if (!_soundMap.TryGetValue(type, out entry))
            return false;

        return entry.Clip != null;
    }
}
