using System;
using System.Collections.Generic;
using System.Linq;
using GameStatus;
using Network;
using RDG;
using Types;
using UnityEngine;
using Utils;
using NetworkPlayer = Network.NetworkPlayer;

public class GameManager : Singleton<GameManager>
{
    private GameObject _uiLoadingPrefab;
    private GameObject _uiLoading;
    private GameObject _uiDisconnectPrefab;
    private GameObject _uiDisconnect;

    [SerializeField]
    private UIHolder.UIHolder _uiHolder;
    public UIHolder.UIHolder UIHolder
    {
        get
        {
            if (_uiHolder is null)
            {
                _uiHolder = FindObjectOfType<UIHolder.UIHolder>();
            }
            return _uiHolder;
        }
    }

    public NetworkManager NetworkManager { get; private set; }
    public SynergyPageManager SynergyPageManager { get; private set; }
    public List<Synergy> SynergyList;
    public List<Weapon> WeaponList;
    public List<Material> CatMaterialList;
    
    public int selectWeaponNum = 0;
    public Weapon SelectWeapon => WeaponList[selectWeaponNum];

    public PlayerBehaviorAnalyzer PlayerBehaviorAnalyzer;

    private Dictionary<BehaviourEvent, int> behaviourEventCount;
    public int shootCount;
    public int hitCount;

    public bool IsVibrateOn = true;
    public bool IsGyroOn = true;

    public void ToggleVibrate()
    {
        IsVibrateOn = !IsVibrateOn;
        if (IsVibrateOn == false)
            Vibration.Cancel();
    }

    public void ToggleGyro()
    {
        IsGyroOn = !IsGyroOn;
    }

    protected override void Initiate()
    {
        SynergyList = Resources.LoadAll<Synergy>(Path.Synergy).ToList();
        WeaponList = Resources.LoadAll<Weapon>(Path.Weapon).ToList();
        CatMaterialList = Resources.LoadAll<Material>(Path.Cat).ToList();
        _uiLoadingPrefab = Resources.Load(Path.Loading) as GameObject;
        _uiDisconnectPrefab = Resources.Load(Path.Disconnect) as GameObject;
    }

    private void Start()
    {
        PlayerBehaviorAnalyzer = new PlayerBehaviorAnalyzer();
    }

    public void ResetBehaviourEventCount()
    {
        shootCount = 0;
        hitCount = 0;
        behaviourEventCount = new Dictionary<BehaviourEvent, int>();
        foreach (BehaviourEvent value in Enum.GetValues(typeof(BehaviourEvent)))
        {
            behaviourEventCount[value] = 0;
        }
    }

    public void AddBehaviourEventCount(BehaviourEvent key, int value)
    {
        behaviourEventCount[key] += value;
    }

    public Dictionary<BehaviourEvent, int> GetBehaviourEventCount()
    {
        behaviourEventCount[BehaviourEvent.회피] -= behaviourEventCount[BehaviourEvent.피격];
        if (behaviourEventCount[BehaviourEvent.회피] < 0)
        {
            behaviourEventCount[BehaviourEvent.회피] = 0;
        }
        behaviourEventCount[BehaviourEvent.명중] = (int)(hitCount / (float)shootCount * 100f);
        behaviourEventCount[BehaviourEvent.특화] = 50;
        return behaviourEventCount;
    }
    
    public void CheckBulletBetweenEnemyAndMe(Vector3 bulletPosition)
    {
        var player = NetworkPlayer.PlayerCharacter;
        var enemy = NetworkPlayer.EnemyCharacter;

        if (player == null || enemy == null)
        {
            return;
        }

        if (!(bulletPosition.x > Mathf.Min(enemy.transform.position.x, player.transform.position.x)) || !(bulletPosition.x < Mathf.Max(enemy.transform.position.x, player.transform.position.x)))
        {
            return;
        }
        
        if (bulletPosition.y > Mathf.Min(enemy.transform.position.y, player.transform.position.y) && bulletPosition.y < Mathf.Max(enemy.transform.position.y, player.transform.position.y))
        {
            behaviourEventCount[BehaviourEvent.파괴] += 1;
        }
    }

    public void SetNetworkManager(NetworkManager networkManager)
    {
        NetworkManager = networkManager;
    }

    public bool GetSynergy(int index, out Synergy synergy)
    {
        if (index > SynergyList.Count)
        {
            synergy = null;
            return false;
        }

        synergy = SynergyList[index];
        return true;
    }

    public void SetUICanvasHolder(UIHolder.UIHolder uiHolder)
    {
        _uiHolder = uiHolder;
    }
    
    public void ClearUICanvasHolder()
    {
        _uiHolder = null;
    }
    
    public void SetSynergyPageManager(SynergyPageManager synergyPageManager)
    {
        SynergyPageManager = synergyPageManager;
    }

