using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyPage
{
    public bool isRerolled = false;
    public int rerollCount = 1;
    public string synergyRarity;
    public Synergy[] synergies = new Synergy[3];
    public GameObject synergyObj = null;
    public List<Synergy> IsNumInSynergyList = new List<Synergy>();
    public int pageNumber;
    public Synergy selectedSynergy;
    public int[] synergyRecommendationPercentage = new int[3];

    public bool AddSynergy(Synergy synergy)
    {
        if (synergies[0] == null)
        {
            synergies[0] = synergy;
            return true;
        }
        else if (synergies[1] == null)
        {
            synergies[1] = synergy;
            return true;
        }
        else if (synergies[2] == null)
        {
            synergies[2] = synergy;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Clear()
    {
        synergies[0] = null;
        synergies[1] = null;
        synergies[2] = null;
        IsNumInSynergyList.Clear();
        // 초기화 작업...
    }

    public void RerollCountClear()
    {
        if(isRerolled == false)
        {
            rerollCount = 1;
        }
    }

    public void FindSelectedSynergyInSynergies(string synergyName)
    {
        for (int i = 0; i < synergies.Length; i++)
        {
            if (synergyName == synergies[i].synergyName)
            {
                selectedSynergy = synergies[i];
                Debug.Log(selectedSynergy);
            }
        }
    }

    // 시너지 리스트에 중복된 숫자 있는지 확인
    public bool CheckIsNumInSynergyList(Synergy randomSynergy)
    {
        if (IsNumInSynergyList.Contains(randomSynergy))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
