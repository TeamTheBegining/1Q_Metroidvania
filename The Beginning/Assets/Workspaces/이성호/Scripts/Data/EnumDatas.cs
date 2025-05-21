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
    PoolTypeCount
}

public enum BGMType
{
    main,
    BGMTypeCount
}

public enum SFXType
{
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
    CameraTypeCount
}

public enum PlayerSkillType
{
    DoubleJump = 0,
    ChargeAttack,    
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