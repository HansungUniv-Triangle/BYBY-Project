namespace Network
{
    public class NetHandGun : NetworkProjectileHolder
    {
        protected override void Attack()
        {
            if (CanAttack())
            {
                SpawnProjectile(WeaponTransform);
            }
        }
    }
}