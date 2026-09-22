using UnityEngine;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Generates a closed room (floor, ceiling, and 4 walls) around the object.
    /// Use the "Generate Room" context menu item in the Inspector to create the room.
    /// </summary>
    public class RoomGenerator : MonoBehaviour
    {
        [Header("Room Dimensions")]
        [SerializeField] private float width = 10f;
        [SerializeField] private float height = 5f;
        [SerializeField] private float depth = 10f;
        [SerializeField] private float wallThickness = 0.2f;

        [Header("Appearance")]
        [SerializeField] private Material roomMaterial;

    void Awake()
    {
        GenerateRoom();
    }

    [ContextMenu("Generate Room")]
    public void GenerateRoom()
    {
        // Clear existing room elements to avoid duplicates
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

            // Create Floor
            CreateWall("Floor", new Vector3(0, -wallThickness / 2, 0), new Vector3(width, wallThickness, depth));
            
            // Create Ceiling
            CreateWall("Ceiling", new Vector3(0, height - wallThickness / 2, 0), new Vector3(width, wallThickness, depth));
            
            // Create North Wall
            CreateWall("Wall_North", new Vector3(0, height / 2, depth / 2 - wallThickness / 2), new Vector3(width, height, wallThickness));
            
            // Create South Wall
            CreateWall("Wall_South", new Vector3(0, height / 2, -depth / 2 + wallThickness / 2), new Vector3(width, height, wallThickness));
            
            // Create East Wall
            CreateWall("Wall_East", new Vector3(width / 2 - wallThickness / 2, height / 2, 0), new Vector3(wallThickness, height, depth));
            
            // Create West Wall
            CreateWall("Wall_West", new Vector3(-width / 2 + wallThickness / 2, height / 2, 0), new Vector3(wallThickness, height, depth));
            
            Debug.Log("Room generated successfully!");
        }

        private void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(this.transform);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;

            if (roomMaterial != null)
            {
                wall.GetComponent<Renderer>().material = roomMaterial;
            }
        }
    }
}
