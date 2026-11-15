using UnityEngine;

namespace NekoLongWayHome
{
    public interface IKinematicMover
    {
        Vector3 GetPointVelocity(Vector3 worldPoint);
    }
}
