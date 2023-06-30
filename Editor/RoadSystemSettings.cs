using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DRS
{
    public class RoadSystemSettings : ScriptableObject
    {
        private const string settingsPath = "Assets/RoadSystem/RoadSystemSettings.asset";
        private const string asphaltMaterialPath = "Assets/RoadSystem/Materials/Asphalt/M_Asphalt.mat";
        private const string roadLineMaterialPath = "Assets/RoadSystem/Materials/Asphalt/M_Line.mat";

        private static RoadSystemSettings instance;

        [SerializeField] private Material asphaltMat;
        [SerializeField] private Material roadLineMat;

        public Material AsphaltMaterial => asphaltMat;
        public Material RoadLineMaterial => roadLineMat;

        internal static RoadSystemSettings GetOrCreateSettings()
        {
            if(instance == null)
                instance = AssetDatabase.LoadAssetAtPath<RoadSystemSettings>(settingsPath);
            
            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<RoadSystemSettings>();
                instance.asphaltMat = AssetDatabase.LoadAssetAtPath<Material>(asphaltMaterialPath);
                instance.roadLineMat = AssetDatabase.LoadAssetAtPath<Material>(roadLineMaterialPath);
                AssetDatabase.CreateAsset(instance, settingsPath);
                AssetDatabase.SaveAssets();
            }
            
            return instance;
        }

        internal static SerializedObject GetSerializedSettings() => new SerializedObject(GetOrCreateSettings());
    }

    static class RoadSystemSettingsIMGUIRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateRoadSystemSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Road System", SettingsScope.Project)
            {
                guiHandler = (searchContext) =>
                {
                    var settings = RoadSystemSettings.GetSerializedSettings();
                    EditorGUILayout.PropertyField(settings.FindProperty("asphaltMat"), new GUIContent(" Asphalt Material"));
                    EditorGUILayout.PropertyField(settings.FindProperty("roadLineMat"), new GUIContent(" Road Line Material"));
                    settings.ApplyModifiedPropertiesWithoutUndo();
                },

                keywords = new HashSet<string>(new[] { "Asphalt", "Road", "Material" })
            };

            return provider;
        }
    }
}