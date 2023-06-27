using UnityEditor;
using UnityEngine;

namespace DRS
{
    public class ObjectsCreator : MonoBehaviour
    {
        [MenuItem("GameObject/Road System/Road")]
        public static void CreateRoad()
        {
            GameObject spawned = new GameObject("Road");
            spawned.AddComponent<RoadGenerator>();         
            EditorUtility.SetDirty(spawned);
        }

        [MenuItem("GameObject/Road System/Crossroad")]
        public static void CreateCrossroad()
        {
            GameObject spawned = new GameObject("Crossroad");
            spawned.AddComponent<CrossroadGenerator>();
            EditorUtility.SetDirty(spawned);
        }
    }
}