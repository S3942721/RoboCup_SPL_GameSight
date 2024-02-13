using System;
using UnityEngine;

public class SoccerFieldGenerator : MonoBehaviour
{
    public float fieldLength = 9f;
    public float fieldWidth = 6f;
    public float lineWidth = 0.05f;
    public float penaltyMarkSize = 0.1f;
    public float goalAreaLength = 0.6f;
    public float goalAreaWidth = 2.2f;
    public float penaltyAreaLength = 1.65f;
    public float penaltyAreaWidth = 4f;
    public float penaltyMarkDistance = 1.3f;
    public float centerCircleDiameter = 1.5f;
    public float borderStripWidth = 0.7f;

    public Material fieldLinesMaterial;

    public Material fieldMaterial;

    public Material team0Material;
    public Material team1Material;

    void Start()
    {
        GenerateSoccerField();
    }

    void GenerateSoccerField()
    {
        // Create the main field GameObject
        GameObject soccerField = new GameObject("SoccerField");

        // Create a child object called "quad"
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(quad.GetComponent<Collider>());  // Remove the collider
        quad.name = "Quad";
        quad.transform.parent = soccerField.transform;  // Set the parent

        // Adjust the size of the quad
        quad.transform.localScale = new Vector3(fieldLength + 2 * borderStripWidth, fieldWidth + 2 * borderStripWidth, 0);

        // Rotate the quad to align with the field
        quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Adjust the position of the quad to be at the same level as the lines
        quad.transform.position = new Vector3(0, 0, 0);

        // Add MeshRenderer component to the quad for the main field material
        MeshRenderer quadRenderer = quad.GetComponent<MeshRenderer>();
        quadRenderer.material = fieldMaterial;



        // OuterLine
        DrawRectangle(soccerField, -fieldLength / 2, -fieldWidth / 2, fieldLength, fieldWidth, lineWidth, "FieldLine");

        // Add lines for Goal Box
        DrawRectangle(soccerField, -fieldLength / 2, -goalAreaWidth / 2, goalAreaLength, goalAreaWidth, lineWidth, "GoalBox");
        DrawRectangle(soccerField, (fieldLength / 2) - goalAreaLength, -goalAreaWidth / 2, goalAreaLength, goalAreaWidth, lineWidth, "GoalBox");

        // Add lines for PenaltyBox
        DrawRectangle(soccerField, -fieldLength / 2, -penaltyAreaWidth / 2, penaltyAreaLength, penaltyAreaWidth, lineWidth, "PenaltyArea");
        DrawRectangle(soccerField, (fieldLength / 2) - penaltyAreaLength, -penaltyAreaWidth / 2, penaltyAreaLength, penaltyAreaWidth, lineWidth, "PenaltyArea");

        // Add penalty marks
        AddPenaltyMark(soccerField, (-fieldLength / 2) + penaltyMarkDistance, 0);
        AddPenaltyMark(soccerField, (fieldLength / 2) - penaltyMarkDistance, 0);

        // Add center circle
        AddCenterCircle(soccerField, new Vector3(0, 0.001f, 0), centerCircleDiameter, lineWidth);

        // Add halfway Line
        AddLineRenderer(soccerField, new Vector3(0, 0.001f, -fieldWidth / 2), new Vector3(0, 0.001f, fieldWidth / 2), lineWidth, "HalfwayLine");

        // Add Centre Mark
        AddLineRenderer(soccerField, new Vector3(-penaltyMarkSize / 2, 0.001f, 0), new Vector3(penaltyMarkSize / 2, 0.001f, 0), lineWidth, "CentreMark");


        // Add some players

        GameObject player0_1 = InstantiatePlayer("Player0-1", new Vector3(0, 0.25f, 0), team0Material);
        GameObject player0_2 = InstantiatePlayer("Player0-2", new Vector3(0, 0.25f, 0), team0Material);
        GameObject player0_3 = InstantiatePlayer("Player0-3", new Vector3(0, 0.25f, 0), team0Material);
        GameObject player0_4 = InstantiatePlayer("Player0-4", new Vector3(0, 0.25f, 0), team0Material);
        GameObject player0_5 = InstantiatePlayer("Player0-5", new Vector3(0, 0.25f, 0), team0Material);

        GameObject player1_1 = InstantiatePlayer("Player1-1", new Vector3(0, 0.25f, 0), team1Material);
        GameObject player1_2 = InstantiatePlayer("Player1-2", new Vector3(0, 0.25f, 0), team1Material);
        GameObject player1_3 = InstantiatePlayer("Player1-3", new Vector3(0, 0.25f, 0), team1Material);
        GameObject player1_4 = InstantiatePlayer("Player1-4", new Vector3(0, 0.25f, 0), team1Material);
        GameObject player1_5 = InstantiatePlayer("Player1-5", new Vector3(0, 0.25f, 0), team1Material);

        SaveAsPrefab(soccerField, "SoccerFieldPrefab");


        GameObject InstantiatePlayer(string playerName, Vector3 position, Material material)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            player.name = playerName;
            player.transform.parent = soccerField.transform;
            player.transform.localScale = new Vector3(0.1f, .3f, 0.1f); // Adjust scale as needed
            player.transform.position = position;

            // Set the player material
            Renderer playerRenderer = player.GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                playerRenderer.material = material;
            }
            else
            {
                Debug.LogError("Renderer component not found on the player object.");
            }

