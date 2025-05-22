using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene6Manager : LocalSceneManager
{
    public override void Init()
    {
        GameObject playerObject = FindFirstObjectByType<Player>().gameObject;
        CameraManager.Instance.SetTarget(CameraType.Scene6Camera, playerObject.transform);
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene6Camera, 20);

        SoundManager.Instance.PlayBGM(BGMType.bgm_Finalboss_01);

        LastBoss boss = FindFirstObjectByType<LastBoss>();
        boss.OnDead += () => { SceneManager.LoadScene("Ending"); };
        boss.FindPlayer();
    }
}