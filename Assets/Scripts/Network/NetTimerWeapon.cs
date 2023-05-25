using Fusion;

namespace Network
{
    public class NetTimerWeapon : NetworkProjectileHolder
    {
        public float timer = 10.0f;
        
        protected override void Attack()
        {
            if (CanAttack())
            {
                SpawnProjectile(WeaponTransform.position);
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