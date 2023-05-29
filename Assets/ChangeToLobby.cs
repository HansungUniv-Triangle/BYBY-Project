using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeToLobby : MonoBehaviour
{
    public void OnLobbyButton()
    {
        SceneManager.LoadSceneAsync("LobbyScene");
        GameManager.Instance.DeActiveDisconnectUI();
    }
}
