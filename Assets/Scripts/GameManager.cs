using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Network;
using Types;
using UnityEngine;
using Utils;
using NetworkPlayer = Network.NetworkPlayer;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    private GameObject _uiLoadingPrefab;
    private GameObject _uiLoading;

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
    
    public List<Synergy> SynergyList { get; private set; }

    public NetworkPrefabRef[] mainWeaponArray = new NetworkPrefabRef[4];
    public int selectWeaponNum = 0;
    public NetworkPrefabRef SelectWeapon => mainWeaponArray[selectWeaponNum];
    
    public List<NetworkPrefabRef> subWeaponList;
    public int selectSubWeaponNum;
    public NetworkPrefabRef SelectSubWeapon => subWeaponList[selectSubWeaponNum];

    public PlayerBehaviorAnalyzer PlayerBehaviorAnalyzer;

    private Dictionary<BehaviourEvent, int> behaviourEventCount;
    public int shootCount;
    public int hitCount;

    protected override void Initiate()
    {
        SynergyList = Resources.LoadAll<Synergy>(Path.Synergy).ToList();
        _uiLoadingPrefab = Resources.Load(Path.Loading) as GameObject;
    }

    private void Start()
    {
        PlayerBehaviorAnalyzer = new PlayerBehaviorAnalyzer();
        ResetBehaviourEventCount();
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

    public void PrintDebug()
    {
        foreach (var (key, value) in _charList)
        {
            foreach (var statCorrelation in value)
            {
                Debug.Log($"{key} : {statCorrelation}");
            }
        }
        
        foreach (var (key, value) in _weaponList)
        {
            foreach (var statCorrelation in value)
            {
                Debug.Log($"{key} : {statCorrelation}");
            }
        }
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
        { BehaviourEvent.회피, new Enum[] { CharStat.Speed, CharStat.Rolling} },
        { BehaviourEvent.명중, new Enum[] { CharStat.Calm, WeaponStat.Velocity} },
        { BehaviourEvent.피해, new Enum[] { WeaponStat.Damage, WeaponStat.Interval} },
        { BehaviourEvent.특화, new Enum[] { WeaponStat.Special} },
        { BehaviourEvent.파괴, new Enum[] { WeaponStat.Range} },
        { BehaviourEvent.장전, new Enum[] { WeaponStat.Bullet, WeaponStat.Reload } },
    };
    
    public Dictionary<BehaviourEvent, int> BehaviourEventCountPlayer = new() {
        { BehaviourEvent.피격, 0 },
        { BehaviourEvent.회피, 0 },
        { BehaviourEvent.명중, 0 },
        { BehaviourEvent.피해, 0 },
        { BehaviourEvent.특화, 0 },
        { BehaviourEvent.파괴, 0 },
        { BehaviourEvent.장전, 0 },
    };

    public Dictionary<BehaviourEvent, int> BehaviourEventCountEnemy = new() {
        { BehaviourEvent.피격, 0 },
        { BehaviourEvent.회피, 0 },
        { BehaviourEvent.명중, 0 },
        { BehaviourEvent.피해, 0 },
        { BehaviourEvent.특화, 0 },
        { BehaviourEvent.파괴, 0 },
        { BehaviourEvent.장전, 0 },
    };
    
    public Dictionary<Enum, float> EventResult = new()
    {
        { CharStat.Health, 0 },
        { CharStat.Speed, 0 },
        { CharStat.Rolling, 0 },
        { CharStat.Armor, 0 },
        { CharStat.Calm, 0 },
        { WeaponStat.Interval, 0 },
        { WeaponStat.Special, 0 },
        { WeaponStat.Damage, 0 },
        { WeaponStat.Range, 0 },
        { WeaponStat.Reload, 0 },
        { WeaponStat.Bullet, 0 },
        { WeaponStat.Velocity, 0 },
    };
    
    public Dictionary<Enum, int> MyStat = new()
    {
        { CharStat.Health, 0 },
        { CharStat.Speed, 0 },
        { CharStat.Rolling, 0 },
        { CharStat.Armor, 0 },
        { CharStat.Calm, 0 },
        { WeaponStat.Interval, 0 },
        { WeaponStat.Special, 0 },
        { WeaponStat.Damage, 0 },
        { WeaponStat.Range, 0 },
        { WeaponStat.Reload, 0 },
        { WeaponStat.Bullet, 0 },
        { WeaponStat.Velocity, 0 },
    };
    
    public Dictionary<Enum, float> StatResult = new()
    {
        { CharStat.Health, 0 },
        { CharStat.Speed, 0 },
        { CharStat.Rolling, 0 },
        { CharStat.Armor, 0 },
        { CharStat.Calm, 0 },
        { WeaponStat.Interval, 0 },
        { WeaponStat.Special, 0 },
        { WeaponStat.Damage, 0 },
        { WeaponStat.Range, 0 },
        { WeaponStat.Reload, 0 },
        { WeaponStat.Bullet, 0 },
        { WeaponStat.Velocity, 0 },
    };
    
    public Dictionary<Enum, float> RecommendFinal = new()
    {
        { CharStat.Health, 0 },
        { CharStat.Speed, 0 },
        { CharStat.Rolling, 0 },
        { CharStat.Armor, 0 },
        { CharStat.Calm, 0 },
        { WeaponStat.Interval, 0 },
        { WeaponStat.Special, 0 },
        { WeaponStat.Damage, 0 },
        { WeaponStat.Range, 0 },
        { WeaponStat.Reload, 0 },
        { WeaponStat.Bullet, 0 },
        { WeaponStat.Velocity, 0 },
    };

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
            .AddCorrelationValue(CharStat.Calm, -0.1f)
            .AddCorrelationValue(CharStat.Calm, 0.2f)
            .AddCorrelationValue(CharStat.Rolling, 0.1f)
            .AddCorrelationValue(WeaponStat.Range, 0.2f)
            .AddCorrelationValue(WeaponStat.Velocity, 0.2f);

        CharStats.SetCorrelationType(CharStat.Rolling)
            .AddCorrelationValue(CharStat.Rolling, -0.5f)
            .AddCorrelationValue(CharStat.Speed, 0.2f);
        
        CharStats.SetCorrelationType(CharStat.Calm)
            .AddCorrelationValue(CharStat.Rolling, -0.4f)
            .AddCorrelationValue(CharStat.Speed, 0.2f);

        WeaponStats.SetCorrelationType(WeaponStat.Interval)
            .AddCorrelationValue(WeaponStat.Interval, -0.1f)
            .AddCorrelationValue(WeaponStat.Damage, 0.3f)
            .AddCorrelationValue(WeaponStat.Bullet, 0.3f)
            .AddCorrelationValue(WeaponStat.Reload, 0.3f);

        WeaponStats.SetCorrelationType(WeaponStat.Damage)
            .AddCorrelationValue(WeaponStat.Damage, 0.1f)
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
            .AddCorrelationValue(WeaponStat.Bullet, 0.7f);

        WeaponStats.SetCorrelationType(WeaponStat.Bullet)
            .AddCorrelationValue(WeaponStat.Damage, 0.2f)
            .AddCorrelationValue(WeaponStat.Reload, 0.7f);

        WeaponStats.SetCorrelationType(WeaponStat.Special)
            .AddCorrelationValue(WeaponStat.Special, 0.5f);
        
        
        // 점유율을 랜덤으로 정합니다.
        foreach (BehaviourEvent behaviourEvent in Enum.GetValues(typeof(BehaviourEvent)))
        {
            BehaviourEventCountPlayer[behaviourEvent] = Random.Range(0, 100);
            BehaviourEventCountEnemy[behaviourEvent] = Random.Range(0, 100);
        }
        
        Debug.Log("스탯 정하기");
        
        // 일단 스탯을 랜덤으로 정합니다.
        var keys = new List<Enum>(MyStat.Keys);
        foreach (var key in keys)
        {
            MyStat[key] = Random.Range(0, 100);
            Debug.Log($"{key}: {MyStat[key]}");
        }
        
        Debug.Log("유사도 정하기");
        
        // 무작위로 정한 값 * 유사도를 구해서 결과에 저장합니다.
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
            Debug.Log($"{key1}: 값: {MyStat[key1]} 가중: {StatResult[key1]}, 비율: {statValue * 100}%");
            StatResult[key1] = StatResult[key1] > 0 ? statValue : -statValue;
        }
        
        // 점유율을 퍼센트로 나누어서 추천률로 변환합니다.
        foreach (BehaviourEvent behaviourEvent in Enum.GetValues(typeof(BehaviourEvent)))
        {
            var all = BehaviourEventCountPlayer[behaviourEvent] + BehaviourEventCountEnemy[behaviourEvent];
            // 낮을수록 해당 스탯을 높게 챙겨야하기 때문에 1에서 빼준다.
            var percent = 1 - (BehaviourEventCountPlayer[behaviourEvent] / (float)all);
            
            foreach (var key in BehaviourEventStats[behaviourEvent])
            {
                if (key is BehaviourEvent.특화)
                {
                    EventResult[key] += 0.5f;
                }
                else
                {
                    EventResult[key] += percent;
                }
                Debug.Log($"{key} {EventResult[key]}");
            }
        }

        // 서로 곱해서 결과를 봅시다.
        Debug.Log($"결과는?");
        foreach (var key in keys)
        {
            RecommendFinal[key] = (1 + EventResult[key]) * (1 + StatResult[key]);
            Debug.Log($"{key}: 가중({1 + StatResult[key]}) 이벤트({1 + EventResult[key]}) 최종({RecommendFinal[key]})");
        }

        var count = 1;
        var sortedDict = from entry in RecommendFinal orderby entry.Value descending select entry;
        foreach (var keyValuePair in sortedDict)
        {
            Debug.Log($"{count++}순위: {keyValuePair.Key}, {keyValuePair.Value}");
        }
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