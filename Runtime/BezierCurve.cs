using System.Runtime.CompilerServices;
using UnityEngine;

namespace DRS
{
    public class BezierCurve
    {
        public Vector3 P0 { get; private set; }
        public Vector3 P1 { get; private set; }
        public Vector3 P2 { get; private set; }
        public Vector3 P3 { get; private set; }
        public float Distance { get; private set; }

        private CustomAnimationCurve distanceTable;

        public const float DistanceAccuracy = 0.1f;

        public BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
            CaluclateDistance();
        }

        public BezierCurve(BezierCurveMB bezierCurveMB)
        {
            P0 = bezierCurveMB.WordlP0;
            P1 = bezierCurveMB.WordlP1;
            P2 = bezierCurveMB.WordlP2;
            P3 = bezierCurveMB.WordlP3;
            CaluclateDistance();
        }
        
        private void CaluclateDistance()
        {
            Distance = 0;

            int sampleCount = Mathf.RoundToInt(1 / DistanceAccuracy);
            Keyframe[] keyframes = new Keyframe[sampleCount + 1];
            keyframes[0] = new Keyframe(0, 0);

            for (int i = 0; i < sampleCount; i++)
            {
                Vector3 point1 = GetPositionOfBezierCurve(i * DistanceAccuracy);
                Vector3 point2 = GetPositionOfBezierCurve((i + 1) * DistanceAccuracy);
                float distanceInThisStep = Vector3.Distance(point1, point2);
                Distance += distanceInThisStep;
                keyframes[i + 1] = new Keyframe(Distance, (i + 1) * DistanceAccuracy);
            }

            AnimationCurve unityDistanceTable = new AnimationCurve(keyframes);
            for (int i = 0; i < unityDistanceTable.keys.Length; i++)
                unityDistanceTable.SmoothTangents(i, 0);            
            distanceTable = new CustomAnimationCurve(unityDistanceTable, 25);
        }

        public Vector3 GetPositionOfBezierCurve(float t)
        {
            Vector3 helperPoints0 = Vector3.Lerp(P0, P1, t);
            Vector3 helperPoints1 = Vector3.Lerp(P1, P2, t);
            Vector3 helperPoints2 = Vector3.Lerp(P2, P3, t);
            Vector3 helperPoints3 = Vector3.Lerp(helperPoints0, helperPoints1, t);
            Vector3 helperPoints4 = Vector3.Lerp(helperPoints1, helperPoints2, t);
            Vector3 helperPoints5 = Vector3.Lerp(helperPoints3, helperPoints4, t);

            return helperPoints5;
        }
        
        public float GetRotationOfBezierCurve(float t)
        {
            Vector3 normalizedDerivative = Vector3.Normalize(GetDerivativeOfBezierCurve(t));
            return Mathf.Atan2(normalizedDerivative.x, normalizedDerivative.z) * (180 / Mathf.PI);
        }

        public Vector3 GetDerivativeOfBezierCurve(float t) => P0 * (-3 * t * t + 6 * t - 3) + P1 * (9 * t * t - 12 * t + 3) + P2 * (-9 * t * t + 6 * t) + P3 * (3 * t * t);

        public Vector3 GetNormalizedDerivativeOfBezierCurveByCurve(float distance) => Vector3.Normalize(GetDerivativeOfBezierCurve(distanceTable.Evaluate(distance / Distance)));
        public Vector3 GetPositionOfBezierCurveByDistance(float distance) => GetPositionOfBezierCurve(distanceTable.Evaluate(distance / Distance));
        public float GetRotationOfBezierCurveByDistance(float distance) => GetRotationOfBezierCurve(distanceTable.Evaluate(distance / Distance));
        public Vector3 GetDerivativeOfBezierCurveByDistance(float distance) => GetDerivativeOfBezierCurve(distanceTable.Evaluate(distance / Distance));

        [System.Serializable]
        public class CustomAnimationCurve
        {
            [SerializeField] private float[] values;
            [SerializeField] private WrapMode preWrapMode;
            [SerializeField] private WrapMode postWrapMode;

            public CustomAnimationCurve(AnimationCurve curve, int resolution)
            {
                preWrapMode = curve.preWrapMode;
                postWrapMode = curve.postWrapMode;

                if (values == null || values.Length != resolution)
                    values = new float[resolution];

                for (int i = 0; i < resolution; i++)
                    values[i] = curve.Evaluate((float)i / (float)(resolution - 1) * curve.keys[curve.length - 1].time);
            }

            public float Evaluate(float t)
            {
                var count = values.Length;

                if (count < 2)
                    return 0;

                if (t < 0f)
                {
                    switch (preWrapMode)
                    {
                        default:
                            return values[0];
                        case WrapMode.Loop:
                            t = 1f - (Mathf.Abs(t) % 1f);
                            break;
                        case WrapMode.PingPong:
                            t = Pingpong(t, 1f);
                            break;
                    }
                }
                else if (t > 1f)
                {
                    switch (postWrapMode)
                    {
                        default:
                            return values[count - 1];
                        case WrapMode.Loop:
                            t %= 1f;
                            break;
                        case WrapMode.PingPong:
                            t = Pingpong(t, 1f);
                            break;
                    }
                }

                var it = t * (count - 1);

                var lower = (int)it;
                var upper = lower + 1;
                if (upper >= count)
                    upper = count - 1;

                return Mathf.Lerp(values[lower], values[upper], it - lower);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float Repeat(float t, float length) => Mathf.Clamp(t - Mathf.Floor(t / length) * length, 0, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float Pingpong(float t, float length)
            {
                t = Repeat(t, length * 2f);
                return length - Mathf.Abs(t - length);
            }
        }
    }
}