using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DRS
{
    [CustomEditor(typeof(BezierCurveMB))]
    public class BezierCurveEditor : Editor
    {
        private static int lastSelectedControlId = -1;
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private Vector3 lastScale = Vector3.one;

        public void OnSceneGUI()
        {
            BezierCurveMB curve = target as BezierCurveMB;

            if (curve.transform.position != lastPosition || curve.transform.rotation != lastRotation || curve.transform.localScale != lastScale)
            {
                curve.SubmitChange();
                lastPosition = curve.transform.position;
                lastRotation = curve.transform.rotation;
                lastScale = curve.transform.localScale;
            }

            Vector3 rp0 = curve.WordlP0;
            Vector3 rp3 = curve.WordlP3;

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0 || EditorWindow.mouseOverWindow != SceneView.currentDrawingSceneView)
                lastSelectedControlId = -1;

            Handles.color = Color.green;
            Handles.SphereHandleCap(0, rp0, Quaternion.identity, HandleUtility.GetHandleSize(rp0) * 0.2f, EventType.Repaint);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(!(lastSelectedControlId == -1 || lastSelectedControlId == 0) || curve.startLockedTo != null);
            Vector3 p0 = Handles.PositionHandle(rp0, Quaternion.identity);
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(curve, "Change P0");
                Vector3 offset = curve.P0 - curve.transform.InverseTransformPoint(p0);
                curve.P0 = curve.transform.InverseTransformPoint(p0);
                curve.P1 -= offset;
                curve.SubmitChange();
                lastSelectedControlId = 0;
            }
            
            Handles.Label(rp0 + Vector3.up * HandleUtility.GetHandleSize(rp0), "Start", EditorStyles.boldLabel);

            Vector3 rp1 = curve.WordlP1;
            Handles.color = Color.white;
            Handles.SphereHandleCap(0, rp1, Quaternion.identity, HandleUtility.GetHandleSize(rp1) * 0.2f, EventType.Repaint);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(!(lastSelectedControlId == -1 || lastSelectedControlId == 1));
            Vector3 p1 = Handles.PositionHandle(rp1, Quaternion.LookRotation(rp0 - rp1, Vector3.up));
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(curve, "Change P1");
                curve.P1 = curve.transform.InverseTransformPoint(p1);
                if (curve.startLockedTo != null)
                {
                    ISnapToTarget target = curve.startLockedTo.GetComponent<ISnapToTarget>();
                    float tangentDistance = Vector3.Distance(curve.WordlP0, curve.WordlP1);
                    curve.P1 = curve.transform.InverseTransformPoint(target.EndPoint - target.EndTangent.normalized * tangentDistance);
                }
                curve.SubmitChange();
                lastSelectedControlId = 1;
            }            

            Handles.color = Color.green;
            Handles.SphereHandleCap(0, rp3, Quaternion.identity, HandleUtility.GetHandleSize(rp3) * 0.2f, EventType.Repaint);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(!(lastSelectedControlId == -1 || lastSelectedControlId == 3) || curve.endLockedTo != null);
            Vector3 p3 = Handles.PositionHandle(rp3, Quaternion.identity);
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(curve, "Change P3");
                Vector3 offset = curve.P3 - curve.transform.InverseTransformPoint(p3);
                curve.P3 = curve.transform.InverseTransformPoint(p3);
                curve.P2 -= offset;
                curve.SubmitChange();
                lastSelectedControlId = 3;
            }
            
            Handles.Label(rp3 + Vector3.up * HandleUtility.GetHandleSize(rp3), "End", EditorStyles.boldLabel);

            Vector3 rp2 = curve.WordlP2;
            Handles.color = Color.white;
            Handles.SphereHandleCap(0, rp2, Quaternion.identity, HandleUtility.GetHandleSize(rp2) * 0.2f, EventType.Repaint);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(!(lastSelectedControlId == -1 || lastSelectedControlId == 2));
            Vector3 p2 = Handles.PositionHandle(rp2, Quaternion.LookRotation(rp3 - rp2, Vector3.up));
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(curve, "Change P2");
                curve.P2 = curve.transform.InverseTransformPoint(p2);
                if (curve.endLockedTo != null)
                {
                    ISnapToTarget target = curve.endLockedTo.GetComponent<ISnapToTarget>();
                    float tangentDistance = Vector3.Distance(curve.WordlP2, curve.WordlP3);
                    curve.P2 = curve.transform.InverseTransformPoint(target.StartPoint - target.StartTangent.normalized * tangentDistance);
                }
                curve.SubmitChange();
                lastSelectedControlId = 2;
            }            

            Handles.DrawBezier(rp0, rp3, rp1, rp2, Color.green, null, 8f);
            Handles.color = Color.white;
            Handles.DrawDottedLine(rp0, rp1, 2f);
            Handles.DrawDottedLine(rp2, rp3, 2f);                   
        }
    }
}