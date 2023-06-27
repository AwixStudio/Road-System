using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DRS
{
    [CustomEditor(typeof(RoadGenerator))]
    public class RoadGeneratorEditor : Editor
    {
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
            leftLanes = serializedObject.FindProperty("leftLanes");
            rightLanes = serializedObject.FindProperty("rightLanes");
            curveAccuracy = serializedObject.FindProperty("curveAccuracy");
            leftRoadSide = serializedObject.FindProperty("leftRoadSide");
            leftRoadSideWidth = serializedObject.FindProperty("leftRoadSideWidth");
            rightRoadSide = serializedObject.FindProperty("rightRoadSide");
            rightRoadSideWidth = serializedObject.FindProperty("rightRoadSideWidth");

            leftLaneExtension = serializedObject.FindProperty("leftLaneExtension");
            leftLaneExtensionCurve = serializedObject.FindProperty("leftLaneExtensionCurve");
            leftLaneNarrowing = serializedObject.FindProperty("leftLaneNarrowing");
            leftLaneNarrowingCurve = serializedObject.FindProperty("leftLaneNarrowingCurve");
            rightLaneExtension = serializedObject.FindProperty("rightLaneExtension");
            rightLaneExtensionCurve = serializedObject.FindProperty("rightLaneExtensionCurve");
            rightLaneNarrowing = serializedObject.FindProperty("rightLaneNarrowing");
            rightLaneNarrowingCurve = serializedObject.FindProperty("rightLaneNarrowingCurve");

            greenLane = serializedObject.FindProperty("greenLane");
            greenLaneWidth = serializedObject.FindProperty("greenLaneWidth");

            lineTypes = serializedObject.FindProperty("lineTypes");
            dashedLineLong = serializedObject.FindProperty("dashedLineLong");
            lineAccuracy = serializedObject.FindProperty("lineAccuracy");
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
    }
}