    private void AddLoadingUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas)
        { 
            _uiLoading = Instantiate(_uiLoadingPrefab, canvas.transform);
            return;
        }

        throw new Exception("로딩 UI, 캔버스가 없음");
    }
    
    public void ActiveLoadingUI()
    {
        if (_uiLoading)
        {
            _uiLoading.SetActive(true);
        }
        else
        {
            AddLoadingUI();
            _uiLoading.SetActive(true);
        }
    }
    
    public void DeActiveLoadingUI()
    {
        if (_uiLoading)
        {
            _uiLoading.SetActive(false);
        }
        else
        {
            AddLoadingUI();
            _uiLoading.SetActive(false);
        }
    }
    
    private void AddDisconnectUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas)
        { 
            _uiDisconnect = Instantiate(_uiDisconnectPrefab, canvas.transform);
            return;
        }

        throw new Exception("로딩 UI, 캔버스가 없음");
    }
    
    public void ActiveDisconnectUI()
    {
        if (_uiDisconnect)
        {
            _uiDisconnect.SetActive(true);
        }
        else
        {
            AddDisconnectUI();
            _uiDisconnect.SetActive(true);
        }
    }
    
    public void DeActiveDisconnectUI()
    {
        if (_uiDisconnect)
        {
            _uiDisconnect.SetActive(false);
        }
        else
        {
            AddDisconnectUI();
            _uiDisconnect.SetActive(false);
        }
    }

    public void OnReady()
    {
        if (NetworkManager == null) return;
        NetworkManager.OnReady();
    }
}

public class StatCorrelationList<T> where T : Enum
{
    private Dictionary<T, List<StatCorrelation<CharStat>>> _charList = new();
    private Dictionary<T, List<StatCorrelation<WeaponStat>>> _weaponList = new();
    private T _typeSetting;

    public StatCorrelationList()
    {
        foreach (T stat in Enum.GetValues(typeof(T)))
        {
            _charList[stat] = new List<StatCorrelation<CharStat>>();
        }
        foreach (T stat in Enum.GetValues(typeof(T)))
        {
            _weaponList[stat] = new List<StatCorrelation<WeaponStat>>();
        }
    }

    public StatCorrelationList<T> SetCorrelationType(T stat)
    {
        _typeSetting = stat;
        return this;
    }

    public StatCorrelationList<T> AddCorrelationValue(CharStat add, float correlation)
    {
        if (_typeSetting is null)
        {
            throw new Exception("typeSetting이 되지 않았음");
        }
        _charList[_typeSetting].Add(new StatCorrelation<CharStat>(add, correlation));
        return this;
    }
    
    public StatCorrelationList<T> AddCorrelationValue(WeaponStat add, float correlation)
    {
        if (_typeSetting is null)
        {
            throw new Exception("typeSetting이 되지 않았음");
        }
        _weaponList[_typeSetting].Add(new StatCorrelation<WeaponStat>(add, correlation));
        return this;
    }
    
    public float GetCorrelationValue(T type, CharStat stat)
    {
        var result = _charList[type].Find(o => o.Stat.Equals(stat));
        if (result != null)
        {
            return result.Correlation;
        }

        return 0;
    }
    
    public float GetCorrelationValue(T type, WeaponStat stat)
    {
        var result = _weaponList[type].Find(o => o.Stat.Equals(stat));
        if (result != null)
        {
            return result.Correlation;
        }

        return 0;
    }
}

public class StatCorrelation<T> where T : Enum
{
    public T Stat;
    public float Correlation;

    public StatCorrelation(T stat, float correlation)
    {
        Stat = stat;
        Correlation = correlation;
    }

    public override string ToString()
    {
        return $"{nameof(Stat)}: {Stat}, {nameof(Correlation)}: {Correlation}";
    }
}

public class PlayerBehaviorAnalyzer
{
    public readonly StatCorrelationList<CharStat> CharStats;
    public readonly StatCorrelationList<WeaponStat> WeaponStats;
    
    public readonly Dictionary<BehaviourEvent, Enum[]> BehaviourEventStats = new() {
        { BehaviourEvent.피격, new Enum[] { CharStat.Health, CharStat.Armor} },
        { BehaviourEvent.회피, new Enum[] { CharStat.Speed, CharStat.Dodge} },
        { BehaviourEvent.명중, new Enum[] { CharStat.Calm, WeaponStat.Velocity} },
        { BehaviourEvent.피해, new Enum[] { WeaponStat.Attack, WeaponStat.Interval} },
        { BehaviourEvent.특화, new Enum[] { WeaponStat.Special} },
        { BehaviourEvent.파괴, new Enum[] { WeaponStat.Range} },
        { BehaviourEvent.장전, new Enum[] { WeaponStat.Bullet, WeaponStat.Reload } },
    };

