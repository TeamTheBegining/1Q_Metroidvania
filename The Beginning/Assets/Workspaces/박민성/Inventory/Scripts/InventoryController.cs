using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public PopupController popupController;

    private void Start()
    {
        // ����: �˾� Ÿ�԰� �κ��丮 Ÿ�� ��Ī
        // ������ �κ��丮 Ŀ�� ��ġ�� ���� �˾� Ÿ�� ���� �ʿ�
        popupController.SetCursorType(PopupType.Equip);
    }

    private void Update()
    {
        // ���⼭ �ʿ��ϸ� �κ��丮 ���� üũ �� �˾� ���� ���� ���� ���� ����
    }
}
