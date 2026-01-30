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
            PatternData data = (PatternData)target;
            serializedObject.Update(); // Bắt đầu theo dõi thay đổi
            
            #region Vẽ UI mặc định
            EditorGUILayout.PropertyField(serializedObject.FindProperty("patternName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));
            #endregion

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("GRID DESIGNER", EditorStyles.boldLabel);

            #region Vẽ ô nhập kích thước
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            
            int newRows = EditorGUILayout.IntField("Rows", data.editorRows);
            int newCols = EditorGUILayout.IntField("Cols", data.editorCols);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(data, "Change Grid Size");
                data.editorRows = Mathf.Max(1, newRows);
                data.editorCols = Mathf.Max(1, newCols);
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Vẽ Ma trận nút bấm
            EditorGUILayout.HelpBox("Click để chọn ô.", MessageType.None);
            Color defaultColor = GUI.backgroundColor;
            EditorGUILayout.BeginVertical("box");

            // Vẽ ngược từ Y cao xuống thấp để khớp visual
            for (int y = data.editorRows - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < data.editorCols; x++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    bool isSelected = data.relativeCoordinates.Contains(coord);

                    // Xanh = Chọn, Trắng = Bỏ
                    GUI.backgroundColor = isSelected ? Color.green : Color.white;

                    if (GUILayout.Button($"({x},{y})", GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        Undo.RecordObject(data, "Toggle Cell");
                        if (isSelected) data.relativeCoordinates.Remove(coord);
                        else data.relativeCoordinates.Add(coord);
                        
                        EditorUtility.SetDirty(data); // Lưu file
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = defaultColor; // Trả lại màu gốc
            #endregion

            #region Hiển thị List kết quả
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Selected Cells: {data.relativeCoordinates.Count}");
            EditorGUI.BeginDisabledGroup(true); 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("relativeCoordinates"), true);
            EditorGUI.EndDisabledGroup();
            #endregion

            serializedObject.ApplyModifiedProperties(); // Apply thay đổi
        }
    }
}