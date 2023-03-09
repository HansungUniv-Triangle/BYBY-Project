using System;
using System.Collections.Generic;
using Observer;
using UnityEngine;
using Util;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private Synergy[] _synergyList;
    public int SynergyCount => _synergyList.Length;
    
    protected override void Initiate()
    {
        _synergyList = Resources.LoadAll<Synergy>(Path.Synergy);
    }

    public Synergy GetSynergy(int index)
    {
        if (index >= SynergyCount)
        {
            throw new ArgumentOutOfRangeException(Message.OutOfSynergyLength);
        }

        return _synergyList[index];
    }
}
