﻿using System;

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
        Rolling, // 구르기
        Armor, // 방어력
        View, // 시야
    }

    public enum WeaponStat
    { // 발사주기, 크기, 재장전, 탄장, 정확도, 데미지,
        Interval, // 연사력 주기적인 발사
        Special, // 무기별 특화
        Damage, // 기본 데미지
        Range, // 사거리
        Reload, // 재장전 속도
        Bullet, // 탄창수
        BulletSize, // 총알 크기
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
}