            // You might want to add other components (like Rigidbody, Collider) to the player object if needed

            return player;
        }

        void AddLineRenderer(GameObject parent, Vector3 startPoint, Vector3 endPoint, float width, string name = "Line")
        {
            Debug.Log("Rendering: " + name + " from: " + startPoint.ToString() + " to: " + endPoint.ToString());
            GameObject lineObject = new GameObject(name);
            lineObject.transform.parent = parent.transform;

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = false;

            // Use local positions instead of world positions
            lineRenderer.SetPosition(0, startPoint - parent.transform.position);
            lineRenderer.SetPosition(1, endPoint - parent.transform.position);

            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.material = fieldLinesMaterial;
        }

        void AddPenaltyMark(GameObject parent, float x, float y)
        {
            AddLineRenderer(parent, new Vector3(x - (penaltyMarkSize / 2), 0.001f, y), new Vector3(x + (penaltyMarkSize / 2), 0.001f, y), lineWidth, "Penalty Mark");
            AddLineRenderer(parent, new Vector3(x, 0.001f, y - (penaltyMarkSize / 2)), new Vector3(x, 0.001f, y + (penaltyMarkSize / 2)), lineWidth, "Penalty Mark");
        }

        void AddCenterCircle(GameObject parent, Vector3 center, float diameter, float lineWidth)
        {
            GameObject centerCircle = new GameObject("CenterCircle");
            centerCircle.transform.parent = parent.transform;
            centerCircle.transform.position = center;

            int segments = 360; // Number of line segments to approximate a circle
            float radius = diameter / 2f;
            LineRenderer lineRenderer = centerCircle.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = segments + 1;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = fieldLinesMaterial;

            float angleIncrement = 360f / segments;
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * i * angleIncrement;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                lineRenderer.SetPosition(i, new Vector3(x, 0.001f, z));
            }
        }



        void DrawRectangle(GameObject parent, float x, float y, float w, float h, float width, String name)
        {
            Debug.Log("DrawRectangle: " + name + "from: " + x.ToString() + "," + y.ToString() + "with size: " + w.ToString() + "," + h.ToString());
            AddLineRenderer(parent, new Vector3(x, 0.001f, y), new Vector3(x + w, 0.001f, y), width, name);
            AddLineRenderer(parent, new Vector3(x, 0.001f, y), new Vector3(x, 0.001f, y + h), width, name);
            AddLineRenderer(parent, new Vector3(x + w, 0.001f, y), new Vector3(x + w, 0.001f, y + h), width, name);
            AddLineRenderer(parent, new Vector3(x, 0.001f, y + h), new Vector3(x + w, 0.001f, y + h), width, name);
        }
    }

    void SaveAsPrefab(GameObject obj, string prefabName)
    {
        // Create a prefab and save it in the "Assets" folder
        string prefabPath = "Assets/" + prefabName + ".prefab";
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
    }

}
