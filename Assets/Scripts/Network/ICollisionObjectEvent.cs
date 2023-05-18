using Fusion;

namespace Network
{
    public interface ICollisionObjectEvent
    { // 총알에 오브젝트 충돌이 일어났을 때
        public void CollisionObjectEvent(NetworkObject networkObject);
        public bool CollisionObjectIsHitCheck();
    }
}