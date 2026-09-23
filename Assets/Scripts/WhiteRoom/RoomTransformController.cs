using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteRoom
{
    /// <summary>
    /// Procedurally morphs the white interactive room into the studio house and back.
    /// Walls reshape over time instead of a hard swap.
    /// </summary>
    public class RoomTransformController : MonoBehaviour
    {
        [SerializeField] GameObject whiteRoomRoot;
        [SerializeField] GameObject interactablesRoot;
        [SerializeField] GameObject uiRoot;
        [SerializeField] GameObject houseRoot;
        [SerializeField] Transform playerRoot;
        [SerializeField] Vector3 houseSpawnPosition = new Vector3(0f, 0f, -4.4f);
        [SerializeField] Vector3 houseSpawnEuler = Vector3.zero;
        [SerializeField] Vector3 whiteSpawnPosition = new Vector3(0f, 0f, -0.5f);
        [SerializeField] Vector3 whiteSpawnEuler = Vector3.zero;
        [SerializeField] bool hideUiOnTransform = true;
        [SerializeField] bool hideInteractablesOnTransform = true;
        [SerializeField] float morphDuration = 2.8f;
        [SerializeField, Range(0f, 0.9f)] float furniturePhase = 0.55f;
        [SerializeField] AnimationCurve morphCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        readonly List<MorphPiece> pieces = new List<MorphPiece>();
        readonly List<Renderer> whiteRenderers = new List<Renderer>();
        readonly List<Renderer> houseRenderers = new List<Renderer>();
        readonly List<GameObject> proxies = new List<GameObject>();

        Material proxyMatTemplate;
        Coroutine morphRoutine;
        bool isHouse;
        bool morphing;

        public bool IsHouse => isHouse;
        public bool IsMorphing => morphing;

        struct MorphPiece
        {
            public Transform from;
            public Transform to;
            public Color fromColor;
            public Color toColor;
            public bool growFromZero;
        }

        void Awake()
        {
            EnsureMaterial();
            CacheRenderers();
            BuildMorphMap();
            ApplyImmediateWhiteRoom();
        }

        void OnDestroy()
        {
            ClearProxies();
        }

        public void TransformToHouse()
        {
            if (morphing || isHouse)
                return;
            BeginMorph(true);
        }

        public void TransformToWhiteRoom()
        {
            if (morphing || !isHouse)
                return;
            BeginMorph(false);
        }

        public void ToggleTransform()
        {
            if (isHouse)
                TransformToWhiteRoom();
            else
                TransformToHouse();
        }

        void BeginMorph(bool toHouse)
        {
            if (morphRoutine != null)
                StopCoroutine(morphRoutine);
            morphRoutine = StartCoroutine(MorphRoutine(toHouse));
        }

        IEnumerator MorphRoutine(bool toHouse)
        {
            morphing = true;

            if (whiteRoomRoot != null)
                whiteRoomRoot.SetActive(true);
            if (houseRoot != null)
                houseRoot.SetActive(true);

            if (toHouse)
            {
                if (hideInteractablesOnTransform && interactablesRoot != null)
                    interactablesRoot.SetActive(false);
                if (hideUiOnTransform && uiRoot != null)
                    uiRoot.SetActive(false);
            }
            else
            {
                if (interactablesRoot != null)
                    interactablesRoot.SetActive(true);
                if (uiRoot != null)
                    uiRoot.SetActive(true);
            }

            SetRenderersEnabled(whiteRenderers, false);
            SetRenderersEnabled(houseRenderers, false);
            SetHouseFurnitureVisible(false);
            CreateProxies(toHouse);

            float duration = Mathf.Max(0.2f, morphDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float u = morphCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
                AnimateProxies(u, toHouse);
                AnimateFurniture(u, toHouse);
                AnimatePlayer(u, toHouse);
                yield return null;
            }

            AnimateProxies(1f, toHouse);
            ClearProxies();

            isHouse = toHouse;
            if (toHouse)
                FinishAsHouse();
            else
                FinishAsWhiteRoom();

            morphing = false;
            morphRoutine = null;
        }

        void FinishAsHouse()
        {
            if (whiteRoomRoot != null)
                whiteRoomRoot.SetActive(false);
            if (houseRoot != null)
                houseRoot.SetActive(true);
            SetRenderersEnabled(houseRenderers, true);
            SetHouseFurnitureVisible(true);
            SnapPlayer(true);
        }

        void FinishAsWhiteRoom()
        {
            if (houseRoot != null)
                houseRoot.SetActive(false);
            if (whiteRoomRoot != null)
                whiteRoomRoot.SetActive(true);
            SetRenderersEnabled(whiteRenderers, true);
            if (interactablesRoot != null)
                interactablesRoot.SetActive(true);
            if (uiRoot != null)
                uiRoot.SetActive(true);
            SnapPlayer(false);
        }

        void ApplyImmediateWhiteRoom()
        {
            isHouse = false;
            ClearProxies();
            if (houseRoot != null)
                houseRoot.SetActive(false);
            if (whiteRoomRoot != null)
                whiteRoomRoot.SetActive(true);
            if (interactablesRoot != null)
                interactablesRoot.SetActive(true);
            if (uiRoot != null)
                uiRoot.SetActive(true);
            SetRenderersEnabled(whiteRenderers, true);
        }

        void CacheRenderers()
        {
            whiteRenderers.Clear();
            houseRenderers.Clear();
            if (whiteRoomRoot != null)
                whiteRoomRoot.GetComponentsInChildren(true, whiteRenderers);
            if (houseRoot != null)
                houseRoot.GetComponentsInChildren(true, houseRenderers);
        }

        void BuildMorphMap()
        {
            pieces.Clear();

            Color white = new Color(0.92f, 0.92f, 0.9f);
            Color warm = new Color(0.78f, 0.74f, 0.67f);
            Color wood = new Color(0.62f, 0.42f, 0.23f);
            Color glass = new Color(0.48f, 0.72f, 0.78f);
            Color dark = new Color(0.18f, 0.16f, 0.13f);
            Color tile = new Color(0.3f, 0.34f, 0.36f);

            AddPair("Floor", "Floor_Main", white, wood);
            AddPair("Ceiling", "Ceiling", white, wood);
            AddPair("Wall_Back", "Wall_West", white, warm);
            AddPair("Wall_Front", "Wall_East", white, warm);
            AddPair("Wall_Left", "Wall_North_West", white, warm);
            AddPair("Wall_Left", "Wall_North_East", white, warm);
            AddPair("Wall_Left", "North_Glass_Door", glass, glass);
            AddPair("Wall_South_West", "Wall_South_West", white, warm);
            AddPair("Wall_South_East", "Wall_South_East", white, warm);
            AddPair("Wall_South", "Wall_South_West", white, warm);
            AddPair("Wall_South", "Wall_South_East", white, warm);
            AddPair("SouthDoor", "EntryDoor", dark, dark);
            AddPair("Wall_South", "EntryDoor", dark, dark);

            AddGrow("Bathroom_Wall_Vertical", warm);
            AddGrow("Bathroom_Wall_Horizontal_Left", warm);
            AddGrow("Bathroom_Wall_Horizontal_Right", warm);
            AddGrow("BathroomDoor", dark);
            AddGrow("Floor_Bathroom_Tile", tile);
            AddGrow("Window_Mullion_Left", dark);
            AddGrow("Window_Mullion_Right", dark);
        }

        void AddPair(string fromName, string toName, Color fromColor, Color toColor)
        {
            var from = FindDeep(whiteRoomRoot != null ? whiteRoomRoot.transform : null, fromName);
            var to = FindDeep(houseRoot != null ? houseRoot.transform : null, toName);
            if (from == null || to == null)
                return;

            pieces.Add(new MorphPiece
            {
                from = from,
                to = to,
                fromColor = fromColor,
                toColor = toColor
            });
        }

        void AddGrow(string toName, Color color)
        {
            var to = FindDeep(houseRoot != null ? houseRoot.transform : null, toName);
            if (to == null)
                return;

            pieces.Add(new MorphPiece
            {
                from = null,
                to = to,
                fromColor = color,
                toColor = color,
                growFromZero = true
            });
        }

        Transform FindFurnitureRoot()
        {
            if (houseRoot == null)
                return null;
            var direct = houseRoot.transform.Find("Furniture");
            if (direct != null)
                return direct;
            return FindDeep(houseRoot.transform, "Furniture");
        }

        static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDeep(root.GetChild(i), name);
                if (found != null)
                    return found;
            }
            return null;
        }

        void CreateProxies(bool toHouse)
        {
            ClearProxies();
            EnsureMaterial();

            for (int i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                var proxy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                proxy.name = "MorphProxy_" + i;
                proxy.transform.SetParent(transform, true);

                var col = proxy.GetComponent<Collider>();
                if (col != null)
                    Destroy(col);

                var rend = proxy.GetComponent<MeshRenderer>();
                rend.sharedMaterial = new Material(proxyMatTemplate);

                GetEndpoints(piece, toHouse,
                    out var startPos, out var startRot, out var startScale, out var startColor,
                    out _, out _, out _, out _);

                proxy.transform.SetPositionAndRotation(startPos, startRot);
                proxy.transform.localScale = AbsScale(startScale);
                SetColor(rend.sharedMaterial, startColor);
                proxies.Add(proxy);
            }
        }

        void AnimateProxies(float u, bool toHouse)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                if (i >= proxies.Count || proxies[i] == null)
                    continue;

                var piece = pieces[i];
                GetEndpoints(piece, toHouse,
                    out var aPos, out var aRot, out var aScale, out var aColor,
                    out var bPos, out var bRot, out var bScale, out var bColor);

                float localU = u;
                if (piece.growFromZero)
                {
                    float delayed = Mathf.InverseLerp(0.25f, 1f, u);
                    localU = morphCurve.Evaluate(Mathf.Clamp01(delayed));
                }

                var proxy = proxies[i].transform;
                proxy.SetPositionAndRotation(
                    Vector3.LerpUnclamped(aPos, bPos, localU),
                    Quaternion.SlerpUnclamped(aRot, bRot, localU));
                proxy.localScale = AbsScale(Vector3.LerpUnclamped(aScale, bScale, localU));

                var rend = proxies[i].GetComponent<MeshRenderer>();
                if (rend != null && rend.sharedMaterial != null)
                    SetColor(rend.sharedMaterial, Color.Lerp(aColor, bColor, localU));
            }
        }

        void GetEndpoints(
            MorphPiece piece,
            bool toHouse,
            out Vector3 aPos, out Quaternion aRot, out Vector3 aScale, out Color aColor,
            out Vector3 bPos, out Quaternion bRot, out Vector3 bScale, out Color bColor)
        {
            if (piece.growFromZero)
            {
                var target = piece.to;
                var tiny = Vector3.Scale(EstimateWorldScale(target), new Vector3(0.02f, 0.02f, 0.02f));
                var full = EstimateWorldScale(target);

                if (toHouse)
                {
                    aPos = target.position; aRot = target.rotation; aScale = tiny; aColor = piece.fromColor;
                    bPos = target.position; bRot = target.rotation; bScale = full; bColor = piece.toColor;
                }
                else
                {
                    aPos = target.position; aRot = target.rotation; aScale = full; aColor = piece.toColor;
                    bPos = target.position; bRot = target.rotation; bScale = tiny; bColor = piece.fromColor;
                }
                return;
            }

            var startPos = piece.from.position;
            var startRot = piece.from.rotation;
            var startScale = EstimateWorldScale(piece.from);
            var endPos = piece.to.position;
            var endRot = piece.to.rotation;
            var endScale = EstimateWorldScale(piece.to);

            if (toHouse)
            {
                aPos = startPos; aRot = startRot; aScale = startScale; aColor = piece.fromColor;
                bPos = endPos; bRot = endRot; bScale = endScale; bColor = piece.toColor;
            }
            else
            {
                aPos = endPos; aRot = endRot; aScale = endScale; aColor = piece.toColor;
                bPos = startPos; bRot = startRot; bScale = startScale; bColor = piece.fromColor;
            }
        }

        static Vector3 EstimateWorldScale(Transform t)
        {
            var rend = t.GetComponent<Renderer>();
            if (rend != null)
            {
                var size = rend.bounds.size;
                return new Vector3(
                    Mathf.Max(0.05f, size.x),
                    Mathf.Max(0.05f, size.y),
                    Mathf.Max(0.05f, size.z));
            }
            return t.lossyScale;
        }

        static Vector3 AbsScale(Vector3 scale)
        {
            return new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        }

        void AnimateFurniture(float u, bool toHouse)
        {
            var furniture = FindFurnitureRoot();
            if (furniture == null)
                return;

            float local = 0f;
            if (u >= furniturePhase)
                local = Mathf.InverseLerp(furniturePhase, 1f, u);

            float visible = toHouse ? local : 1f - local;
            furniture.gameObject.SetActive(visible > 0.01f);
            furniture.localScale = Vector3.one * Mathf.Lerp(0.02f, 1f, visible);
        }

        void SetHouseFurnitureVisible(bool visible)
        {
            var furniture = FindFurnitureRoot();
            if (furniture == null)
                return;
            furniture.gameObject.SetActive(visible);
            furniture.localScale = Vector3.one;
        }

        void AnimatePlayer(float u, bool toHouse)
        {
            if (playerRoot == null)
                return;

            Vector3 fromPos = toHouse ? whiteSpawnPosition : houseSpawnPosition;
            Vector3 toPos = toHouse ? houseSpawnPosition : whiteSpawnPosition;
            Quaternion fromRot = Quaternion.Euler(toHouse ? whiteSpawnEuler : houseSpawnEuler);
            Quaternion toRot = Quaternion.Euler(toHouse ? houseSpawnEuler : whiteSpawnEuler);
            playerRoot.position = Vector3.LerpUnclamped(fromPos, toPos, u);
            playerRoot.rotation = Quaternion.SlerpUnclamped(fromRot, toRot, u);
        }

        void SnapPlayer(bool toHouse)
        {
            if (playerRoot == null)
                return;
            playerRoot.position = toHouse ? houseSpawnPosition : whiteSpawnPosition;
            playerRoot.rotation = Quaternion.Euler(toHouse ? houseSpawnEuler : whiteSpawnEuler);
        }

        static void SetRenderersEnabled(List<Renderer> renderers, bool enabled)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = enabled;
            }
        }

        void ClearProxies()
        {
            for (int i = 0; i < proxies.Count; i++)
            {
                if (proxies[i] != null)
                    DestroyImmediateSafe(proxies[i]);
            }
            proxies.Clear();
        }

        static void DestroyImmediateSafe(Object obj)
        {
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }

        void EnsureMaterial()
        {
            if (proxyMatTemplate != null)
                return;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("URP/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            proxyMatTemplate = new Material(shader);
        }

        static void SetColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
        }
    }
}
