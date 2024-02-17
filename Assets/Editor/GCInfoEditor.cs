#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(GCInfo))]
public class GCInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GCInfo gcInfo = (GCInfo)target;

        // Display default properties
        DrawDefaultInspector();

        // Display additional information about the team
        EditorGUILayout.LabelField("Team Information", EditorStyles.boldLabel);

        for (int i = 0; i < gcInfo.gameControlData.team.Length; i++)
        {
            EditorGUILayout.LabelField($"Team {i}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Team Number: {gcInfo.gameControlData.team[i].teamNumber}");
            EditorGUILayout.LabelField($"Goals: {gcInfo.gameControlData.team[i].score}");
            // Add other team information as needed
        }
    }
}
#endif
