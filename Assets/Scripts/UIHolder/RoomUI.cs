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
        public Button HugeOne;
        public Button SummonSword;

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

                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("자동소총"));
                weaponName.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponExplain;
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

                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("캐논"));
                weaponName.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponExplain;

                //GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("광전사"));
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

                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("핸드건"));
                weaponName.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponExplain;
                //GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("유도탄"));
            });

            /*Healgun.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("구급상자"));
            });

            Shield.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("회전 회오리"));
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

                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("스나이퍼"));
                weaponName.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponName;
                weaponExplain.text = GameManager.Instance.WeaponList[GameManager.Instance.selectWeaponNum].weaponExplain;
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("거미줄"));
            });

            /*HugeOne.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("속도와질량"));
            });

            SummonSword.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("하늘에서 칼이"));
            });*/

            GameManager.Instance.NetworkManager.UpdateCanvasData();
        }
    }
}
