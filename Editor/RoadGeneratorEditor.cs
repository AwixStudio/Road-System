using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace DRS
{
    [CustomEditor(typeof(RoadGenerator))]
    [InitializeOnLoad]
    public class RoadGeneratorEditor : Editor
    {
        public const float LaneWidth = 3.5f;
        public const float LineHalfWidth = 0.1f;
        public const float DoubleLinesHalfSpace = 0.2f;

        private static Transform helper;

        SerializedProperty leftLanes;
        SerializedProperty rightLanes;
        SerializedProperty curveAccuracy;
        SerializedProperty leftRoadSide;
        SerializedProperty leftRoadSideWidth;
        SerializedProperty rightRoadSide;
        SerializedProperty rightRoadSideWidth;
        
        SerializedProperty leftLaneExtension;
        SerializedProperty leftLaneExtensionCurve;
        SerializedProperty leftLaneNarrowing;
        SerializedProperty leftLaneNarrowingCurve;
        SerializedProperty rightLaneExtension;
        SerializedProperty rightLaneExtensionCurve;
        SerializedProperty rightLaneNarrowing;
        SerializedProperty rightLaneNarrowingCurve;
        
        SerializedProperty greenLane;
        SerializedProperty greenLaneWidth;
        
        SerializedProperty lineTypes;
        SerializedProperty dashedLineLong;
        SerializedProperty lineAccuracy;

        private void OnEnable()
        {
            leftLanes = serializedObject.FindProperty("LeftLanes");
            rightLanes = serializedObject.FindProperty("RightLanes");
            curveAccuracy = serializedObject.FindProperty("CurveAccuracy");
            leftRoadSide = serializedObject.FindProperty("LeftRoadSide");
            leftRoadSideWidth = serializedObject.FindProperty("LeftRoadSideWidth");
            rightRoadSide = serializedObject.FindProperty("RightRoadSide");
            rightRoadSideWidth = serializedObject.FindProperty("RightRoadSideWidth");

            leftLaneExtension = serializedObject.FindProperty("LeftLaneExtension");
            leftLaneExtensionCurve = serializedObject.FindProperty("LeftLaneExtensionCurve");
            leftLaneNarrowing = serializedObject.FindProperty("LeftLaneNarrowing");
            leftLaneNarrowingCurve = serializedObject.FindProperty("LeftLaneNarrowingCurve");
            rightLaneExtension = serializedObject.FindProperty("RightLaneExtension");
            rightLaneExtensionCurve = serializedObject.FindProperty("RightLaneExtensionCurve");
            rightLaneNarrowing = serializedObject.FindProperty("RightLaneNarrowing");
            rightLaneNarrowingCurve = serializedObject.FindProperty("RightLaneNarrowingCurve");

            greenLane = serializedObject.FindProperty("GreenLane");
            greenLaneWidth = serializedObject.FindProperty("GreenLaneWidth");

            lineTypes = serializedObject.FindProperty("LineTypes");
            dashedLineLong = serializedObject.FindProperty("DashedLineLong");
            lineAccuracy = serializedObject.FindProperty("LineAccuracy");
        }

        public override void OnInspectorGUI()
        {
            RoadGenerator script = (RoadGenerator)target;

            EditorGUILayout.PropertyField(leftLanes);
            EditorGUILayout.PropertyField(rightLanes);
            EditorGUILayout.PropertyField(curveAccuracy);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(leftRoadSide);            
            if(leftRoadSide.boolValue)            
                EditorGUILayout.PropertyField(leftRoadSideWidth);

            EditorGUILayout.PropertyField(rightRoadSide);
            if(rightRoadSide.boolValue)
                EditorGUILayout.PropertyField(rightRoadSideWidth);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(leftLaneExtension);
            if (leftLaneExtension.boolValue)
                EditorGUILayout.PropertyField(leftLaneExtensionCurve);

            EditorGUILayout.PropertyField(leftLaneNarrowing);
            if (leftLaneNarrowing.boolValue)
                EditorGUILayout.PropertyField(leftLaneNarrowingCurve);

            EditorGUILayout.PropertyField(rightLaneExtension);
            if (rightLaneExtension.boolValue)
                EditorGUILayout.PropertyField(rightLaneExtensionCurve);

            EditorGUILayout.PropertyField(rightLaneNarrowing);
            if (rightLaneNarrowing.boolValue)
                EditorGUILayout.PropertyField(rightLaneNarrowingCurve);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(greenLane);
            if (greenLane.boolValue)
                EditorGUILayout.PropertyField(greenLaneWidth);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(lineTypes);
            EditorGUILayout.PropertyField(dashedLineLong);
            EditorGUILayout.PropertyField(lineAccuracy);

            serializedObject.ApplyModifiedProperties();
        }

        public RoadGeneratorEditor()
        {
            RoadGenerator.GenerateMesh = GenerateMesh;
        }

        public static void GenerateMesh(RoadGenerator road)
        {
            if (helper == null)
            {
                GameObject helperGO = GameObject.FindGameObjectWithTag("Helper");
                if (helperGO == null)
                {
                    helperGO = new GameObject("Helper");
                    helperGO.tag = "Helper";
                }
                helper = helperGO.transform;
            }

            float distance = 0;
            BezierCurveMB curveMB = road.GetComponent<BezierCurveMB>();
            BezierCurve curve = new BezierCurve(curveMB.WordlP0, curveMB.WordlP1, curveMB.WordlP2, curveMB.WordlP3);

            #region ValidateValues

            if (road.CurveAccuracy <= 0)
                road.CurveAccuracy = 0.1f;

            if (road.LineAccuracy <= 0)
                road.LineAccuracy = 0.1f;

            if (road.LeftLanes < 0)
                road.LeftLanes = 0;

            if (road.RightLanes < 0)
                road.RightLanes = 0;

            if (road.RightLanes == 0 || road.LeftLanes == 0)
                road.GreenLane = false;

            if (road.GreenLaneWidth.mode != ParticleSystemCurveMode.TwoCurves)
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
                road.GreenLaneWidth = new MinMaxCurve(1, leftCurve, rightCurve);
            }

            if (road.LeftLaneExtensionCurve == null || road.LeftLaneExtensionCurve.keys.Length == 0 || road.LeftLaneExtensionCurve.keys[^1].value != LaneWidth)
            {
                road.LeftLaneExtensionCurve = new AnimationCurve();
                road.LeftLaneExtensionCurve.AddKey(0, 0);
                road.LeftLaneExtensionCurve.AddKey(curve.Distance, LaneWidth);
            }

            if (road.LeftLaneNarrowingCurve == null || road.LeftLaneNarrowingCurve.keys.Length == 0 || road.LeftLaneNarrowingCurve.keys[0].value != LaneWidth)
            {
                road.LeftLaneNarrowingCurve = new AnimationCurve();
                road.LeftLaneNarrowingCurve.AddKey(0, LaneWidth);
                road.LeftLaneNarrowingCurve.AddKey(curve.Distance, 0);
            }

            if (road.RightLaneExtensionCurve == null || road.RightLaneExtensionCurve.keys.Length == 0 || road.RightLaneExtensionCurve.keys[^1].value != LaneWidth)
            {
                road.RightLaneExtensionCurve = new AnimationCurve();
                road.RightLaneExtensionCurve.AddKey(0, 0);
                road.RightLaneExtensionCurve.AddKey(curve.Distance, LaneWidth);
            }

            if (road.RightLaneNarrowingCurve == null || road.RightLaneNarrowingCurve.keys.Length == 0 || road.RightLaneNarrowingCurve.keys[0].value != LaneWidth)
            {
                road.RightLaneNarrowingCurve = new AnimationCurve();
                road.RightLaneNarrowingCurve.AddKey(0, LaneWidth);
                road.RightLaneNarrowingCurve.AddKey(curve.Distance, 0);
            }

            if (road.LineTypes.Length != road.LineTypesCount())
                SetDefaultLineTypes(road);

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
                if (road.GreenLane)
                {
                    Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                    helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);

                    helper.position = curvePos;
                    helper.position += -helper.right * road.LeftLanes * LaneWidth + -helper.right * road.GetLeftRoadSideWidth;
                    helper.position += helper.right * road.GetLeftGreenLaneWidth(distance);
                    helper.position += -helper.right * road.GetLeftExtenstionWidth(curve.Distance - distance);

                    vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                    leftOutsideEdgeVertices.AddLast(vertices);
                    helper.position = curvePos;
                    helper.position += helper.right * road.GetLeftGreenLaneWidth(distance);

                    vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                    leftInsideEdgeVertices.AddLast(vertices);
                    helper.position = curvePos;
                    helper.position += helper.right * road.GetRightGreenLaneWidth(distance);

                    vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                    rightInsideEdgeVertices.AddLast(vertices);

                    helper.position = curvePos;
                    helper.position += helper.right * road.RightLanes * LaneWidth + helper.right * road.GetRightRoadSideWidth;
                    helper.position += helper.right * road.GetRightGreenLaneWidth(distance);
                    helper.position += helper.right * road.GetRightExtenstionWidth(distance);

                    vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                    rightOutsideEdgeVertices.AddLast(vertices);

                    helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                    upDirectionForVertex.Add(helper.up);
                }
                else
                {
                    Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                    helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);

                    helper.position = curvePos;
                    helper.position += -helper.right * road.LeftLanes * LaneWidth + -helper.right * road.GetLeftRoadSideWidth;
                    helper.position += -helper.right * road.GetLeftExtenstionWidth(curve.Distance - distance);

                    vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                    leftOutsideEdgeVertices.AddLast(vertices);
                    helper.position = curvePos;
                    helper.position += helper.right * road.RightLanes * LaneWidth + helper.right * road.GetRightRoadSideWidth;
                    helper.position += helper.right * road.GetRightExtenstionWidth(distance);

                    vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                    rightOutsideEdgeVertices.AddLast(vertices);

                    helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                    upDirectionForVertex.Add(helper.up);
                }

                if (distance == curve.Distance)
                    break;
                distance += road.CurveAccuracy;
                if (distance > curve.Distance)
                    distance = curve.Distance;
            }

            if (road.GreenLane)
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
            for (int i = 0; i < road.LineTypes.Length; i++)
            {
                if (road.LineTypes[i] == LineType.None)
                    continue;

                distance = 0;
                float targetDistance = 0;
                RealDistanceConverter realDistanceConverter = new RealDistanceConverter(curve, -road.LeftLanes * LaneWidth + LaneWidth + i * LaneWidth);
                if (road.LineTypes[i] == LineType.Dashed)
                {
                    targetDistance = road.DashedLineLong * 0.5f;
                    distance = realDistanceConverter.FindRealDistance(targetDistance);
                }

                while (distance <= curve.Distance)
                {
                    if (road.LineTypes[i] == LineType.Dashed)
                    {
                        float endOfDashRealDistance = targetDistance + road.DashedLineLong;
                        Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                        helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);
                        helper.position = curvePos;
                        helper.position += -helper.right * road.LeftLanes * LaneWidth;
                        helper.position += helper.right * (road.GetLeftRoadSideWidth == 0 ? i + 1 : i) * LaneWidth;
                        helper.position += helper.up * 0.03f;
                        helper.position += helper.right * LineHalfWidth;
                        if (road.IsDrawingLinesOnLeft(i))
                            helper.position += helper.right * road.GetLeftGreenLaneWidth(distance);
                        else
                            helper.position += helper.right * road.GetRightGreenLaneWidth(distance);

                        if (road.GetLeftRoadSideWidth > 0 && i == 0)
                            helper.position += -helper.right * road.GetLeftExtenstionWidth(curve.Distance - distance);

                        if (road.GetRightRoadSideWidth > 0 && i == road.LineTypes.Length - 1)
                            helper.position += helper.right * road.GetRightExtenstionWidth(distance);

                        while (distance < curve.Distance && targetDistance <= endOfDashRealDistance)
                        {
                            curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                            helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);
                            helper.position = curvePos;
                            helper.position += -helper.right * road.LeftLanes * LaneWidth;
                            helper.position += helper.right * (road.GetLeftRoadSideWidth == 0 ? i + 1 : i) * LaneWidth;
                            helper.position += helper.up * 0.03f;
                            if (road.IsDrawingLinesOnLeft(i))
                                helper.position += helper.right * road.GetLeftGreenLaneWidth(distance);
                            else
                                helper.position += helper.right * road.GetRightGreenLaneWidth(distance);

                            if (road.GetLeftRoadSideWidth > 0 && i == 0)
                                helper.position += -helper.right * road.GetLeftExtenstionWidth(curve.Distance - distance);

                            if (road.GetRightRoadSideWidth > 0 && i == road.LineTypes.Length - 1)
                                helper.position += helper.right * road.GetRightExtenstionWidth(distance);
                            helper.position += -helper.right * LineHalfWidth;

                            vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                            lines_leftVertices.AddLast(vertices);
                            helper.position += helper.right * LineHalfWidth * 2;

                            vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                            lines_rightVertices.AddLast(vertices);

                            helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                            lines_upDirectionForVertex.Add(helper.up);

                            if (targetDistance == endOfDashRealDistance)
                                break;
                            targetDistance += road.LineAccuracy;
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
                    else if (road.LineTypes[i] == LineType.Single)
                    {
                        Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                        helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);
                        helper.position = curvePos;
                        helper.position += -helper.right * road.LeftLanes * LaneWidth;
                        helper.position += helper.right * (road.GetLeftRoadSideWidth == 0 ? i + 1 : i) * LaneWidth;
                        helper.position += helper.up * 0.03f;
                        if (road.IsDrawingLinesOnLeft(i))
                            helper.position += helper.right * road.GetLeftGreenLaneWidth(distance);
                        else
                            helper.position += helper.right * road.GetRightGreenLaneWidth(distance);

                        if (road.GetLeftRoadSideWidth > 0 && i == 0)
                            helper.position += -helper.right * road.GetLeftExtenstionWidth(curve.Distance - distance);

                        if (road.GetRightRoadSideWidth > 0 && i == road.LineTypes.Length - 1)
                            helper.position += helper.right * road.GetRightExtenstionWidth(distance);
                        helper.position += -helper.right * LineHalfWidth;

                        vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                        lines_leftVertices.AddLast(vertices);
                        helper.position += helper.right * LineHalfWidth * 2;

                        vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                        lines_rightVertices.AddLast(vertices);

                        helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                        lines_upDirectionForVertex.Add(helper.up);
                    }
                    else if (road.LineTypes[i] == LineType.Double)
                    {
                        Vector3 curvePos = curve.GetPositionOfBezierCurveByDistance(distance);
                        helper.rotation = Quaternion.Euler(0, curve.GetRotationOfBezierCurveByDistance(distance), 0);
                        helper.position = curvePos;
                        helper.position += -helper.right * road.LeftLanes * LaneWidth - helper.right * DoubleLinesHalfSpace;
                        helper.position += helper.right * (road.GetLeftRoadSideWidth == 0 ? i + 1 : i) * LaneWidth;
                        helper.position += helper.up * 0.03f;
                        if (road.IsDrawingLinesOnLeft(i))
                            helper.position += helper.right * road.GetLeftGreenLaneWidth(distance);
                        else
                            helper.position += helper.right * road.GetRightGreenLaneWidth(distance);

                        if (road.GetLeftRoadSideWidth > 0 && i == 0)
                            helper.position += -helper.right * road.GetLeftExtenstionWidth(curve.Distance - distance);

                        if (road.GetRightRoadSideWidth > 0 && i == road.LineTypes.Length - 1)
                            helper.position += helper.right * road.GetRightExtenstionWidth(distance);
                        helper.position += -helper.right * LineHalfWidth;

                        vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                        lines_leftVertices.AddLast(vertices);
                        helper.position += helper.right * LineHalfWidth * 2;

                        vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                        lines_rightVertices.AddLast(vertices);

                        helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                        lines_upDirectionForVertex.Add(helper.up);
                        helper.position += helper.right * DoubleLinesHalfSpace;

                        vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                        lines_leftVertices.AddLast(vertices);
                        helper.position += helper.right * LineHalfWidth * 2;

                        vertices.NewVertex(road.transform.InverseTransformPoint(helper.position));
                        lines_rightVertices.AddLast(vertices);

                        helper.forward = curve.GetDerivativeOfBezierCurveByDistance(distance);
                        lines_upDirectionForVertex.Add(helper.up);
                    }


                    if (distance >= curve.Distance)
                        break;
                    targetDistance += road.LineTypes[i] == LineType.Dashed ? road.DashedLineLong : road.LineAccuracy;
                    distance = realDistanceConverter.FindRealDistance(targetDistance);
                    if (distance > curve.Distance)
                        distance = curve.Distance;
                }

                if (road.LineTypes[i] == LineType.Single)
                {
                    for (int j = lineVerticesToTriangleIndex; j < lines_leftVertices.Count - 1; j++)
                    {
                        lines_triangles.Add(new Triangle(new Vertex[] { lines_leftVertices[j], lines_leftVertices[j + 1], lines_rightVertices[j] }, lines_upDirectionForVertex[j]));
                        lines_triangles.Add(new Triangle(new Vertex[] { lines_leftVertices[j + 1], lines_rightVertices[j + 1], lines_rightVertices[j] }, lines_upDirectionForVertex[j]));
                    }
                    lineVerticesToTriangleIndex = lines_leftVertices.Count;
                }
                else if (road.LineTypes[i] == LineType.Double)
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

            MeshRenderer renderer = road.GetComponent<MeshRenderer>();
            Material[] materials = new Material[2] { RoadSystemSettings.Instance.AsphaltMat, RoadSystemSettings.Instance.RoadLineMat };
            renderer.sharedMaterials = materials;

            road.GetComponent<MeshFilter>().mesh = mesh;
        }

        public static void SetDefaultLineTypes(RoadGenerator road)
        {
            road.LineTypes = new LineType[road.LineTypesCount()];
            for (int i = 0; i < road.LineTypes.Length; i++)
                road.LineTypes[i] = LineType.Dashed;

            if (road.RightLanes > 0 && road.LeftLanes > 0)
            {
                if (road.GreenLane)
                    road.LineTypes[road.LeftLanes - 1 + (road.GetLeftRoadSideWidth > 0 ? 1 : 0)] = LineType.None;
                else
                    road.LineTypes[road.LeftLanes - 1 + (road.GetLeftRoadSideWidth > 0 ? 1 : 0)] = LineType.Single;
            }

            if (road.GetLeftRoadSideWidth > 0)
                road.LineTypes[0] = LineType.Single;

            if (road.GetRightRoadSideWidth > 0)
                road.LineTypes[^1] = LineType.Single;

            EditorUtility.SetDirty(road);
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

                    if (cathedRealDistance >= target)
                        return distance;

                    distance += 0.05f;
                }

                return curve.Distance;
            }
        }
    }
}