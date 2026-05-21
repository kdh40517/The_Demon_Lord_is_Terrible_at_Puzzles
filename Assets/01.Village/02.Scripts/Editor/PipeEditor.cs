using UnityEngine;
using UnityEditor;

namespace TM
{
    // TM.Pipe 클래스의 인스펙터를 커스텀하겠다고 선언합니다.
    [CustomEditor(typeof(Pipe))]
    public class PipeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // 스크립트 오브젝트의 최신 상태를 가져옵니다.
            serializedObject.Update();

            SerializedProperty prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    // "isOpened" 배열 부분에 도달하면, 기본 리스트 대신 우리가 만든 커스텀 십자 UI를 그립니다.
                    if (prop.name == "isOpened")
                    {
                        DrawDirectionalUI(prop);
                    }
                    else
                    {
                        // "isOpened"가 아닌 나머지 변수(스프라이트, 오디오 등)는 원래 유니티 기본 방식대로 그립니다.
                        EditorGUILayout.PropertyField(prop, true);
                    }
                } while (prop.NextVisible(false));
            }

            // 변경사항을 실제 오브젝트에 적용합니다.
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDirectionalUI(SerializedProperty isOpenedProp)
        {
            // 배열 크기가 4가 아니면 강제로 4로 맞춥니다 (안전장치)
            if (isOpenedProp.arraySize != 4)
            {
                isOpenedProp.arraySize = 4;
            }

            // 각각 북, 동, 남, 서 속성 매핑
            SerializedProperty north = isOpenedProp.GetArrayElementAtIndex(0);
            SerializedProperty east = isOpenedProp.GetArrayElementAtIndex(1);
            SerializedProperty south = isOpenedProp.GetArrayElementAtIndex(2);
            SerializedProperty west = isOpenedProp.GetArrayElementAtIndex(3);

            EditorGUILayout.Space(10);
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("파이프 방향 설정 (클릭하여 토글)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // [북]
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            north.boolValue = DrawToggleButton(north.boolValue, "북(N)");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // [서]      [동]
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            west.boolValue = DrawToggleButton(west.boolValue, "서(W)");
            GUILayout.Space(30); // 가운데 십자 간격
            east.boolValue = DrawToggleButton(east.boolValue, "동(E)");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // [남]
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            south.boolValue = DrawToggleButton(south.boolValue, "남(S)");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        // 상태에 따라 색상이 변하는 커스텀 버튼을 그리는 함수
        private bool DrawToggleButton(bool currentValue, string label)
        {
            // 열려있으면(true) 초록색, 닫혀있으면(false) 원래 색상으로 시각적 피드백을 줍니다.
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = currentValue ? new Color(0.4f, 1f, 0.4f) : originalColor;

            // 버튼을 클릭하면 true/false 값이 반전됩니다.
            if (GUILayout.Button(label, GUILayout.Width(50), GUILayout.Height(30)))
            {
                currentValue = !currentValue;
            }

            // 다른 UI에 영향을 주지 않도록 색상 원상복구
            GUI.backgroundColor = originalColor;

            return currentValue;
        }
    }
}