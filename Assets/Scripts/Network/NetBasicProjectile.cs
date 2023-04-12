namespace Network
{
    public class NetBasicProjectile : NetworkProjectileBase
    {
        protected override bool IsExpirationProjectile()
        {
            return Distance > MaxRange;
        }

        protected override void UpdateProjectile()
        {
            transform.position += gameObject.transform.forward * (TotalVelocity * Runner.DeltaTime);
        }
    }
}