    public Dictionary<Enum, float> EventResult;
    public Dictionary<Enum, int> MyStat;
    public Dictionary<Enum, float> StatResult;
    public Dictionary<Enum, float> Recommendation;

    public void ClearStatCorrelation()
    {
        EventResult = new()
        {
            { CharStat.Health, 0 },
            { CharStat.Speed, 0 },
            { CharStat.Dodge, 0 },
            { CharStat.Armor, 0 },
            { CharStat.Calm, 0 },
            { WeaponStat.Interval, 0 },
            { WeaponStat.Special, 0 },
            { WeaponStat.Attack, 0 },
            { WeaponStat.Range, 0 },
            { WeaponStat.Reload, 0 },
            { WeaponStat.Bullet, 0 },
            { WeaponStat.Velocity, 0 },
        };
        
        MyStat = new()
        {
            { CharStat.Health, 0 },
            { CharStat.Speed, 0 },
            { CharStat.Dodge, 0 },
            { CharStat.Armor, 0 },
            { CharStat.Calm, 0 },
            { WeaponStat.Interval, 0 },
            { WeaponStat.Special, 0 },
            { WeaponStat.Attack, 0 },
            { WeaponStat.Range, 0 },
            { WeaponStat.Reload, 0 },
            { WeaponStat.Bullet, 0 },
            { WeaponStat.Velocity, 0 },
        };
        
        StatResult = new()
        {
            { CharStat.Health, 0 },
            { CharStat.Speed, 0 },
            { CharStat.Dodge, 0 },
            { CharStat.Armor, 0 },
            { CharStat.Calm, 0 },
            { WeaponStat.Interval, 0 },
            { WeaponStat.Special, 0 },
            { WeaponStat.Attack, 0 },
            { WeaponStat.Range, 0 },
            { WeaponStat.Reload, 0 },
            { WeaponStat.Bullet, 0 },
            { WeaponStat.Velocity, 0 },
        };
        
        Recommendation = new()
        {
            { CharStat.Health, 0 },
            { CharStat.Speed, 0 },
            { CharStat.Dodge, 0 },
            { CharStat.Armor, 0 },
            { CharStat.Calm, 0 },
            { WeaponStat.Interval, 0 },
            { WeaponStat.Special, 0 },
            { WeaponStat.Attack, 0 },
            { WeaponStat.Range, 0 },
            { WeaponStat.Reload, 0 },
            { WeaponStat.Bullet, 0 },
            { WeaponStat.Velocity, 0 },
        };
    }

    public void AddCharStatCorrelation(BaseStat<CharStat> baseStat)
    {
        var keys = new List<Enum>(MyStat.Keys);
        foreach (CharStat charStatKey in Enum.GetValues(typeof(CharStat)))
        {
            MyStat[charStatKey] = (int)baseStat.GetStat(charStatKey).Total;
        }
    }
    
    public void AddWeaponStatCorrelation(BaseStat<WeaponStat> baseStat)
    {
        var keys = new List<Enum>(MyStat.Keys);
        foreach (WeaponStat weaponStatKey in Enum.GetValues(typeof(WeaponStat)))
        {
            MyStat[weaponStatKey] = (int)baseStat.GetStat(weaponStatKey).Total;
        }
    }

    public void AddBehaviourEventCount(BehaviourEvent behaviourEvent, float player, float enemy)
    {
        var ratio = player / (player + enemy);
        
        if (float.IsNaN(ratio) || float.IsInfinity(ratio))
        {
            ratio = 0.5f;
        }

        switch (behaviourEvent)
        {
            // 높은 수치일 수록 해당 스탯이 필요하게 설정 됨 -> 피격, 파괴, 장전은 높을수록 안좋음 -> 정상 작동
            case BehaviourEvent.피격:
            case BehaviourEvent.파괴:
            case BehaviourEvent.장전:
            case BehaviourEvent.특화:
                break;
            // 높은 수치일 수록 해당 스탯이 필요하게 설정 됨 -> 회피, 명중, 피해 높을수록 좋음 -> 1에서 빼서 역으로 만들어서 작동
            case BehaviourEvent.회피:
            case BehaviourEvent.명중:
            case BehaviourEvent.피해:
                ratio = 1 - ratio;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(behaviourEvent), behaviourEvent, null);
        }

