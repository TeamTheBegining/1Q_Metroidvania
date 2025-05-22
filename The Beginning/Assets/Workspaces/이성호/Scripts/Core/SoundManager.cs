using System;
using System.Collections;
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

        #region Lode Resource
        // BGMType 내용 정의 순서대로 클립추가하기
        bgmClips[0] = Resources.Load<AudioClip>("Audio/BGM/BGM01(Test)");   
        bgmClips[1] = Resources.Load<AudioClip>("Audio/BGM/BGM_SCREEN_01");   

        // SFXType 내용 정의 순서대로 클립추가하기

        // ------------------------------------------- Player ------------------------------------------------
        sfxClips[0] = Resources.Load<AudioClip>("Audio/PlayerSound/Attack/Player/Sword_Slash_01");
        sfxClips[1] = Resources.Load<AudioClip>("Audio/PlayerSound/Attack/Player/Sword_Slash_02");
        sfxClips[2] = Resources.Load<AudioClip>("Audio/PlayerSound/Attack/Player/Sword_Slash_03");
        sfxClips[3] = Resources.Load<AudioClip>("Audio/PlayerSound/Attack/Player/Sword_Slash_04");

        sfxClips[4] = Resources.Load<AudioClip>("Audio/PlayerSound/Death/Blood_Main_01");
        sfxClips[5] = Resources.Load<AudioClip>("Audio/PlayerSound/Death/Blood_Main_02");
        sfxClips[6] = Resources.Load<AudioClip>("Audio/PlayerSound/Death/Blood_Main_03");

        sfxClips[7] = Resources.Load<AudioClip>("Audio/PlayerSound/Footstep/Player/NORMAL_FOOTSTEP_MAIN01");
        sfxClips[8] = Resources.Load<AudioClip>("Audio/PlayerSound/Footstep/Player/NORMAL_FOOTSTEP_MAIN02");
        sfxClips[9] = Resources.Load<AudioClip>("Audio/PlayerSound/Footstep/Player/NORMAL_FOOTSTEP_MAIN03");

        sfxClips[10] = Resources.Load<AudioClip>("Audio/PlayerSound/Jump/Jump_01");

        sfxClips[11] = Resources.Load<AudioClip>("Audio/PlayerSound/Ladder/Ladder_01");
        sfxClips[12] = Resources.Load<AudioClip>("Audio/PlayerSound/Ladder/Ladder_02");
        sfxClips[13] = Resources.Load<AudioClip>("Audio/PlayerSound/Ladder/Ladder_03");
        sfxClips[14] = Resources.Load<AudioClip>("Audio/PlayerSound/Ladder/Ladder_04");

        sfxClips[15] = Resources.Load<AudioClip>("Audio/PlayerSound/Landing/Landing_01");

        sfxClips[16] = Resources.Load<AudioClip>("Audio/PlayerSound/Parrying/Parrying_Hit_01");
        sfxClips[17] = Resources.Load<AudioClip>("Audio/PlayerSound/Parrying/Parrying_Swing_01");

        sfxClips[18] = Resources.Load<AudioClip>("Audio/PlayerSound/Sliding/Slide_01");
        sfxClips[19] = Resources.Load<AudioClip>("Audio/PlayerSound/Sliding/Slide_02");

        sfxClips[20] = Resources.Load<AudioClip>("Audio/PlayerSound/Damaged/Armor_Hit_01");
        sfxClips[21] = Resources.Load<AudioClip>("Audio/PlayerSound/Damaged/Armor_Hit_02");

        sfxClips[22] = Resources.Load<AudioClip>("Audio/PlayerSound/Charging/CHARGING_01");
        sfxClips[23] = Resources.Load<AudioClip>("Audio/PlayerSound/Charging/CHARGING_SLASH_01");

        sfxClips[24] = Resources.Load<AudioClip>("Audio/PlayerSound/Grab/GRAB_01");

        // ---------------------------------------------- Enemy --------------------------------------------

        // --------------------------------------------- MiniBoss ------------------------------------------

        sfxClips[25] = Resources.Load<AudioClip>("Audio/MiniBossSound/Attack/ATTACK_A01");
        sfxClips[26] = Resources.Load<AudioClip>("Audio/MiniBossSound/Attack/ATTACK_B01");
        sfxClips[27] = Resources.Load<AudioClip>("Audio/MiniBossSound/Attack/ATTACK_C01");

        sfxClips[28] = Resources.Load<AudioClip>("Audio/MiniBossSound/Damaged/HIT_01");
        sfxClips[29] = Resources.Load<AudioClip>("Audio/MiniBossSound/Damaged/HIT_02");
        sfxClips[30] = Resources.Load<AudioClip>("Audio/MiniBossSound/Damaged/HIT_03");

        sfxClips[31] = Resources.Load<AudioClip>("Audio/MiniBossSound/Death/MiniBoss_Death_01");
        #endregion
    }

    public void PlayBGM(BGMType type)
    {
        bgmSource.clip = bgmClips[(int)type];
        bgmSource.Play();
        bgmSource.loop = true;
        bgmSource.volume = bgmSoundValue;
    }    

    /// <summary>
    /// BGM Volume값이 1에서 0으로 선형적으로 줄어드는 함수 ( 결과적으로 음악은 안 변하고 볼륨값만 0 됨)
    /// </summary>
    /// <param name="duration">0까지 변화하는 시간</param>
    public void FadeInBGM(float duration)
    {
        // 0522 소리가 끊기는지 확인할 것 | TODO 코루틴 실행이 연속적으로 되는지 확인하고 어색하면 수정하기
        StartCoroutine(FadeInBGMProcess(duration)); 
    }

    /// <summary>
    /// BGM Volume값이 1에서 0으로 선형적으로 줄어드는 함수 ( 결과적으로 음악은 안 변하고 볼륨값만 1 됨)
    /// </summary>
    /// <param name="duration">1까지 변화하는 시간</param>
    public void FadeOutBGM(float duration)
    {
        StartCoroutine(FadeOutBGMProcess(duration));
    }

    private IEnumerator FadeInBGMProcess(float duration)
    {
        float timeElapsed = 0.0f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / duration);
            bgmSource.volume = t;

            yield return null;
        }
    }

    private IEnumerator FadeOutBGMProcess(float duration)
    {
        float timeElapsed = 0.0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / duration);
            bgmSource.volume = 1 - t;

            yield return null;
        }
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
