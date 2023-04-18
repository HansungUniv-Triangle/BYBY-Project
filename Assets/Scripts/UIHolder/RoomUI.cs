using TMPro;
using UnityEngine.UI;

namespace UIHolder
{
    public class RoomUI : UIHolder
    {
        public TMP_Text text1;
        public TMP_Text text2;
        public RawImage player1Ready;
        public RawImage player2Ready;
        public Button readyButton;
        
        protected override void Initial()
        {
            readyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.OnReady();
            });
        }
    }
}
