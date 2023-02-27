namespace Type
{
    public enum PoolObject
    {
        Block,
        DropItem,
        PlayerBullet,
        EnemyBullet
    }

    public enum Layer
    {
        Entity = 1 << 7,
        Block = 1 << 8,
    }

    public enum Character
    {
        Player,
        Enemy,
        Neutral,
    }

    public enum Stat
    {
        None,
        Speed,
        FireRate,
        ShotAtOnce,
        Special,
        Reload,
        Bullet,
        Range,
        ShieldBreak,
        Damage,
        BulletSize,
        Velocity,
    }
}

