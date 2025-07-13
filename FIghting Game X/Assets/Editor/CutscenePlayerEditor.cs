using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CutscenePlayer))]
public class CutscenePlayerEditor : Editor
{
    private SerializedProperty cutscenes;

    private void OnEnable()
    {
        cutscenes = serializedObject.FindProperty("cutscenes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Cutscenes", EditorStyles.boldLabel);

        string[] enumNames = System.Enum.GetNames(typeof(CutscenePlayer.CutsceneType));

        for (int i = 0; i < cutscenes.arraySize; i++)
        {
            SerializedProperty element = cutscenes.GetArrayElementAtIndex(i);
            string label = (i < enumNames.Length) ? enumNames[i] : $"Element {i}";
            EditorGUILayout.PropertyField(element, new GUIContent(label));
        }

        EditorGUILayout.Space();
        DrawPropertiesExcluding(serializedObject, "cutscenes");

        serializedObject.ApplyModifiedProperties();
    }
}
