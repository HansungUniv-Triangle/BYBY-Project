namespace Weapon
{
    public class HandGun : Weapon
    {
        private ProjectileHolder<HandGunBullet> _projectileHolder;

        protected override void Initialize()
        {
            _projectileHolder = new ProjectileHolder<HandGunBullet>();
        }

        public override void Attack()
        {
            throw new System.NotImplementedException();
        }
    }
}