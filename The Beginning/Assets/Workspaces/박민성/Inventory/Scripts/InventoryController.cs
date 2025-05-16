using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public PopupController popupController;

    private void Start()
    {
        // 예시: 팝업 타입과 인벤토리 타입 매칭
        // 실제로 인벤토리 커서 위치에 따라 팝업 타입 세팅 필요
        popupController.SetCursorType(PopupType.Equip);
    }

    private void Update()
    {
        // 여기서 필요하면 인벤토리 상태 체크 후 팝업 열기 같은 로직 구현 가능
    }
}
