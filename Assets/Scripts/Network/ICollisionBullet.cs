using Fusion;

namespace Network
{
    public interface ICollisionBullet
    {
        public void CollisionBullet(NetworkObject bullet);
        public bool CollisionBulletIsHitCheck();
    }
}