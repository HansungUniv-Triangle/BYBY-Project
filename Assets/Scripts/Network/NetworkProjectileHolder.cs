using System;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using GameStatus;
using TMPro;
using Types;
using UIHolder;
using UnityEngine;

namespace Network
{
    public abstract class NetworkProjectileHolder : NetworkBehaviour
    {
        protected BaseStat<WeaponStat> _baseWeaponStat;
        private List<NetworkObject> _projectileList;
        [SerializeField]
        private NetworkObject _projectileObject;

        protected Transform WeaponTransform;
        public Transform ShootPointTransform { get; set; }
        protected Vector3 Target;
        protected bool IsDoneShootAction;
        protected int RemainBullet;
        protected TextMeshProUGUI BulletText;
        
        [Networked] protected TickTimer delay { get; set; }

        private void Awake()
        {
            _baseWeaponStat = new BaseStat<WeaponStat>(1, 1);
            _projectileList = new List<NetworkObject>();

            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Damage, 10, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Velocity, 20, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Range, 10, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Special, 6, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Bullet, 10, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Reload, 10, 0));
            
            WeaponTransform = gameObject.transform;
            
            var shootPoint = transform.Find("ShootPoint");
            if (shootPoint == null)
            {
                shootPoint = WeaponTransform;
            }
            ShootPointTransform = shootPoint;

            Target = gameObject.transform.forward;
            IsDoneShootAction = true;
        }

        private void Start()
        {
            if(!_projectileObject.TryGetComponent<NetworkProjectileBase>(out _))
            {
                Debug.LogError("Holder 연결 에러");
            }
            
            RemainBullet = (int)GetWeaponStat(WeaponStat.Bullet).Total;
            //BulletText = (GameManager.Instance.UIHolder as GameUI).bulletText;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasInputAuthority) return;
            
            switch (GameManager.Instance.NetworkManager.GameRoundState)
            {
                case RoundState.RoundStart:
                    break;
                case RoundState.None:
                case RoundState.GameStart:
                case RoundState.SynergySelect:
                case RoundState.WaitToStart:
                case RoundState.RoundEnd:
                case RoundState.GameEnd:
                default:
                    return;
            }
            
            Attack();

            //BulletText.text = RemainBullet.ToString();
        }

        public void SetTarget(Vector3 target)
        {
            Target = target;
        }
        
        public Vector3 GetTarget()
        {
            return Target;
        }
        
        private void InitializeProjectile(NetworkRunner runner, NetworkObject obj)
        {
            var objInit = obj.GetComponent<NetworkProjectileBase>();
            objInit.Initialized(this);
        }

        protected NetworkObject SpawnProjectile(Transform transform)
        {
            var position = transform.position;
            var obj = Runner.Spawn(
                _projectileObject, 
                position, //+ position.TransformDirection(Vector3.forward), 
                Quaternion.LookRotation(Target - position), 
                Runner.LocalPlayer,
                InitializeProjectile
            );
            _projectileList.Add(obj);
            return obj;
        }
        
        protected virtual bool CanAttack()
        {
            if (!IsDoneShootAction)
            {
                return false;
            }
            
            if (RemainBullet == 0)
            {
                ReloadBullet();
                return false;
            }
            
            if (delay.ExpiredOrNotRunning(Runner))
            {
                delay = TickTimer.CreateFromSeconds(Runner, _baseWeaponStat.GetStat(WeaponStat.Interval).Total);
                return true;
            }
            
            return false;
        }

        public void ChangeIsDone(bool value)
        {
            IsDoneShootAction = value;
        }

        protected void ReloadBullet()
        {
            Sequence reloadSequence = DOTween.Sequence();

            reloadSequence
                .OnStart(() =>
                {
                    IsDoneShootAction = false;
                    GameManager.Instance.ActiveLoadingUI();
                })
                .OnComplete(() =>
                {
                    IsDoneShootAction = true;
                    GameManager.Instance.DeActiveLoadingUI();
                });

            var max = GetWeaponStat(WeaponStat.Bullet).Total;
            var time = GetWeaponStat(WeaponStat.Reload).Total;
            var separateTime = (50 + time) / (50 * max);
            
            for (int i = 0; i < max; i++)
            {
                reloadSequence
                    .AppendCallback(() => RemainBullet++)
                    .AppendInterval(separateTime);
            }

            reloadSequence.Play();
        }

        protected abstract void Attack();

        #region 스탯

        protected void AddWeaponAdditionStat(WeaponStat weaponStat, float add)
        {
            GetWeaponStat(weaponStat).AddAddition(add);
        }
        
        protected void AddCharAdditionStat(CharStat charStat, float add)
        {
            GetCharStat(charStat).AddAddition(add);
        }
        
        protected Stat<CharStat> GetCharStat(CharStat stat)
        {
            var localCharacter = GameManager.Instance.NetworkManager.LocalCharacter;
            return localCharacter.GetCharStat(stat);
        }
        
        public void AddWeaponStat(Stat<WeaponStat> stat)
        {
            _baseWeaponStat.AddStat(stat);
        }
        
        public void AddWeaponStatList(List<Stat<WeaponStat>> statList)
        {
            _baseWeaponStat.AddStatList(statList);
        }
        
        public Stat<WeaponStat> GetWeaponStat(WeaponStat stat)
        {
            return _baseWeaponStat.GetStat(stat);
        }
        
        public float GetWeaponStatTotal(WeaponStat stat)
        {
            return _baseWeaponStat.GetStat(stat).Total;
        }

        public void ClearWeaponStat()
        {
            _baseWeaponStat.ClearStatList();
        }

        #endregion
    }
}