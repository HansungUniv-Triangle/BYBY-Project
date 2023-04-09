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
        World = 1 << 6,
        Entity = 1 << 7,
        Block = 1 << 8,
    }

    public enum Character
    {
        Player,
        Enemy,
        Neutral,
    }

    public enum CharStat
    {
        Health, // 체력
        Speed, // 속도
        Shield, // 쉴드 비율
        Vampire, // 피흡
        Armor, // 방어력
        Ultimate, // 필살기 횟수
        DamageRatio, // 최종 데미지 비율
        IntervalRatio, // 연사 비율
    }

    public enum WeaponStat
    {
        Interval, // 연사력 주기적인 발사
        ShotAtOnce, // 한번에 발사
        Reload, // 재장전 속도
        Bullet, // 탄창수
        Range, // 사거리
        ShieldBreak, // 쉴드에 입히는 데미지
        Damage, // 기본 데미지
        BulletSize, // 총알 크기
        Velocity, // 총알 속도
        Special, // 무기별 특화
        Pierce, // 관통수
        Guided, // 유도력
        MaxLevel, // 최대 성장 레벨
    }

    public enum CanvasType 
    { 
        None,
        GameMoving,
        GameAiming
    }

    public enum AttackType
    {
        Basic,
        Ultimate
    }

    public enum JoystickSettingType
    {
        Variable,
        Floating
    }
}

