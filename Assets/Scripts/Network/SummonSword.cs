using Fusion;
using UnityEngine;

namespace Network
{
    public class SummonSword : NetworkProjectileHolder
    {
        protected override void Attack()
        {
            if (CanAttack())
            {
                SpawnProjectile(Target + new Vector3(0, 50, 0), basicRotate: true);
            }
        }
        
        protected override bool CanAttack()
        {
            if (delay.ExpiredOrNotRunning(Runner))
            {
                delay = TickTimer.CreateFromSeconds(Runner, 5f);
                return true;
            }

            return false;
        }
    }
}