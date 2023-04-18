using UnityEngine;

namespace UIHolder
{
    public abstract class UIHolder : MonoBehaviour
    {
        private void Start()
        {
            GameManager.Instance.SetUICanvasHolder(this);
            Initial();
        }

        protected abstract void Initial();
    }
}