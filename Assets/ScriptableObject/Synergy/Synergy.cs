using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Status;

[ CreateAssetMenu(fileName = "Synergy", menuName = "SO/Synergy" )]
public class Synergy : ScriptableObject
{
    public Sprite sprite;
    public string synergyName;
    public int priority = 0;
    public List<RatioStat> statList = new List<RatioStat>();

    private void Awake()
    {
        var result = statList
            .GroupBy(x => x.statType)
            .Count(g => g.Count() > 1);
        if (result > 0)
        {
            Debug.Log($"{synergyName}에 중복된 stat이 존재합니다.");
        }
    }
}