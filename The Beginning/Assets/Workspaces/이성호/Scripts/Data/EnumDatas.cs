using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// 범용 Enum 타입정의

public enum PoolType
{
    Hit1 = 0,
    PlayerSlideAfterImage,
    ProjectilePlayer,
    ProjectileEnemy,
    UltEffect,
    ProjectileObstacle,
    Thunder,
    PoolTypeCount
}

public enum BGMType
{
    mainTest,
    menu,               // 01.BGM_SCREEN_01
    bgm_Ambience_01,    // 02.BGM_AMBIENCE_01
    bgm_Credit_01,      // 03_BGM_CREDIT_01
    bgm_Ambience_02,    // 04_BGM_AMBIENCE_01
    guardian_01,        // 05_BGM_GUARDIAN_01
    bgm_Finalboss_01,   // 07_BGM_FINALBOSS_01

    BGMTypeCount
}

public enum SFXType
{
    // ---------------------- Player
    playerSword1 = 0,
    playerSword2,       // 1
    playerSword3,       // 2
    playerSword4,       // 3
    blood_Main_01,      // 4
    blood_Main_02,      // 5
    blood_Main_03,      // 6
    playerFootstep1,    // 7
    playerFootstep2,    // 8
    playerFootstep3,    // 9
    jump_01,            // 10
    ladder_01,          // 11
    ladder_02,          // 12
    ladder_03,          // 13
    ladder_04,          // 14
    landing_01,         // 15
    parrying_Hit_01,    // 16
    parrying_Swing_01,  // 17
    slide_01,           // 18
    slide_02,           // 19
    armor_Hit_01,       // 20
    armor_Hit_02,       // 21
    charging_01,        // 22
    charging_Slash_01,  // 23
    grab_01,            // 24
    // ---------------------- Player

    // ---------------------- Enemy
    // --------------------- Mini Boss
    attack_A01,         // 25
    attack_B01,         // 26
    attack_C01,         // 27
    hit_01,             // 28
    hit_02,             // 29
    hit_03,             // 30
    MiniBoss_Death_01,  // 31

    // --------------------- S_Attacker
    attack_01,          // 32
    HIT_01,             // 33
    HIT_02,             // 34
    HIT_03,             // 35
    down_01,            // 36
    FOOTSTEP_01,        // 37
    FOOTSTEP_02,        // 38
    FOOTSTEP_03,        // 39
    FOOTSTEP_04,        // 40

    // --------------------- trashMob
    t_ATTACK_01,        // 41
    t_ATTACK_02,        // 42
    t_HIT_01,           // 43
    t_HIT_02,           // 44
    t_DOWN_01,          // 45
    t_FOOTSTEP_01,      // 46
    t_FOOTSTEP_02,      // 47
    t_FOOTSTEP_03,      // 48
    t_FOOTSTEP_04,      // 49

    // --------------------- QualityUP
    climbing_01,        // 50
    wall_Slide_01,      // 51

    // --------------------- Effect Sound
    intro_01,           // 52
    intro_02,           // 53

    SoundTypeCount
}

public enum CameraType
{
    TitleCamra = 0,
    Scene1Camera,
    Scene2PlayerCamera,
    Scene2CutSceneCamera,
    Scene3Camera,
    Scene4Camera,
    Scene5CameraUpper,
    Scene5CameraMiddle,
    Scene5CameraBottom,
    Scene5CameraBottomBossWay,
    Scene5CameraBottomRoom1,
    Scene6Camera,
    CameraTypeCount
}

public enum PlayerSkillType
{
    ChargAttack = 0,
    DoubleJump,    
    PlayerSkillTypeCount,
}

public enum EnemyType // TODO : 적 이름 정의 명확하게 지정할 것 - [0518] 기준 프리팹에 저장된 이름 기준 작성
{
    Attacker = 0,
    Middle,
    S_Enemy,
    EnemyTypeCount
}

public enum CutSceneType
{
    Intro = 0,
    Credit,
    PlayerUlt,
    CutSceneTypeCount
}