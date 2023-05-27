using TMPro;
using UnityEngine;
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

        #region WeaponButton

        public Button Autorifle;
        public Button Berserk;
        public Button Cannon;
        public Button GuidedGun;
        public Button Handgun;
        public Button Healgun;
        public Button Shield;
        public Button Slow;
        public Button Sniper;

        #endregion
        
        private readonly Color32 _ready = Color.green;
        private readonly Color32 _notReady = Color.red;
        
        protected override void Initial()
        {
            readyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.OnReady();
            });

            Autorifle.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 0;
            });

            Berserk.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 1;
            });

            Cannon.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 2;
            });

            GuidedGun.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 3;
            });

            Handgun.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 4;
            });

            Healgun.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 5;
            });

            Shield.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 6;
            });

            Slow.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 7;
            });

            Sniper.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 8;
            });

            if (GameManager.Instance.NetworkManager is not null)
            {
                GameManager.Instance.NetworkManager.UpdateCanvasData();
            }
        }
        
        public void UpdateRoomItem(int index, string nick, bool ready)
        {
            switch (index)
            {
                case 0:
                    text1.text = nick;
                    player1Ready.color = ready ? _ready : _notReady;
                    break;
                case 1:
                    text2.text = nick;
                    player2Ready.color = ready ? _ready : _notReady;
                    break;
            }
        }

        public void ClearRoom()
        {
            text1.text = "-";
            text1.color = Color.black;
            player1Ready.color = _notReady;
            text2.text = "-";
            text2.color = Color.black;
            player2Ready.color = _notReady;
        }
    }
}
