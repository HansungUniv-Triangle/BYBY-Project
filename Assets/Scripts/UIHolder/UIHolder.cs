using System;
using UnityEngine;

namespace UIHolder
{
    public abstract class UIHolder : MonoBehaviour
    {
        private void Awake()
        {
            GameManager.Instance.SetUICanvasHolder(this);
        }

        private void Start()
        {
            Initial();
        }

        private void OnDestroy()
        {
            GameManager.Instance.ClearUICanvasHolder();
        }

        protected abstract void Initial();
    }
}