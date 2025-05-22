using System;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    private int capacity = 8;
    private AudioSource bgmSource;
    private AudioSource[] sfxSources;
    private float bgmSoundValue = 1.0f;
    private float sfxSoundValue = 0.8f;

    [SerializeField] private AudioClip[] bgmClips = new AudioClip[(int)BGMType.BGMTypeCount];
    [SerializeField] private AudioClip[] sfxClips = new AudioClip[(int)SFXType.SoundTypeCount];

    protected override void Awake()
    {
        base.Awake();

        bgmSource = this.gameObject.AddComponent<AudioSource>();

        sfxSources = new AudioSource[capacity];
        for (int i = 0; i < capacity; i++)
        {
            sfxSources[i] = this.gameObject.AddComponent<AudioSource>();
        }

        // BGMType 내용 정의 순서대로 클립추가하기
        bgmClips[0] = Resources.Load<AudioClip>("Audio/BGM/BGM01(Test)");

        // SFXType 내용 정의 순서대로 클립추가하기
        sfxClips[0] = Resources.Load<AudioClip>("Audio/Attack/Player/Sword_Slash_01");
        sfxClips[1] = Resources.Load<AudioClip>("Audio/Attack/Player/Sword_Slash_02");
        sfxClips[2] = Resources.Load<AudioClip>("Audio/Attack/Player/Sword_Slash_03");
        sfxClips[3] = Resources.Load<AudioClip>("Audio/Attack/Player/Sword_Slash_04");

        sfxClips[4] = Resources.Load<AudioClip>("Audio/Death/Blood_Main_01");
        sfxClips[5] = Resources.Load<AudioClip>("Audio/Death/Blood_Main_02");
        sfxClips[6] = Resources.Load<AudioClip>("Audio/Death/Blood_Main_03");

        sfxClips[7] = Resources.Load<AudioClip>("Audio/Footstep/Player/NORMAL_FOOTSTEP_MAIN01");
        sfxClips[8] = Resources.Load<AudioClip>("Audio/Footstep/Player/NORMAL_FOOTSTEP_MAIN02");
        sfxClips[9] = Resources.Load<AudioClip>("Audio/Footstep/Player/NORMAL_FOOTSTEP_MAIN03");

        sfxClips[10] = Resources.Load<AudioClip>("Audio/Jump/Jump_01");

        sfxClips[11] = Resources.Load<AudioClip>("Audio/Ladder/Ladder_01");
        sfxClips[12] = Resources.Load<AudioClip>("Audio/Ladder/Ladder_02");
        sfxClips[13] = Resources.Load<AudioClip>("Audio/Ladder/Ladder_03");
        sfxClips[14] = Resources.Load<AudioClip>("Audio/Ladder/Ladder_04");

        sfxClips[15] = Resources.Load<AudioClip>("Audio/Landing/Landing_01");

        sfxClips[16] = Resources.Load<AudioClip>("Audio/Parrying/Parrying_Hit_01");
        sfxClips[17] = Resources.Load<AudioClip>("Audio/Parrying/Parrying_Swing_01");

        sfxClips[18] = Resources.Load<AudioClip>("Audio/Sliding/Slide_01");
        sfxClips[19] = Resources.Load<AudioClip>("Audio/Sliding/Slide_02");

        sfxClips[20] = Resources.Load<AudioClip>("Audio/Damaged/Armor_Hit_01");
        sfxClips[21] = Resources.Load<AudioClip>("Audio/Damaged/Armor_Hit_02");

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
