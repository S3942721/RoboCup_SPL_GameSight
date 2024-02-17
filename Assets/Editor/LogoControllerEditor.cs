#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LogoController))]
public class LogoControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LogoController logoController = (LogoController)target;

        if (GUILayout.Button("Print Logo Paths"))
        {
            PrintLogoPaths(logoController.teamNumber.ToString());
        }
    }

    private void PrintLogoPaths(string teamNumber)
    {
        string logosFolder = "Assets/Resources/Logos/";
        string[] guids = AssetDatabase.FindAssets(teamNumber + " t:texture", new[] { logosFolder });

        Debug.Log("Logo paths for team number " + teamNumber + ":");

        foreach (string guid in guids)
        {
            string logoPath = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log(logoPath);
        }
    }
}
#endif
