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

        public Button handgun;
        public Button autorifle;
        public Button cannon;
        public Button sniper;

        #endregion
        
        protected override void Initial()
        {
            readyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.OnReady();
            });
            
            handgun.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 0;
            });
            
            autorifle.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 1;
            });
            
            cannon.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 2;
            });
            
            sniper.onClick.AddListener(() =>
            {
                GameManager.Instance.selectWeaponNum = 3;
            });
        }
    }
}
