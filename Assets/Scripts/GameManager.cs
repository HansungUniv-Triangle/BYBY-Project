using System;
using Fusion;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Utils;

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

    [SerializeField]
    private Synergy[] _synergyList;
    public int SynergyCount => _synergyList.Length;

    protected override void Initiate()
    {
        _synergyList = Resources.LoadAll<Synergy>(Path.Synergy);
        _uiLoadingPrefab = Resources.Load(Path.Loading) as GameObject;
    }

    public void SetNetworkManager(NetworkManager networkManager)
    {
        NetworkManager = networkManager;
    }

    public bool GetSynergy(int index, out Synergy synergy)
    {
        if (index >= SynergyCount)
        {
            synergy = null;
            
            return false;
        }
        synergy = _synergyList[index];
        return true;
    }

    public void SetUICanvasHolder(UIHolder.UIHolder uiHolder)
    {
        _uiHolder = uiHolder;
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

    // adapter
    public void DisconnectedSever()
    {
        GameObject.Find("Spawner").GetComponent<Network.BasicSpawner>().DisconnectingServer();
    }

    public void OnReady()
    {
        if (NetworkManager == null) return;
        NetworkManager.OnReady();
    }
}