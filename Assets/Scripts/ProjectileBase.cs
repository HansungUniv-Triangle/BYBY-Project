using System;
using Type;
using UnityEngine;
using GameStatus;

public abstract class ProjectileBase<T> : MonoBehaviour where T : ProjectileBase<T>
{
    // 클래스별 
    private static ProjectileHolder<T> _projectileHolder;
    
    // 방향 관련
    private Vector3 _direction;
    
    // 기본 스탯
    private Stat<WeaponStat> _range;
    private Stat<WeaponStat> _velocity;
    private Stat<WeaponStat> _bulletSize;
    private Stat<WeaponStat> _damage;
    
    // 변동 스탯
    private float _distance;
    private float _addVelocity;
    private float _addScale;
    private float _addDamage;
    
    // 기본 + 변동 스탯
    public float TotalVelocity => _velocity.Total + _addVelocity;
    public float TotalBulletSize => _bulletSize.Total + _addScale;
    public float TotalDamage => _damage.Total + _addDamage;

    private void Clear()
    {
        _range = new Stat<WeaponStat>(WeaponStat.Range, 0);
        _velocity = new Stat<WeaponStat>(WeaponStat.Velocity, 0);
        _bulletSize = new Stat<WeaponStat>(WeaponStat.BulletSize, 0);
        _damage = new Stat<WeaponStat>(WeaponStat.Damage, 0);
        
        _distance = 0;
        _addVelocity = 0;
        _addScale = 0;
        _addDamage = 0;
        
        _direction = Vector3.forward;
    }

    // 방향 지정 메소드
    public virtual ProjectileBase<T> SetDirection(Vector3 dir)
    {
        _direction = dir;
        return this;
    }
    
    // 기본 스탯 지정
    protected virtual ProjectileBase<T> SetBaseStat(Stat<WeaponStat> stat)
    {
        switch (stat.Type)
        {
            case WeaponStat.Range:
                _range.AddStat(stat);
                break;
            case WeaponStat.Damage:
                _damage.AddStat(stat);
                break;
            case WeaponStat.Velocity:
                _velocity.AddStat(stat);
                break;
            case WeaponStat.BulletSize:
                _bulletSize.AddStat(stat);
                break;
            case WeaponStat.Interval:
            case WeaponStat.ShotAtOnce:
            case WeaponStat.Reload:
            case WeaponStat.Bullet:
            case WeaponStat.ShieldBreak:
            case WeaponStat.Special:
            case WeaponStat.Pierce:
            case WeaponStat.Guided:
            case WeaponStat.MaxLevel:
            default:
                throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
        }
        return this;
    }
    
    // FixedUpdate로 프레임 대비 일관적으로 이동시킨다
    private void FixedUpdate()
    {
        _distance += Time.deltaTime * TotalVelocity;
        gameObject.transform.Translate(_direction * TotalVelocity);
    }
}
