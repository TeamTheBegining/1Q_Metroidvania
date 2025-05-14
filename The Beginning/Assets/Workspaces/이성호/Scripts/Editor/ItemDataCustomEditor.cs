 using UnityEngine;
 using UnityEditor;

[CustomEditor(typeof(ItemDataSO))]
public class ItemDataCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ItemDataSO item = (ItemDataSO)target;

        if(item.icon != null)
        {
            GUILayout.Label("아이콘 미리보기", EditorStyles.boldLabel);

            // 이미지 정렬
            float width = EditorGUIUtility.currentViewWidth - 40;
            float size = Mathf.Min(128, width);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Rect rect = GUILayoutUtility.GetRect(size, size);

            Sprite sprite = item.icon;
            Texture2D tex = sprite.texture;

            Rect spriteRect = sprite.rect;
            Rect uv = new Rect(
                spriteRect.x / tex.width,
                spriteRect.y / tex.height,
                spriteRect.width / tex.width,
                spriteRect.height / tex.height
            );

            GUI.DrawTextureWithTexCoords(rect, tex, uv);


            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
