using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DRS
{
    [InitializeOnLoad]
    public class RoadSystemSettings : ScriptableObject
    {
        public static RoadSystemSettings Instance { get; private set; }

        [field: SerializeField] public Material AsphaltMat { get; private set; }
        [field: SerializeField] public Material RoadLineMat { get; private set; }

        public RoadSystemSettings()
        {
            Instance = this;
        }
    }
}