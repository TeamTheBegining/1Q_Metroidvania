using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// 범용 Enum 타입정의

public enum PoolType
{
    Hit1 = 0,
    PlayerSlideAfterImage,
    PoolTypeCount
}

public enum BGMType
{
    Menu = 0,
    Battle,
    BGMTypeCount
}

public enum SFXType
{ 
    Hit = 0,
    SoundTypeCount
}

public enum CameraType
{
    TitleCamra = 0,
    Scene1Camera,
    Scene2PlayerCamera,
    Scene2CutSceneCamera,
    Scene3Camera,
    CameraTypeCount
}

public enum PlayerSkillType
{
    Skill1 = 0,
    Skill2,
    PlayerSkillTypeCount,
}

public enum EnemyType // TODO : 적 이름 정의 명확하게 지정할 것 - [0518] 기준 프리팹에 저장된 이름 기준 작성
{
    Attacker = 0,
    Middle,
    S_Enemy,
    EnemyTypeCount
}