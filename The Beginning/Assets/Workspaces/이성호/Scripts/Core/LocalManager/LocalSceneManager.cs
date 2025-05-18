using UnityEngine;

/// <summary>
/// 모든 로컬 씬 매니저가 상속받아야할 클래스
/// </summary>
public class LocalSceneManager : MonoBehaviour
{
    public virtual void Init()
    {
        // 씬 로드 후 실행할 내용
    }
}