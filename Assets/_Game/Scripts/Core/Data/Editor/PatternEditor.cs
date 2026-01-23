using UnityEngine;
using UnityEditor;
using _Game.Scripts.Core.Data; 

namespace _Game.Scripts.Core.Editor
{
    [CustomEditor(typeof(PatternData))]
    public class PatternDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 1. Lấy dữ liệu từ file PatternData hiện tại
            PatternData data = (PatternData)target;

            // 2. Cập nhật các thay đổi
            serializedObject.Update();
            
            // --- PHẦN 1: VẼ CÁC TRƯỜNG CƠ BẢN ---
            EditorGUILayout.PropertyField(serializedObject.FindProperty("patternName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("multiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("GRID DESIGNER", EditorStyles.boldLabel);

            // --- PHẦN 2: CẤU HÌNH KÍCH THƯỚC LƯỚI ---
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck(); // Bắt đầu theo dõi thay đổi
            
            int newRows = EditorGUILayout.IntField("Rows", data.editorRows);
            int newCols = EditorGUILayout.IntField("Cols", data.editorCols);
            
            if (EditorGUI.EndChangeCheck())
            {
                // Nếu số thay đổi, lưu lại action để có thể Undo (Ctrl+Z)
                Undo.RecordObject(data, "Change Grid Size");
                data.editorRows = Mathf.Max(1, newRows);
                data.editorCols = Mathf.Max(1, newCols);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Click cells to select pattern shape.", MessageType.None);

            // --- PHẦN 3: VẼ MA TRẬN NÚT BẤM ---
            Color defaultColor = GUI.backgroundColor;
            EditorGUILayout.BeginVertical("box"); // Tạo khung bao quanh

            // Vẽ từ hàng trên cùng xuống hàng dưới cùng (để khớp visual)
            for (int y = data.editorRows - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < data.editorCols; x++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    
                    // Kiểm tra xem tọa độ này đã có trong list chưa
                    bool isSelected = data.relativeCoordinates.Contains(coord);

                    // Đổi màu nút: Xanh (Chọn) - Trắng (Chưa chọn)
                    GUI.backgroundColor = isSelected ? Color.green : Color.white;

                    // Vẽ nút bấm hình vuông
                    if (GUILayout.Button($"({x},{y})", GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        Undo.RecordObject(data, "Toggle Cell"); // Hỗ trợ Undo
                        
                        if (isSelected)
                            data.relativeCoordinates.Remove(coord);
                        else
                            data.relativeCoordinates.Add(coord);
                        
                        // Đánh dấu file đã thay đổi để Unity lưu xuống đĩa
                        EditorUtility.SetDirty(data);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            
            // Trả lại màu mặc định
            GUI.backgroundColor = defaultColor;

            // --- PHẦN 4: HIỂN THỊ LIST KẾT QUẢ (READ ONLY) ---
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Selected Cells: {data.relativeCoordinates.Count}");
            
            // Vẽ list coordinate dạng mờ (không cho sửa tay để tránh lỗi)
            EditorGUI.BeginDisabledGroup(true); 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("relativeCoordinates"), true);
            EditorGUI.EndDisabledGroup();

            // Áp dụng mọi thay đổi
            serializedObject.ApplyModifiedProperties();
        }
    }
}