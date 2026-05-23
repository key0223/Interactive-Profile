using UnityEngine;

public class RoomBgmPlayer : MonoBehaviour
{
    [SerializeField] private UxSoundType _bgmSound = UxSoundType.RoomBgm;
    [SerializeField] private bool _playOnStart = true;
    [SerializeField] private bool _stopOnDisable;

    private void Start()
    {
        if (_playOnStart)
            Play();
    }

    private void OnDisable()
    {
        if (_stopOnDisable)
            UxSoundManager.StopBgm();
    }

    public void Play()
    {
        UxSoundManager.PlayBgm(_bgmSound);
    }
}
