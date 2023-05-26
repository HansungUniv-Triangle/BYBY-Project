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
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("자동소총"));
            });

            Berserk.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("광전사"));
            });

            Cannon.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("캐논"));
            });

            GuidedGun.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("유도탄"));
            });

            Handgun.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("핸드건"));
            });

            Healgun.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("구급상자"));
            });

            Shield.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("회전 회오리"));
            });

            Slow.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("거미줄"));
            });

            Sniper.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("스나이퍼"));
            });

            HugeOne.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("속도와질량"));
            });

            SummonSword.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = GameManager.Instance.WeaponList.FindIndex(x => x.weaponName.Equals("하늘에서 칼이"));
            });

            GameManager.Instance.NetworkManager.UpdateCanvasData();
        }
    }
}