        foreach (var key in BehaviourEventStats[behaviourEvent])
        {
            EventResult[key] += ratio;
        }
    }
    
    public void CalculateStatCorrelation()
    {
        var keys = new List<Enum>(MyStat.Keys);
        foreach (var key1 in keys)
        {
            foreach (var key2 in keys)
            {
                StatResult[key2] += MyStat[key1] * GetCorrelation(key1, key2);
            }
        }
        
        var minusToZeroAllSum = StatResult.Where(o=> o.Value > 0).Sum(o => o.Value);
        foreach (var key1 in keys)
        {
            var statValue = Math.Abs(StatResult[key1]) / minusToZeroAllSum;
            StatResult[key1] = StatResult[key1] > 0 ? statValue : -statValue;
        }
    }
    
    public void CalculateFinalCorrelation()
    {
        var keys = new List<Enum>(MyStat.Keys);
        foreach (var key in keys)
        {
            Recommendation[key] = (1 + EventResult[key]) * (1 + StatResult[key]);
        }
    }
    
    public float GetRecommendation(Enum statType)
    {
        return Recommendation[statType];
    }
    
    public PlayerBehaviorAnalyzer()
    {
        CharStats = new StatCorrelationList<CharStat>();
        WeaponStats = new StatCorrelationList<WeaponStat>();
        
        CharStats.SetCorrelationType(CharStat.Health)
            .AddCorrelationValue(CharStat.Armor, 0.5f);

        CharStats.SetCorrelationType(CharStat.Armor)
            .AddCorrelationValue(CharStat.Armor, -0.2f)
            .AddCorrelationValue(CharStat.Health, 0.5f);

        CharStats.SetCorrelationType(CharStat.Speed)
            .AddCorrelationValue(CharStat.Speed, -0.1f)
            .AddCorrelationValue(CharStat.Calm, 0.2f)
            .AddCorrelationValue(CharStat.Dodge, 0.1f)
            .AddCorrelationValue(WeaponStat.Range, 0.2f)
            .AddCorrelationValue(WeaponStat.Velocity, 0.2f);

        CharStats.SetCorrelationType(CharStat.Dodge)
            .AddCorrelationValue(CharStat.Dodge, -0.5f)
            .AddCorrelationValue(CharStat.Speed, 0.2f);
        
        CharStats.SetCorrelationType(CharStat.Calm)
            .AddCorrelationValue(CharStat.Calm, -0.4f)
            .AddCorrelationValue(CharStat.Speed, 0.2f);

        WeaponStats.SetCorrelationType(WeaponStat.Interval)
            .AddCorrelationValue(WeaponStat.Interval, -0.1f)
            .AddCorrelationValue(WeaponStat.Attack, 0.3f)
            .AddCorrelationValue(WeaponStat.Bullet, 0.3f)
            .AddCorrelationValue(WeaponStat.Reload, 0.3f);

        WeaponStats.SetCorrelationType(WeaponStat.Attack)
            .AddCorrelationValue(WeaponStat.Attack, 0.1f)
            .AddCorrelationValue(WeaponStat.Interval, 0.2f)
            .AddCorrelationValue(WeaponStat.Bullet, 0.2f)
            .AddCorrelationValue(WeaponStat.Reload, 0.2f)
            .AddCorrelationValue(WeaponStat.Special, 0.2f);

        WeaponStats.SetCorrelationType(WeaponStat.Range)
            .AddCorrelationValue(WeaponStat.Range, -0.2f)
            .AddCorrelationValue(WeaponStat.Velocity, 0.5f);

        WeaponStats.SetCorrelationType(WeaponStat.Velocity)
            .AddCorrelationValue(WeaponStat.Velocity, -0.2f)
            .AddCorrelationValue(WeaponStat.Range, 0.5f);

        WeaponStats.SetCorrelationType(WeaponStat.Reload)
            .AddCorrelationValue(WeaponStat.Bullet, 0.5f);

        WeaponStats.SetCorrelationType(WeaponStat.Bullet)
            .AddCorrelationValue(WeaponStat.Attack, 0.2f)
            .AddCorrelationValue(WeaponStat.Reload, 0.5f);

        WeaponStats.SetCorrelationType(WeaponStat.Special)
            .AddCorrelationValue(WeaponStat.Special, 0.5f);
    }

    private float GetCorrelation<T1, T2>(T1 stat1, T2 stat2)
        where T1 : Enum
        where T2 : Enum
    {
        if (stat1 is CharStat charStatA && stat2 is CharStat charStatB)
            return CharStats.GetCorrelationValue(charStatA, charStatB);

        if (stat1 is CharStat charStat && stat2 is WeaponStat weaponStat)
            return CharStats.GetCorrelationValue(charStat, weaponStat);

        if (stat1 is WeaponStat weaponStatA && stat2 is CharStat charStatC)
            return WeaponStats.GetCorrelationValue(weaponStatA, charStatC);

        if (stat1 is WeaponStat weaponStatB && stat2 is WeaponStat weaponStatC)
            return WeaponStats.GetCorrelationValue(weaponStatB, weaponStatC);

        return 0;
    }
}