using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

namespace UIHolder
{
    public class RoomUI : UIHolder
    {
        public TMP_Text text1;
        public TMP_Text text2;
        public Image player1Ready;
        public Image player2Ready;
        public Button readyButton;
        public Image tabFocused;
        public TMP_Text weaponName;
        public TMP_Text weaponExplain;

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
        
        protected override void Initial()
        {
            readyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.OnReady();
            });

            Autorifle.onClick.AddListener(() =>
            {
                GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
                Vector3 buttonPosition = selectedButton.transform.position;
                Vector3 tabPosition = tabFocused.transform.position;
                tabPosition.x = buttonPosition.x;
                tabPosition.y = buttonPosition.y - selectedButton.GetComponent<RectTransform>().sizeDelta.y / 2;
                tabFocused.transform.position = tabPosition;

                GameManager.Instance.selectWeaponNum = 0;
                weaponName.text = GameManager.Instance.WeaponList[0].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[0].weaponExplain;
            });

            /*Berserk.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 1;
            });*/

            Cannon.onClick.AddListener(() =>
            {
                GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
                Vector3 buttonPosition = selectedButton.transform.position;
                Vector3 tabPosition = tabFocused.transform.position;
                tabPosition.x = buttonPosition.x;
                tabPosition.y = buttonPosition.y - selectedButton.GetComponent<RectTransform>().sizeDelta.y / 2;
                tabFocused.transform.position = tabPosition;

                GameManager.Instance.selectWeaponNum = 2;
                weaponName.text = GameManager.Instance.WeaponList[2].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[2].weaponExplain;
            });

            /*GuidedGun.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 3;
            });*/

            Handgun.onClick.AddListener(() =>
            {
                GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
                Vector3 buttonPosition = selectedButton.transform.position;
                Vector3 tabPosition = tabFocused.transform.position;
                tabPosition.x = buttonPosition.x;
                tabPosition.y = buttonPosition.y - selectedButton.GetComponent<RectTransform>().sizeDelta.y / 2;
                tabFocused.transform.position = tabPosition;

                GameManager.Instance.selectWeaponNum = 4;
                weaponName.text = GameManager.Instance.WeaponList[4].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[4].weaponExplain;
            });

            /*Healgun.onClick.AddListener(() =>
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
            });*/

            Sniper.onClick.AddListener(() =>
            {
                GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
                Vector3 buttonPosition = selectedButton.transform.position;
                Vector3 tabPosition = tabFocused.transform.position;
                tabPosition.x = buttonPosition.x;
                tabPosition.y = buttonPosition.y - selectedButton.GetComponent<RectTransform>().sizeDelta.y / 2;
                tabFocused.transform.position = tabPosition;

                GameManager.Instance.selectWeaponNum = 8;
                weaponName.text = GameManager.Instance.WeaponList[8].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[8].weaponExplain;
            });
            
            GameManager.Instance.NetworkManager.UpdateCanvasData();
        }
    }
}
