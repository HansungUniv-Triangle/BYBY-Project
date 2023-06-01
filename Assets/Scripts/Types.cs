using System;

namespace Types
{
    public enum PoolObject
    {
        Block,
        DropItem,
        PlayerBullet,
        EnemyBullet
    }

    [Flags]
    public enum Layer
    {
        World = 1 << 6,
        Player = 1 << 7,
        Enemy = 1 << 8,
        Bullet = 1 << 9,
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
        Dodge, // 회피
        Armor, // 방어
        Calm // 차분
    }

    public enum WeaponStat
    { // 발사주기, 크기, 재장전, 탄장, 정확도, 데미지,
        Interval, // 연사력 주기적인 발사
        Special, // 무기별 특화
        Attack, // 기본 데미지
        Range, // 사거리
        Reload, // 재장전 속도
        Bullet, // 탄창수
        Velocity, // 총알 속도
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
    
    public enum RoundState
    {
        None,
        GameStart,
        SynergySelect, 
        WaitToStart,
        RoundStart,
        RoundEnd,
        RoundResult,
        RoundAnalysis,
        GameEnd,
    }
    
    public enum Rarity
    {
        Common,
        UnCommon,
        Rare
    }

    public enum BehaviourEvent
    {
        피격,
        회피,
        명중,
        피해,
        특화,
        파괴,
        장전
    }

    public enum CameraMode
    {
        None,
        Game,
        Player,
        Winner,
    }

    public enum HitEffectType { 
        Everything,
        Player
    }

    public enum Sound {
        BGM,
        Effect,
        MaxCount    // enum 총 개수
    }
}

