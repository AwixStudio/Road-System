using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace DRS
{
    [RequireComponent(typeof(BezierCurveMB))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class RoadGenerator : MonoBehaviour
    {
        [Header("Road")]
        [SerializeField] private int leftLanes = 2;
        [SerializeField] private int rightLanes = 2;
        [SerializeField] private float curveAccuracy = 1f;
        [SerializeField] private bool leftRoadSide;
        [SerializeField] private float leftRoadSideWidth = 0;
        private float GetLeftRoadSideWidth => leftRoadSide ? leftRoadSideWidth : 0;
        [SerializeField] private bool rightRoadSide;
        [SerializeField] private float rightRoadSideWidth = 0;
        private float GetRightRoadSideWidth => rightRoadSide ? rightRoadSideWidth : 0;
        [Header("Extensions")]
        [SerializeField] private bool leftLaneExtension;
        [SerializeField] private AnimationCurve leftLaneExtensionCurve;
        [SerializeField] private bool leftLaneNarrowing;
        [SerializeField] private AnimationCurve leftLaneNarrowingCurve;
        [SerializeField] private bool rightLaneExtension;
        [SerializeField] private AnimationCurve rightLaneExtensionCurve;
        [SerializeField] private bool rightLaneNarrowing;
        [SerializeField] private AnimationCurve rightLaneNarrowingCurve;
        private float GetLeftExtenstionWidth(float distance) => (leftLaneExtension) ? (leftLaneNarrowing ? Mathf.Max(leftLaneExtensionCurve.Evaluate(distance), leftLaneNarrowingCurve.Evaluate(distance)) : leftLaneExtensionCurve.Evaluate(distance)) : (leftLaneNarrowing ? leftLaneNarrowingCurve.Evaluate(distance) : 0);
        private float GetRightExtenstionWidth(float distance) => (rightLaneExtension) ? (rightLaneNarrowing ? Mathf.Max(rightLaneExtensionCurve.Evaluate(distance), rightLaneNarrowingCurve.Evaluate(distance)) : rightLaneExtensionCurve.Evaluate(distance)) : (rightLaneNarrowing ? rightLaneNarrowingCurve.Evaluate(distance) : 0);

        [SerializeField] private bool greenLane;
        [SerializeField] private MinMaxCurve greenLaneWidth;
        private float GetRightGreenLaneWidth(float distance) => greenLane ? greenLaneWidth.curveMax.Evaluate(distance) : 0;
        private float GetLeftGreenLaneWidth(float distance) => greenLane ? greenLaneWidth.curveMin.Evaluate(distance) : 0;

        [Header("Lines")]
        public LineType[] lineTypes = new LineType[0];
        [SerializeField] private float dashedLineLong = 4;
        [SerializeField] private float lineAccuracy = 1f;

        private bool initialized;
        private MeshFilter meshFilter;
        private BezierCurveMB curveMB;
        private Transform helper;

        public const float LaneWidth = 3.5f;
        public const float LineHalfWidth = 0.1f;
        public const float DoubleLinesHalfSpace = 0.2f;

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                meshFilter = GetComponent<MeshFilter>();
                curveMB = GetComponent<BezierCurveMB>();

                GameObject helperGO = GameObject.FindGameObjectWithTag("Helper");
                if (helperGO == null)
                {
                    helperGO = new GameObject("Helper");
                    helperGO.tag = "Helper";
                }
                helper = helperGO.transform;

                GetComponent<DRS.BezierCurveMB>().OnChange += GenerateMesh;
                if (Selection.activeGameObject == gameObject)
                    GenerateMesh();

                initialized = true;
            }
        }

        private void GenerateMesh(int deep = 0)
        {
            float distance = 0;
            BezierCurve curve = new BezierCurve(curveMB);

            #region ValidateValues

            if (curveAccuracy <= 0)
                curveAccuracy = 0.1f;

            if (lineAccuracy <= 0)
                lineAccuracy = 0.1f;

            if (leftLanes < 0)
                leftLanes = 0;

            if (rightLanes < 0)
                rightLanes = 0;

            if(rightLanes == 0 || leftLanes == 0)
                greenLane = false;

            if (greenLaneWidth.mode != ParticleSystemCurveMode.TwoCurves)
            {
                AnimationCurve rightCurve = new AnimationCurve();
                rightCurve.AddKey(0, 1);
                rightCurve.AddKey(0.25f * curve.Distance, 1.5f);
                rightCurve.AddKey(0.75f * curve.Distance, 1.5f);
                rightCurve.AddKey(curve.Distance, 1);
                AnimationUtility.SetKeyRightTangentMode(rightCurve, 1, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyLeftTangentMode(rightCurve, 2, AnimationUtility.TangentMode.Linear);
                AnimationCurve leftCurve = new AnimationCurve();
                leftCurve.AddKey(0, -1);
                leftCurve.AddKey(0.25f * curve.Distance, -1.5f);
                leftCurve.AddKey(0.75f * curve.Distance, -1.5f);
                leftCurve.AddKey(curve.Distance, -1);                
                AnimationUtility.SetKeyRightTangentMode(leftCurve, 1, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyLeftTangentMode(leftCurve, 2, AnimationUtility.TangentMode.Linear);
                greenLaneWidth = new MinMaxCurve(1, leftCurve, rightCurve);
            }

            if(leftLaneExtensionCurve == null || leftLaneExtensionCurve.keys.Length == 0 || leftLaneExtensionCurve.keys[^1].value != LaneWidth)
            {
                leftLaneExtensionCurve = new AnimationCurve();
                leftLaneExtensionCurve.AddKey(0, 0);
                leftLaneExtensionCurve.AddKey(curve.Distance, LaneWidth);
            }

            if (leftLaneNarrowingCurve == null || leftLaneNarrowingCurve.keys.Length == 0 || leftLaneNarrowingCurve.keys[0].value != LaneWidth)
            {
                leftLaneNarrowingCurve = new AnimationCurve();
                leftLaneNarrowingCurve.AddKey(0, LaneWidth);
                leftLaneNarrowingCurve.AddKey(curve.Distance, 0);
            }

            if (rightLaneExtensionCurve == null || rightLaneExtensionCurve.keys.Length == 0 || rightLaneExtensionCurve.keys[^1].value != LaneWidth)
            {
                rightLaneExtensionCurve = new AnimationCurve();
                rightLaneExtensionCurve.AddKey(0, 0);
                rightLaneExtensionCurve.AddKey(curve.Distance, LaneWidth);
            }

            if (rightLaneNarrowingCurve == null || rightLaneNarrowingCurve.keys.Length == 0 || rightLaneNarrowingCurve.keys[0].value != LaneWidth)
            {
                rightLaneNarrowingCurve = new AnimationCurve();
                rightLaneNarrowingCurve.AddKey(0, LaneWidth);
                rightLaneNarrowingCurve.AddKey(curve.Distance, 0);
            }

            if (lineTypes.Length != LineTypesCount())
                SetDefaultLineTypes();

            #endregion

            #region GenerateAsphalt
            List<Vertex> vertices = new List<Vertex>();
            List<Vertex> leftOutsideEdgeVertices = new List<Vertex>();
            List<Vertex> leftInsideEdgeVertices = new List<Vertex>();
            List<Vertex> rightInsideEdgeVertices = new List<Vertex>();
            List<Vertex> rightOutsideEdgeVertices = new List<Vertex>();
            List<Triangle> triangles = new List<Triangle>();
            List<Vector3> upDirectionForVertex = new List<Vector3>();

            while (distance <= curve.Distance)
            {
                if (greenLane)
                {
                    Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                    helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);

                    helper.position = curvePos;
                    helper.position += -helper.right * leftLanes * LaneWidth + -helper.right * GetLeftRoadSideWidth;
                    helper.position += helper.right * GetLeftGreenLaneWidth(distance);
                    helper.position += -helper.right * GetLeftExtenstionWidth(curve.Distance - distance);

                    vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                    leftOutsideEdgeVertices.AddLast(vertices);

                    helper.position = curvePos;
                    helper.position += helper.right * GetLeftGreenLaneWidth(distance);

                    vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                    leftInsideEdgeVertices.AddLast(vertices);

                    helper.position = curvePos;
                    helper.position += helper.right * GetRightGreenLaneWidth(distance);

                    vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                    rightInsideEdgeVertices.AddLast(vertices);

                    helper.position = curvePos;
                    helper.position += helper.right * rightLanes * LaneWidth + helper.right * GetRightRoadSideWidth;
                    helper.position += helper.right * GetRightGreenLaneWidth(distance);
                    helper.position += helper.right * GetRightExtenstionWidth(distance);

                    vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                    rightOutsideEdgeVertices.AddLast(vertices);

                    helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                    upDirectionForVertex.Add(helper.up);
                }
                else
                {
                    Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                    helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);

                    helper.position = curvePos;
                    helper.position += -helper.right * leftLanes * LaneWidth + -helper.right * GetLeftRoadSideWidth;
                    helper.position += -helper.right * GetLeftExtenstionWidth(curve.Distance - distance);

                    vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                    leftOutsideEdgeVertices.AddLast(vertices);

                    helper.position = curvePos;
                    helper.position += helper.right * rightLanes * LaneWidth + helper.right * GetRightRoadSideWidth;
                    helper.position += helper.right * GetRightExtenstionWidth(distance);

                    vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                    rightOutsideEdgeVertices.AddLast(vertices);

                    helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                    upDirectionForVertex.Add(helper.up);
                }

                if (distance == curve.Distance)
                    break;
                distance += curveAccuracy;
                if (distance > curve.Distance)
                    distance = curve.Distance;
            }

            if (greenLane)
            {
                for (int i = 0; i < leftOutsideEdgeVertices.Count - 1; i++)
                {
                    triangles.Add(new Triangle(new Vertex[] { leftOutsideEdgeVertices[i], leftOutsideEdgeVertices[i + 1], leftInsideEdgeVertices[i] }, upDirectionForVertex[i]));
                    triangles.Add(new Triangle(new Vertex[] { leftOutsideEdgeVertices[i + 1], leftInsideEdgeVertices[i + 1], leftInsideEdgeVertices[i] }, upDirectionForVertex[i]));
                    
                    triangles.Add(new Triangle(new Vertex[] { rightInsideEdgeVertices[i], rightInsideEdgeVertices[i + 1], rightOutsideEdgeVertices[i] }, upDirectionForVertex[i]));
                    triangles.Add(new Triangle(new Vertex[] { rightInsideEdgeVertices[i + 1], rightOutsideEdgeVertices[i + 1], rightOutsideEdgeVertices[i] }, upDirectionForVertex[i]));
                }
            }
            else
            {
                for (int i = 0; i < leftOutsideEdgeVertices.Count - 1; i++)
                {
                    triangles.Add(new Triangle(new Vertex[] { leftOutsideEdgeVertices[i], leftOutsideEdgeVertices[i + 1], rightOutsideEdgeVertices[i] }, upDirectionForVertex[i]));
                    triangles.Add(new Triangle(new Vertex[] { leftOutsideEdgeVertices[i + 1], rightOutsideEdgeVertices[i + 1], rightOutsideEdgeVertices[i] }, upDirectionForVertex[i]));
                }                
            }
            #endregion
            #region GenerateLines
            List<Vertex> lines_leftVertices = new List<Vertex>();
            List<Vertex> lines_rightVertices = new List<Vertex>();
            List<Triangle> lines_triangles = new List<Triangle>();
            List<Vector3> lines_upDirectionForVertex = new List<Vector3>();

            int lineVerticesToTriangleIndex = 0;
            for (int i = 0; i < lineTypes.Length; i++)
            {
                if (lineTypes[i] == LineType.None)
                    continue;

                distance = 0;
                float targetDistance = 0;
                RealDistanceConverter realDistanceConverter = new RealDistanceConverter(curve, -leftLanes * LaneWidth + LaneWidth + i * LaneWidth);
                if (lineTypes[i] == LineType.Dashed)
                {
                    targetDistance = dashedLineLong * 0.5f;
                    distance = realDistanceConverter.FindRealDistance(targetDistance);
                }

                while (distance <= curve.Distance)
                {                    
                    if (lineTypes[i] == LineType.Dashed)
                    {
                        float endOfDashRealDistance = targetDistance + dashedLineLong;
                        Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                        helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);
                        helper.position = curvePos;
                        helper.position += -helper.right * leftLanes * LaneWidth;
                        helper.position += helper.right * (GetLeftRoadSideWidth == 0 ? i + 1 : i) * LaneWidth;
                        helper.position += helper.up * 0.03f;
                        helper.position += helper.right * LineHalfWidth;
                        if (IsDrawingLinesOnLeft(i))
                            helper.position += helper.right * GetLeftGreenLaneWidth(distance);
                        else
                            helper.position += helper.right * GetRightGreenLaneWidth(distance);

                        if (GetLeftRoadSideWidth > 0 && i == 0)
                            helper.position += -helper.right * GetLeftExtenstionWidth(curve.Distance - distance);

                        if (GetRightRoadSideWidth > 0 && i == lineTypes.Length - 1)
                            helper.position += helper.right * GetRightExtenstionWidth(distance);

                        while (distance < curve.Distance && targetDistance <= endOfDashRealDistance)
                        {
                            curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                            helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);
                            helper.position = curvePos;
                            helper.position += -helper.right * leftLanes * LaneWidth;
                            helper.position += helper.right * (GetLeftRoadSideWidth == 0 ? i + 1 : i) * LaneWidth;
                            helper.position += helper.up * 0.03f;
                            if (IsDrawingLinesOnLeft(i))
                                helper.position += helper.right * GetLeftGreenLaneWidth(distance);
                            else
                                helper.position += helper.right * GetRightGreenLaneWidth(distance);

                            if (GetLeftRoadSideWidth > 0 && i == 0)
                                helper.position += -helper.right * GetLeftExtenstionWidth(curve.Distance - distance);

                            if (GetRightRoadSideWidth > 0 && i == lineTypes.Length - 1)
                                helper.position += helper.right * GetRightExtenstionWidth(distance);

                            helper.position += -helper.right * LineHalfWidth;

                            vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                            lines_leftVertices.AddLast(vertices);

                            helper.position += helper.right * LineHalfWidth * 2;

                            vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                            lines_rightVertices.AddLast(vertices);

                            helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                            lines_upDirectionForVertex.Add(helper.up);

                            if (targetDistance == endOfDashRealDistance)
                                break;
                            targetDistance += lineAccuracy;
                            if (targetDistance > endOfDashRealDistance)
                                targetDistance = endOfDashRealDistance;
                            distance = realDistanceConverter.FindRealDistance(targetDistance);
                        }

                        for (int j = lineVerticesToTriangleIndex; j < lines_leftVertices.Count - 1; j++)
                        {
                            lines_triangles.Add(new Triangle(new Vertex[] { lines_leftVertices[j], lines_leftVertices[j + 1], lines_rightVertices[j] }, lines_upDirectionForVertex[j]));
                            lines_triangles.Add(new Triangle(new Vertex[] { lines_leftVertices[j + 1], lines_rightVertices[j + 1], lines_rightVertices[j] }, lines_upDirectionForVertex[j]));
                        }
                        lineVerticesToTriangleIndex = lines_leftVertices.Count;
                    }
                    else if (lineTypes[i] == LineType.Single)
                    {
                        Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                        helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);
                        helper.position = curvePos;
                        helper.position += -helper.right * leftLanes * LaneWidth;
                        helper.position += helper.right * (GetLeftRoadSideWidth == 0? i+1: i) * LaneWidth;
                        helper.position += helper.up * 0.03f;
                        if (IsDrawingLinesOnLeft(i))
                            helper.position += helper.right * GetLeftGreenLaneWidth(distance);
                        else
                            helper.position += helper.right * GetRightGreenLaneWidth(distance);

                        if (GetLeftRoadSideWidth > 0 && i == 0)
                            helper.position += -helper.right * GetLeftExtenstionWidth(curve.Distance - distance);

                        if (GetRightRoadSideWidth > 0 && i == lineTypes.Length - 1)
                            helper.position += helper.right * GetRightExtenstionWidth(distance);
                        
                        helper.position += -helper.right * LineHalfWidth;

                        vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                        lines_leftVertices.AddLast(vertices);

                        helper.position += helper.right * LineHalfWidth * 2;

                        vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                        lines_rightVertices.AddLast(vertices);

                        helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                        lines_upDirectionForVertex.Add(helper.up);
                    }
                    else if (lineTypes[i] == LineType.Double)
                    {
                        Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                        helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);
                        helper.position = curvePos;
                        helper.position += -helper.right * leftLanes * LaneWidth - helper.right * DoubleLinesHalfSpace;
                        helper.position += helper.right * (GetLeftRoadSideWidth == 0 ? i + 1 : i) * LaneWidth;
                        helper.position += helper.up * 0.03f;
                        if (IsDrawingLinesOnLeft(i))
                            helper.position += helper.right * GetLeftGreenLaneWidth(distance);
                        else
                            helper.position += helper.right * GetRightGreenLaneWidth(distance);

                        if (GetLeftRoadSideWidth > 0 && i == 0)
                            helper.position += -helper.right * GetLeftExtenstionWidth(curve.Distance - distance);

                        if (GetRightRoadSideWidth > 0 && i == lineTypes.Length - 1)
                            helper.position += helper.right * GetRightExtenstionWidth(distance);

                        helper.position += -helper.right * LineHalfWidth;

                        vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                        lines_leftVertices.AddLast(vertices);

                        helper.position += helper.right * LineHalfWidth * 2;

                        vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                        lines_rightVertices.AddLast(vertices);

                        helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                        lines_upDirectionForVertex.Add(helper.up);

                        helper.position += helper.right * DoubleLinesHalfSpace;

                        vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                        lines_leftVertices.AddLast(vertices);

                        helper.position += helper.right * LineHalfWidth * 2;

                        vertices.NewVertex(transform.InverseTransformPoint(helper.position));
                        lines_rightVertices.AddLast(vertices);

                        helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                        lines_upDirectionForVertex.Add(helper.up);
                        }
                    

                    if (distance >= curve.Distance)
                        break;
                    targetDistance += lineTypes[i] == LineType.Dashed ? dashedLineLong : lineAccuracy;
                    distance = realDistanceConverter.FindRealDistance(targetDistance);
                    if (distance > curve.Distance)
                        distance = curve.Distance;
                }

                if (lineTypes[i] == LineType.Single)
                {
                    for (int j = lineVerticesToTriangleIndex; j < lines_leftVertices.Count - 1; j++)
                    {
                        lines_triangles.Add(new Triangle(new Vertex[] { lines_leftVertices[j], lines_leftVertices[j + 1], lines_rightVertices[j] }, lines_upDirectionForVertex[j]));
                        lines_triangles.Add(new Triangle(new Vertex[] { lines_leftVertices[j + 1], lines_rightVertices[j + 1], lines_rightVertices[j] }, lines_upDirectionForVertex[j]));
                    }
                    lineVerticesToTriangleIndex = lines_leftVertices.Count;
                }
                else if (lineTypes[i] == LineType.Double)
                {
                    for (int j = lineVerticesToTriangleIndex; j < lines_leftVertices.Count - 2; j++)
                    {
                        lines_triangles.Add(new Triangle(new Vertex[] { lines_leftVertices[j], lines_leftVertices[j + 2], lines_rightVertices[j] }, lines_upDirectionForVertex[j]));
                        lines_triangles.Add(new Triangle(new Vertex[] { lines_leftVertices[j + 2], lines_rightVertices[j + 2], lines_rightVertices[j] }, lines_upDirectionForVertex[j]));
                    }
                    lineVerticesToTriangleIndex = lines_leftVertices.Count;
                }
            }
            #endregion

            Mesh mesh = new Mesh();
            mesh.subMeshCount = 2;

            mesh.vertices = vertices.ListToVectorArray();
            mesh.uv = new Vector2[vertices.Count];

            mesh.SetTriangles(triangles.ListToIntArray(), 0);
            mesh.SetTriangles(lines_triangles.ListToIntArray(), 1);

            mesh.RecalculateTangents();
            mesh.RecalculateNormals();

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            Material[] materials = new Material[2] { RoadSystemSettings.GetOrCreateSettings().AsphaltMaterial, RoadSystemSettings.GetOrCreateSettings().RoadLineMaterial };
            renderer.sharedMaterials = materials;

            meshFilter.mesh = mesh;
        }

        private void OnValidate() => UnityEditor.EditorApplication.delayCall += _OnValidate;

        private void _OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= _OnValidate;
            if (this == null) return;
            if (initialized && !Application.isPlaying && Selection.activeGameObject == gameObject)
                GenerateMesh();
        }

        [ContextMenu("Debug Vertices")]
        public void DebugVertices()
        {
            for (int i = 0; i < meshFilter.sharedMesh.vertexCount; i++)
            {
                GameObject spawned = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spawned.transform.position = meshFilter.sharedMesh.vertices[i];
            }
        }

        [ContextMenu("Regenerate")]
        public void Regenerate() => GenerateMesh();

        private void SetDefaultLineTypes()
        {
            lineTypes = new LineType[LineTypesCount()];
            for (int i = 0; i < lineTypes.Length; i++)
                lineTypes[i] = LineType.Dashed;

            if (rightLanes > 0 && leftLanes > 0)
            {
                if(greenLane)
                    lineTypes[leftLanes - 1 + (GetLeftRoadSideWidth > 0? 1:0)] = LineType.None;
                else
                    lineTypes[leftLanes - 1 + (GetLeftRoadSideWidth > 0? 1:0)] = LineType.Single;
            }

            if (GetLeftRoadSideWidth > 0)
                lineTypes[0] = LineType.Single;

            if (GetRightRoadSideWidth > 0)
                lineTypes[^1] = LineType.Single;
            
            EditorUtility.SetDirty(this);
        }

        private int LineTypesCount()
        {
            if(leftLanes == 0 && rightLanes == 0)
                return 0;

            return rightLanes + leftLanes - 1 + ((GetLeftRoadSideWidth > 0) ? 1 : 0) + ((GetRightRoadSideWidth > 0) ? 1 : 0);
        }

        private bool IsDrawingLinesOnLeft(int i)
        {
            int midLineIndex = (leftLanes - 1 + (GetLeftRoadSideWidth > 0 ? 1 : 0));
            return i < midLineIndex;
        }

        private void DebugVertex(Vertex v)
        {
            GameObject spawned = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spawned.transform.position = v.pos;
        }

        private class RealDistanceConverter
        {
            private BezierCurve curve;
            private float offset;

            private float cathedDistance;
            private float cathedRealDistance;
            private Vector3 cathedPosition;
            private Transform helper;

            public RealDistanceConverter(BezierCurve curve, float offset)
            {
                this.curve = curve;
                this.offset = offset;

                GameObject helperGO = GameObject.FindGameObjectWithTag("Helper");
                if (helperGO == null)
                {
                    helperGO = new GameObject("Helper");
                    helperGO.tag = "Helper";
                }
                helper = helperGO.transform;

                cathedDistance = 0;
                cathedRealDistance = 0;
                helper.position = curve.GetPositionOfBezierCurveByDistance(0);
                helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(0), 0);
                helper.position += helper.right * offset;
                cathedPosition = helper.position;
            }

            public float FindRealDistance(float target)
            {
                if (target == 0)
                    return 0;

                float distance = cathedDistance;
                while (distance < curve.Distance)
                {
                    helper.position = curve.GetPositionOfBezierCurveByDistance(distance);
                    helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);
                    helper.position += helper.right * offset;
                    cathedRealDistance += Vector3.Distance(helper.position, cathedPosition);
                    cathedPosition = helper.position;                        
                    cathedDistance = distance;
                    
                    if(cathedRealDistance >= target)                    
                        return distance;                                    
                        
                    distance += 0.05f;
                }

                return curve.Distance;
            }        
        }
    }
}