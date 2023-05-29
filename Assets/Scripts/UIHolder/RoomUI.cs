using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UIHolder
{
    public class RoomUI : UIHolder
    {
        public TMP_Text text1;
        public TMP_Text text2;
        public GameObject player1Ready;
        public GameObject player2Ready;
        public Button readyButton;
        public Button exitButton;
        public Image tabFocused;
        public TMP_Text weaponName;
        public TMP_Text weaponExplain;
        public TMP_Text roomNumber;

        #region WeaponButton

        public Button Autorifle;
        public Button Cannon;
        public Button Handgun;
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
            
            exitButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.DisconnectingServer();
            });

            Autorifle.onClick.AddListener(() =>
            {
                GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
                Vector3 buttonPosition = selectedButton.transform.position;
                Vector3 tabPosition = tabFocused.transform.position;
                tabPosition.x = buttonPosition.x;
                tabPosition.y = buttonPosition.y - selectedButton.GetComponent<RectTransform>().sizeDelta.y / 2;
                tabFocused.transform.position = tabPosition;

                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("자동소총"));
                weaponName.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponExplain;
            });

            Cannon.onClick.AddListener(() =>
            {
                GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
                Vector3 buttonPosition = selectedButton.transform.position;
                Vector3 tabPosition = tabFocused.transform.position;
                tabPosition.x = buttonPosition.x;
                tabPosition.y = buttonPosition.y - selectedButton.GetComponent<RectTransform>().sizeDelta.y / 2;
                tabFocused.transform.position = tabPosition;

                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("대포"));
                weaponName.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponExplain;
            });

            Handgun.onClick.AddListener(() =>
            {
                GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
                Vector3 buttonPosition = selectedButton.transform.position;
                Vector3 tabPosition = tabFocused.transform.position;
                tabPosition.x = buttonPosition.x;
                tabPosition.y = buttonPosition.y - selectedButton.GetComponent<RectTransform>().sizeDelta.y / 2;
                tabFocused.transform.position = tabPosition;

                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("권총"));
                weaponName.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponExplain;
            });

            Sniper.onClick.AddListener(() =>
            {
                GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
                Vector3 buttonPosition = selectedButton.transform.position;
                Vector3 tabPosition = tabFocused.transform.position;
                tabPosition.x = buttonPosition.x;
                tabPosition.y = buttonPosition.y - selectedButton.GetComponent<RectTransform>().sizeDelta.y / 2;
                tabFocused.transform.position = tabPosition;

                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("저격총"));
                weaponName.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponExplain;
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
                    player1Ready.SetActive(ready);
                    break;
                case 1:
                    text2.text = nick;
                    player2Ready.SetActive(ready);
                    break;
            }
        }

        public void ClearRoom()
        {
            text1.text = "-";
            player1Ready.SetActive(false);
            text2.text = "-";
            player2Ready.SetActive(false);
        }
    }
}
