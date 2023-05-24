using UnityEngine;
using Fusion;

namespace Network
{
    public class EnemyTest : NetworkBehaviour
    {
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Debug.Log("asdasd");
            
            if (hit.gameObject.TryGetComponent(out ICollisionObjectEvent collisionObject))
            {
                collisionObject.CollisionObjectEvent(Object);
            }
            
            if (hit.gameObject.layer == LayerMask.NameToLayer("World") && hit.normal.y == 0)
            {
                return;
            }
        }
    }
}