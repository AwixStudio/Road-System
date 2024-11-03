using UnityEngine;

public interface ISnapToTarget
{
    public Vector3 StartPoint { get; }
    public Vector3 EndPoint { get; }

    public Vector3 StartTangent { get; }
    public Vector3 EndTangent { get; }
}
