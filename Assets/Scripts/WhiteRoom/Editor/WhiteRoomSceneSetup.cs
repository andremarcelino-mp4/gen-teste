#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace WhiteRoom.Editor
{
    [InitializeOnLoad]
    public static class WhiteRoomSceneSetup
    {
        const string PrefKey = "WhiteRoom.SceneSetup.v2";

        static WhiteRoomSceneSetup()
        {
            EditorApplication.delayCall += RunOnce;
        }

        static void RunOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (EditorPrefs.GetBool(PrefKey, false))
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/SampleScene.unity")
                return;

            Apply();
            EditorPrefs.SetBool(PrefKey, true);
        }

        [MenuItem("Tools/White Room/Apply Ceiling And South Door")]
        public static void Apply()
        {
            var env = GameObject.Find("Environment/Template Environment");
            if (env == null)
            {
                Debug.LogWarning("WhiteRoom setup: Template Environment not found.");
                return;
            }

            var white = LoadOrCreateLit("Assets/Materials/WhiteRoom/CeilingWhite.mat", Color.white, 0.08f, false);
            var grey = LoadOrCreateLit("Assets/Materials/WhiteRoom/HandleGrey.mat", new Color(0.55f, 0.56f, 0.58f), 0.35f, false);
            var ledMat = LoadOrCreateLit("Assets/Materials/WhiteRoom/SwitchLed.mat", new Color(0.15f, 0.9f, 0.25f), 0.2f, true);

            ApplyCeiling(env.transform, white);
            ApplySouthDoor(env.transform, white, grey, ledMat);

            EditorSceneManager.MarkSceneDirty(env.scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("WhiteRoom setup: white ceiling and south door applied.");
        }

        static Material LoadOrCreateLit(string path, Color color, float smoothness, bool emission)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder("Assets/Materials/WhiteRoom"))
                AssetDatabase.CreateFolder("Assets/Materials", "WhiteRoom");

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.shader = shader;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", 0f);
            if (emission)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.2f, 1.6f, 0.3f));
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void ApplyCeiling(Transform env, Material white)
        {
            var ceiling = env.Find("Ceiling");
            if (ceiling == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Ceiling";
                go.transform.SetParent(env, true);
                ceiling = go.transform;
            }

            var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var mf = ceiling.GetComponent<MeshFilter>();
            if (mf == null)
                mf = ceiling.gameObject.AddComponent<MeshFilter>();
            mf.sharedMesh = cube;

            var mr = ceiling.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = ceiling.gameObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = white;

            ceiling.position = new Vector3(0f, 2.90f, 0f);
            ceiling.rotation = Quaternion.identity;
            ceiling.localScale = new Vector3(11f, 0.13f, 11f);

            var meshCol = ceiling.GetComponent<MeshCollider>();
            if (meshCol != null)
                Object.DestroyImmediate(meshCol);
            if (ceiling.GetComponent<BoxCollider>() == null)
                ceiling.gameObject.AddComponent<BoxCollider>();
        }

        static void ApplySouthDoor(Transform env, Material white, Material grey, Material ledMat)
        {
            var oldSouth = env.Find("Wall_South");
            if (oldSouth != null)
                oldSouth.gameObject.SetActive(false);

            var existing = env.Find("SouthDoor");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var root = new GameObject("SouthDoor");
            root.transform.SetParent(env, true);
            root.transform.position = Vector3.zero;

            const float z = -5.25f;
            const float thick = 0.5f;
            const float yWall = 1.415f;
            const float wallH = 2.83f;
            const float doorMinX = 1.20f;
            const float doorMaxX = 2.10f;
            const float doorH = 2.20f;
            const float xMin = -5.5f;
            const float xMax = 5.5f;

            CreateStaticCube("Wall_South_West", root.transform,
                new Vector3((xMin + doorMinX) * 0.5f, yWall, z),
                new Vector3(doorMinX - xMin, wallH, thick), white);
            CreateStaticCube("Wall_South_East", root.transform,
                new Vector3((doorMaxX + xMax) * 0.5f, yWall, z),
                new Vector3(xMax - doorMaxX, wallH, thick), white);
            CreateStaticCube("Wall_South_Header", root.transform,
                new Vector3((doorMinX + doorMaxX) * 0.5f, (doorH + wallH) * 0.5f, z),
                new Vector3(doorMaxX - doorMinX, wallH - doorH, thick), white);

            var frame = new GameObject("DoorFrame");
            frame.transform.SetParent(root.transform, true);
            frame.transform.position = new Vector3(0f, 0f, -5.0f);
            var frameBody = frame.AddComponent<Rigidbody>();
            frameBody.isKinematic = true;
            frameBody.useGravity = false;

            CreateDecorCube("Post_Left", frame.transform, new Vector3(doorMinX, doorH * 0.5f, 0f), new Vector3(0.06f, doorH, 0.08f), white);
            CreateDecorCube("Post_Right", frame.transform, new Vector3(doorMaxX, doorH * 0.5f, 0f), new Vector3(0.06f, doorH, 0.08f), white);
            CreateDecorCube("Lintel", frame.transform, new Vector3((doorMinX + doorMaxX) * 0.5f, doorH, 0f), new Vector3(doorMaxX - doorMinX + 0.06f, 0.06f, 0.08f), white);

            const float leafW = 0.86f;
            const float leafH = 2.16f;
            const float leafT = 0.05f;
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door_Leaf";
            door.transform.SetParent(root.transform, true);
            door.transform.position = new Vector3(doorMaxX - leafW * 0.5f, leafH * 0.5f, -5.02f);
            door.transform.localScale = new Vector3(leafW, leafH, leafT);
            door.GetComponent<MeshRenderer>().sharedMaterial = white;
            var body = door.AddComponent<Rigidbody>();
            body.mass = 8f;
            var joint = door.AddComponent<HingeJoint>();
            joint.connectedBody = frameBody;
            joint.anchor = new Vector3(0.5f, 0f, 0f);
            joint.axis = Vector3.up;
            joint.autoConfigureConnectedAnchor = true;
            door.AddComponent<XRGrabInteractable>();
            var hingeGrab = door.AddComponent<HingeGrabDoor>();
            var so = new SerializedObject(hingeGrab);
            so.FindProperty("minAngle").floatValue = 0f;
            so.FindProperty("maxAngle").floatValue = 95f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var lever = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lever.name = "Lever";
            lever.transform.SetParent(door.transform, false);
            lever.transform.localPosition = new Vector3(-0.32f, 0f, 0.85f);
            lever.transform.localScale = new Vector3(0.16f / leafW, 0.018f / leafH, 0.028f / leafT);
            lever.GetComponent<MeshRenderer>().sharedMaterial = grey;
            Object.DestroyImmediate(lever.GetComponent<Collider>());

            var rose = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rose.name = "Rose";
            rose.transform.SetParent(door.transform, false);
            rose.transform.localPosition = new Vector3(-0.22f, 0f, 0.85f);
            rose.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rose.transform.localScale = new Vector3(0.045f / leafW, 0.012f / leafT, 0.045f / leafH);
            rose.GetComponent<MeshRenderer>().sharedMaterial = grey;
            Object.DestroyImmediate(rose.GetComponent<Collider>());

            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "LightSwitch";
            plate.transform.SetParent(root.transform, true);
            plate.transform.position = new Vector3(2.32f, 1.32f, -4.97f);
            plate.transform.localScale = new Vector3(0.10f, 0.10f, 0.02f);
            plate.GetComponent<MeshRenderer>().sharedMaterial = white;

            var led = GameObject.CreatePrimitive(PrimitiveType.Cube);
            led.name = "Led";
            led.transform.SetParent(plate.transform, false);
            led.transform.localPosition = new Vector3(0f, 0f, 0.7f);
            led.transform.localScale = new Vector3(0.18f, 0.18f, 0.2f);
            led.GetComponent<MeshRenderer>().sharedMaterial = ledMat;
            Object.DestroyImmediate(led.GetComponent<Collider>());

            plate.AddComponent<XRSimpleInteractable>();
            var tog = plate.AddComponent<ToggleLightSwitch>();
            var togSo = new SerializedObject(tog);
            togSo.FindProperty("led").objectReferenceValue = led.GetComponent<Renderer>();
            var lights = togSo.FindProperty("lights");
            var roomLight = Object.FindFirstObjectByType<Light>();
            lights.arraySize = roomLight != null ? 1 : 0;
            if (roomLight != null)
                lights.GetArrayElementAtIndex(0).objectReferenceValue = roomLight;
            togSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static void CreateStaticCube(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.isStatic = true;
        }

        static void CreateDecorCube(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }
    }
}
#endif
