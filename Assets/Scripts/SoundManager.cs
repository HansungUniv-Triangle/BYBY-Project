using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class SoundManager : Singleton<SoundManager>
{
    public AudioSource[] _audioSources;
    public Dictionary<string, AudioClip> _audioClips;

    private readonly float initVolume = 0.5f;

    protected override void Initiate()
    {
        _audioSources = new AudioSource[(int)Sound.MaxCount];
        _audioClips = new Dictionary<string, AudioClip>();

        string[] soundNames = System.Enum.GetNames(typeof(Sound));
        for (int i = 0; i < soundNames.Length - 1; i++)
        {
            var gameObject = new GameObject(soundNames[i]);
            _audioSources[i] = gameObject.AddComponent<AudioSource>();
            _audioSources[i].volume = initVolume;
            gameObject.transform.parent = transform;
        }

        _audioSources[(int)Sound.BGM].loop = true;
    }

    // Scene이 초기화 되면 호출
    public void Clear()
    {
        foreach (var audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }

        _audioClips.Clear();
    }

    public float GetVolume(Sound type)
    {
        return _audioSources[(int)type].volume;
    }

    public void SetVolume(Sound type, float volume)
    {
        _audioSources[(int)type].volume = volume;
    }

    public void Play(string path, Sound type, float pitch = 1.0f)
    {
        var audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch);
    }

    public void Play3DSound(string path, Sound type, Vector3 position)
    {
        var audioClip = GetOrAddAudioClip(path, type);
        if (audioClip == null)
            return;
        AudioSource.PlayClipAtPoint(audioClip, position, _audioSources[(int)type].volume);
    }

    public void Play(AudioClip audioClip, Sound type, float pitch = 1.0f)
    {
        if (audioClip == null)
        {
            return;
        }

        AudioSource audioSource;

        switch (type)
        {
            case Sound.BGM:
                audioSource = _audioSources[(int)Sound.BGM];
                if (audioSource.isPlaying)
                    audioSource.Stop();

                audioSource.pitch = pitch;
                audioSource.clip = audioClip;
                audioSource.Play();
                break;

            case Sound.Effect:
                audioSource = _audioSources[(int)Sound.Effect];
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(audioClip);
                break;

            default:
                return;
        }
    }

    private AudioClip GetOrAddAudioClip(string path, Sound type)
    {
        if (path.Contains("Sounds/") == false)
            path = $"Sounds/{path}";

        AudioClip audioClip;

        switch (type)
        {
            case Sound.BGM:
                audioClip = Resources.Load<AudioClip>(path);
                break;

            case Sound.Effect:
                if (_audioClips.TryGetValue(path, out audioClip) == false)
                {
                    audioClip = Resources.Load<AudioClip>(path);
                    _audioClips.Add(path, audioClip);
                }
                break;

            default:
                audioClip = null;
                break;
        }

        if (audioClip == null)
            Debug.LogWarning($"AudioClip Missing ! {path}");

        return audioClip;
    }
}
