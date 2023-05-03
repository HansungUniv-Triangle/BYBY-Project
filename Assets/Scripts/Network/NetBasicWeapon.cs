using Fusion;

namespace Network
{
    public class NetBasicWeapon : NetworkProjectileHolder
    {
        public float timer = 5.0f;
        
        protected override void Attack()
        {
            if (CanAttack())
            {
                SpawnProjectile(WeaponTransform);
            }
        }
        
        protected override bool CanAttack()
        {
            if (!IsDoneShootAction)
            {
                return false;
            }

            if (delay.ExpiredOrNotRunning(Runner))
            {
                delay = TickTimer.CreateFromSeconds(Runner, timer);
                return true;
            }

            return false;
        }
    }
}