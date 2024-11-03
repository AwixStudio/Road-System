using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DRS
{
    [ExecuteInEditMode]
    public class BezierCurveMB : MonoBehaviour, ISnapToTarget
    {
        public Vector3 P0 = new(1, 0, 0);
        public Vector3 P1 = new(2, 0, 0);
        public Vector3 P2 = new(4, 0, 0);
        public Vector3 P3 = new(5, 0, 0);

        public GameObject startLockedTo;
        public GameObject endLockedTo;

        public event Action<int> OnChange = delegate { };

        public Vector3 WordlP0 => transform.TransformPoint(P0);
        public Vector3 WordlP1 => transform.TransformPoint(P1);
        public Vector3 WordlP2 => transform.TransformPoint(P2);
        public Vector3 WordlP3 => transform.TransformPoint(P3);

        public Vector3 StartPoint => WordlP0;
        public Vector3 EndPoint => WordlP3;
        public Vector3 StartTangent => WordlP1 - WordlP0;
        public Vector3 EndTangent => WordlP2 - WordlP3;

        public void SubmitChange(int deep = 0) => OnChange.Invoke(deep);

        private void Start()
        {
            if (startLockedTo != null)
            {
                BezierCurveMB bezierCurveMB = startLockedTo.GetComponent<BezierCurveMB>();
                if (bezierCurveMB != null)
                    bezierCurveMB.OnChange += MoveStartToLocked;
            }

            if (endLockedTo != null)
            {
                BezierCurveMB bezierCurveMB = endLockedTo.GetComponent<BezierCurveMB>();
                if (bezierCurveMB != null)
                    bezierCurveMB.OnChange += MoveEndToLocked;
            }
        }

        [ContextMenu("Reverse")]
        public void Reverse()
        {
            if(startLockedTo != null || endLockedTo != null)
            {
                Debug.Log("You have to unlock knots before reverse operation.");
                return;
            }

            Vector3 p0 = P0;
            Vector3 p1 = P1;
            P0 = P3;
            P1 = P2;
            P2 = p1;
            P3 = p0;

            SubmitChange(0);
            SetDirty();
        }

        [ContextMenu("Snap/Snap")]
        public void SnapBoth()
        {
            Snap(true, false);
            Snap(false, false);
        }

        [ContextMenu("Snap/Snap Start")]
        public void SnapStart() => Snap(true, false);

        [ContextMenu("Snap/Snap End")]
        public void SnapEnd() => Snap(false, false);

        [ContextMenu("Snap/Snap and Lock")]
        public void SnapBothAndLock()
        {
            Snap(true, false);
            Snap(false, false);
        }

        [ContextMenu("Snap/Snap Start and Lock")]
        public void SnapStartAndLock() => Snap(true, true);

        [ContextMenu("Snap/Snap End and Lock")]
        public void SnapEndAndLock() => Snap(false, true);

        private void Snap(bool start, bool lockKnot)
        {
            BezierCurveMB[] bezierCurves = FindObjectsOfType<BezierCurveMB>();

            if (bezierCurves.Length < 2)
                return;

            if (start)
            {
                float minDistance = float.MaxValue;
                int minIndex = 0;

                for (int i = 0; i < bezierCurves.Length; i++)
                {
                    float distance = Vector3.Distance(StartPoint, bezierCurves[i].EndPoint);
                    if (distance < minDistance && bezierCurves[i] != this)
                    {
                        minDistance = distance;
                        minIndex = i;
                    }
                }

                float tangentDistance = Vector3.Distance(WordlP0, WordlP1);
                P0 = transform.InverseTransformPoint(bezierCurves[minIndex].EndPoint);
                P1 = transform.InverseTransformPoint(bezierCurves[minIndex].EndPoint - bezierCurves[minIndex].EndTangent.normalized * tangentDistance);

                if(lockKnot)
                {
                    startLockedTo = bezierCurves[minIndex].gameObject;
                    BezierCurveMB bezierCurveMB = startLockedTo.GetComponent<BezierCurveMB>();
                    if (bezierCurveMB != null)
                    {                        
                        bezierCurveMB.OnChange -= MoveStartToLocked;
                        bezierCurveMB.OnChange += MoveStartToLocked;
                    }
                }
            }
            else
            {
                float minDistance = float.MaxValue;
                int minIndex = 0;

                for (int i = 0; i < bezierCurves.Length; i++)
                {
                    float distance = Vector3.Distance(EndPoint, bezierCurves[i].StartPoint);
                    if (distance < minDistance && bezierCurves[i] != this)
                    {
                        minDistance = distance;
                        minIndex = i;
                    }
                }

                float tangentDistance = Vector3.Distance(WordlP2, WordlP3);
                P3 = transform.InverseTransformPoint(bezierCurves[minIndex].StartPoint);
                P2 = transform.InverseTransformPoint(bezierCurves[minIndex].StartPoint - bezierCurves[minIndex].StartTangent.normalized * tangentDistance);

                if (lockKnot)
                {
                    endLockedTo = bezierCurves[minIndex].gameObject;
                    BezierCurveMB bezierCurveMB = endLockedTo.GetComponent<BezierCurveMB>();
                    if (bezierCurveMB != null)
                    {                        
                        bezierCurveMB.OnChange -= MoveEndToLocked;
                        bezierCurveMB.OnChange += MoveEndToLocked;
                    }
                }
            }        
            
            SubmitChange();
            SetDirty();
        }

        [ContextMenu("Snap/Unlock")]
        public void UnlockBoth()
        {
            UnlockStart();
            UnlockEnd();
        }

        [ContextMenu("Snap/Unlock Start")]
        public void UnlockStart()
        {
            if (startLockedTo == null)
                return;

            BezierCurveMB bezierCurveMB = startLockedTo.GetComponent<BezierCurveMB>();
            if (bezierCurveMB != null)
                bezierCurveMB.OnChange -= MoveStartToLocked;
            startLockedTo = null;
        }

        [ContextMenu("Snap/Unlock End")]
        public void UnlockEnd()
        {
            if (endLockedTo == null)
                return;

            BezierCurveMB bezierCurveMB = endLockedTo.GetComponent<BezierCurveMB>();
            if (bezierCurveMB != null)
                bezierCurveMB.OnChange -= MoveEndToLocked;
            endLockedTo = null;
        }

        private void MoveStartToLocked(int deep)
        {
            if (deep > 0)
                return;

            ISnapToTarget target = startLockedTo.GetComponent<ISnapToTarget>();

            float tangentDistance = Vector3.Distance(WordlP0, WordlP1);
            P0 = transform.InverseTransformPoint(target.EndPoint);
            P1 = transform.InverseTransformPoint(target.EndPoint - target.EndTangent.normalized * tangentDistance);
            SubmitChange(++deep);
            SetDirty();
        }

        private void MoveEndToLocked(int deep)
        {
            if (deep > 0)
                return;

            ISnapToTarget target = endLockedTo.GetComponent<ISnapToTarget>();

            float tangentDistance = Vector3.Distance(WordlP2, WordlP3);
            P3 = transform.InverseTransformPoint(target.StartPoint);
            P2 = transform.InverseTransformPoint(target.StartPoint - target.StartTangent.normalized * tangentDistance);
            SubmitChange(++deep);
            SetDirty();
        }

        private void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}