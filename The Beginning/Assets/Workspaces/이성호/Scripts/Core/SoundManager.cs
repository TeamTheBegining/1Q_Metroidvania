using System;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    private int capacity = 8;
    private AudioSource bgmSource;
    private AudioSource[] sfxSources;
    private float bgmSoundValue = 0.1f;
    private float sfxSoundValue = 0.6f;

    [SerializeField] private AudioClip[] bgmClips = new AudioClip[(int)BGMType.BGMTypeCount];
    [SerializeField] private AudioClip[] sfxClips = new AudioClip[(int)SFXType.SoundTypeCount];

    protected override void Awake()
    {
        bgmSource = this.gameObject.AddComponent<AudioSource>();

        sfxSources = new AudioSource[capacity];
        for (int i = 0; i < capacity; i++)
        {
            sfxSources[i] = this.gameObject.AddComponent<AudioSource>();
        }

        // BGMType 내용 정의 순서대로 클립추가하기
        bgmClips[0] = Resources.Load<AudioClip>("Audio/BGM/BGM01(Test)");

        // SFXType 내용 정의 순서대로 클립추가하기
        sfxClips[0] = Resources.Load<AudioClip>("Audio/Footstep/Player/NORMAL_FOOTSTEP_MAIN01");
    }

    public void PlayBGM(BGMType type)
    {
        bgmSource.clip = bgmClips[(int)type];
        bgmSource.Play();
        bgmSource.loop = true;
        bgmSource.volume = bgmSoundValue;
    }

    public void PlaySound(SFXType type)
    {
        int index = GetAudioSourceIndex();

        if (index != -1)
        {
            sfxSources[index].clip = sfxClips[(int)type];
            sfxSources[index].Play();
            sfxSources[index].volume = sfxSoundValue;
        }
    }

    private int GetAudioSourceIndex()
    {
        for (int i = 0; i < sfxSources.Length; i++)
        {
            if (sfxSources[i].clip == null)
            {
                return i;
            }
        }

        return -1;
    }

    private void Update()
    {
        foreach (var audio in sfxSources)
        {
            if (!audio.isPlaying && audio.clip != null)
            {
                audio.clip = null;  // 재생 끝나면 종료
            }
        }
    }
}
