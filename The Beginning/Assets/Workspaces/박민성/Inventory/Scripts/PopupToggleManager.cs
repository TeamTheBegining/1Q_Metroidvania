using UnityEngine;
using System;

public class PopupToggleManager : MonoBehaviour
{
    public static event Action OpenPopup;
    public static event Action ClosePopup;

    public GameObject SkillPopup;
    public GameObject EquipPopup;
    public GameObject SpecialPopup;

    private enum CursorType { Skill, Equip, Special }
    private CursorType currentCursor;

    private GameObject currentActivePopup;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            switch (currentCursor)
            {
                case CursorType.Skill:
                    TogglePopup(SkillPopup);
                    break;
                case CursorType.Equip:
                    TogglePopup(EquipPopup);
                    break;
                case CursorType.Special:
                    TogglePopup(SpecialPopup);
                    break;
            }
        }
    }

    void TogglePopup(GameObject popup)
    {
        bool isActive = popup.activeSelf;

        // 꺼질 때
        if (isActive)
        {
            popup.SetActive(false);
            currentActivePopup = null;
            PopupCancel?.Invoke();
        }
        else // 켜질 때
        {
            if (currentActivePopup != null)
                currentActivePopup.SetActive(false);

            popup.SetActive(true);
            currentActivePopup = popup;
            OpenPopup?.Invoke();
        }
    }

    public void SetCursorType(string type)
    {
        currentCursor = (CursorType)Enum.Parse(typeof(CursorType), type);
    }
}


//using UnityEngine;

//public class PopupToggleManager : MonoBehaviour
//{
//    public GameObject skillPopup;
//    public GameObject equipPopup;
//    public GameObject specialPopup;

//    private enum CursorType { Skill, Equip, Special }
//    private CursorType currentCursor;

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Return))
//        {
//            switch (currentCursor)
//            {
//                case CursorType.Skill:
//                    TogglePopup(skillPopup);
//                    break;
//                case CursorType.Equip:
//                    TogglePopup(equipPopup);
//                    break;
//                case CursorType.Special:
//                    TogglePopup(specialPopup);
//                    break;
//            }
//        }
//    }

//    void TogglePopup(GameObject popup)
//    {
//        bool isActive = popup.activeSelf;
//        popup.SetActive(!isActive);
//    }

//    public void SetCursorType(string type)
//    {
//        currentCursor = (CursorType)System.Enum.Parse(typeof(CursorType), type);
//    }
//}
