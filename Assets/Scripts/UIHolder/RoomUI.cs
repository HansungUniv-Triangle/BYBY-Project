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

        #endregion
        
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
        }
    }
}
