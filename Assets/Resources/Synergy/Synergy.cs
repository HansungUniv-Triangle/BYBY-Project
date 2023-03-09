using System.Collections.Generic;
using System.Linq;
using GameStatus;
using UnityEngine;
using Type;

[ CreateAssetMenu(fileName = "Synergy", menuName = "SO/Synergy" )]
public class Synergy : ScriptableObject
{
    public Sprite sprite;
    public string synergyName;
    public string synergyExplain;
    public int priority = 0;
    public List<Stat<CharStat>> charStatList = new List<Stat<CharStat>>();
    public List<Stat<WeaponStat>> weaponStatList = new List<Stat<WeaponStat>>();
    
    private void Awake()
    {
        var checkChar = charStatList
            .GroupBy(x => x.Type)
            .Count(g => g.Count() > 1);
        
        var checkWeapon = weaponStatList
            .GroupBy(x => x.Type)
            .Count(g => g.Count() > 1);
        
        if ((checkChar > 0) || (checkWeapon > 0))
        {
            Debug.LogWarning($"{synergyName}에 중복된 stat이 존재합니다.");
        }
    }
}