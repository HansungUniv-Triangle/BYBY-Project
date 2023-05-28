using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HitDamage : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private Sequence sequence;
    private bool isAnimating;
    private float _damage;
    private float _initFontSize;
    private Vector2 _initPos;

    public int damageThreshold;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();

        textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.35f);
        textMesh.outlineColor = new Color32(255, 0, 0, 225);
        textMesh.outlineWidth = 0.35f;

        isAnimating = false;
        _damage = 0;
        _initFontSize = textMesh.fontSize;
        _initPos = textMesh.rectTransform.anchoredPosition;
    }

    public void HitDamageAnimation(float damage, bool isCritical)
    {
        textMesh.color = Color.white;
        if (isCritical)
        {
            textMesh.color = Color.black;
            damage = 100;
            //damage *= 2;
        }

        _damage += damage;

        textMesh.fontSize = _initFontSize;
        if (damage > damageThreshold)
            textMesh.fontSize *= damage / damageThreshold;

        textMesh.text = _damage.ToString();
        textMesh.rectTransform.anchoredPosition = _initPos;

        if (isAnimating)
        {
            sequence.Restart();            
        }
        else
        {
            sequence = DOTween.Sequence()
                .Append(textMesh.rectTransform.DOShakeAnchorPos(1.0f, 20, 50))
                .Insert(0.7f, textMesh.DOFade(0f, 0.35f))
                .Insert(0.8f, textMesh.rectTransform.DOAnchorPosY(_initPos.y + 50, 0.25f))
                .OnStart(() =>
                {
                    textMesh.alpha = 1f;
                    isAnimating = true;
                })
                .OnComplete(() =>
                {
                    _damage = 0;
                    isAnimating = false;
                });
        }
    }
}
