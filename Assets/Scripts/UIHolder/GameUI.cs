﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIHolder
{
    public class GameUI : UIHolder
    {
        public RectTransform crossHair;
        public Joystick joystick;
        public Button resetPlayerLocation;
        public TextMeshProUGUI hitDamageText;
        public Button ultButton;

        protected override void Initial()
        {
            resetPlayerLocation.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.LocalCharacter.InitPosition();
            });
            
            ultButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.LocalCharacter.GetUlt();
            });
        }
    }
}

