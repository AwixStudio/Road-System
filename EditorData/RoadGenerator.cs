using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace DRS
{
    [RequireComponent(typeof(BezierCurveMB))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class RoadGenerator : MonoBehaviour
    {
        [Header("Road")]
        public int LeftLanes = 2;
        public int RightLanes = 2;
        
        public float CurveAccuracy = 1f;
        
        public bool LeftRoadSide;
        public float LeftRoadSideWidth = 1;
        public float GetLeftRoadSideWidth => LeftRoadSide ? LeftRoadSideWidth : 0;
        
        public bool RightRoadSide;
        public float RightRoadSideWidth = 1;
        public float GetRightRoadSideWidth => RightRoadSide ? RightRoadSideWidth : 0;
        
        [Header("Extensions")]
        public bool LeftLaneExtension;
        public ExtentionEditType LeftLaneExtensionEditType;        
        public AnimationCurve LeftLaneExtensionCurve;

        public bool LeftLaneNarrowing;
        public ExtentionEditType LeftLaneNarrowingEditType;
        public AnimationCurve LeftLaneNarrowingCurve;

        public bool RightLaneExtension;
        public ExtentionEditType RightLaneExtensionEditType;
        public AnimationCurve RightLaneExtensionCurve;

        public bool RightLaneNarrowing;
        public ExtentionEditType RightLaneNarrowingEditType;
        public AnimationCurve RightLaneNarrowingCurve;

        public float GetLeftExtenstionWidth(float distance)
        {
            if (LeftLaneExtension)
            {
                if (LeftLaneNarrowing)
                {
                    return Mathf.Max(LeftLaneExtensionCurve.Evaluate(distance), LeftLaneNarrowingCurve.Evaluate(distance));
                }
                else
                {
                    return LeftLaneExtensionCurve.Evaluate(distance);
                }
            }
            else
            {
                if(LeftLaneNarrowing)
                {
                    return LeftLaneNarrowingCurve.Evaluate(distance);
                }
                else
                {
                    return 0;
                }
            }             
        }

        public float GetRightExtenstionWidth(float distance)
        {
            if (RightLaneExtension)
            {
                if (RightLaneNarrowing)
                {
                    return Mathf.Max(RightLaneExtensionCurve.Evaluate(distance), RightLaneNarrowingCurve.Evaluate(distance));
                }
                else
                {
                    return RightLaneExtensionCurve.Evaluate(distance);
                }
            }
            else
            {
                if (RightLaneNarrowing)
                {
                    return RightLaneNarrowingCurve.Evaluate(distance);
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool GreenLane;
        public ExtentionEditType GreenLaneEditType;
        public float GreenLaneWidth;
        public MinMaxCurve GreenLaneWidthCurve;
        public float GetRightGreenLaneWidth(float distance) => GreenLane ? (GreenLaneEditType == ExtentionEditType.Custom ? GreenLaneWidthCurve.curveMax.Evaluate(distance) : GreenLaneWidth * 0.5f) : 0;
        public float GetLeftGreenLaneWidth(float distance) => GreenLane ? (GreenLaneEditType == ExtentionEditType.Custom ? GreenLaneWidthCurve.curveMin.Evaluate(distance) : -GreenLaneWidth * 0.5f) : 0;

        [Header("Lines")]
        public LineType[] LineTypes = new LineType[0];
        public float DashedLineLong = 4;
        public float LineAccuracy = 1f;

        private bool initialized;

        public static Action<RoadGenerator> GenerateMesh;

        private void Awake()
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                GetComponent<DRS.BezierCurveMB>().OnChange += (int deep) => GenerateMesh.Invoke(this);
                if (Selection.activeGameObject == gameObject)
                    GenerateMesh.Invoke(this);
#endif
                initialized = true;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => UnityEditor.EditorApplication.delayCall += _OnValidate;

        private void _OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= _OnValidate;
            if (this == null) return;
            if (initialized && !Application.isPlaying && Selection.activeGameObject == gameObject)
                GenerateMesh.Invoke(this);
        }
#endif

        [ContextMenu("Debug Vertices")]
        public void DebugVertices()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            for (int i = 0; i < meshFilter.sharedMesh.vertexCount; i++)
            {
                GameObject spawned = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spawned.transform.position = meshFilter.sharedMesh.vertices[i];
            }
        }

        [ContextMenu("Regenerate")]
        public void Regenerate() => GenerateMesh.Invoke(this);

        public int LineTypesCount()
        {
            if(LeftLanes == 0 && RightLanes == 0)
                return 0;

            return RightLanes + LeftLanes - 1 + ((GetLeftRoadSideWidth > 0) ? 1 : 0) + ((GetRightRoadSideWidth > 0) ? 1 : 0);
        }

        public bool IsDrawingLinesOnLeft(int i)
        {
            int midLineIndex = (LeftLanes - 1 + (GetLeftRoadSideWidth > 0 ? 1 : 0));
            return i < midLineIndex;
        }

        //private void DebugVertex(Vertex v)
        //{
        //    GameObject spawned = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    spawned.transform.position = v.pos;
        //}
    }
}