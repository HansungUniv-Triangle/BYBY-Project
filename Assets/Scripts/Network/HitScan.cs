using DG.Tweening;
using Fusion;
using Types;
using UnityEngine;

public class HitScan : NetworkBehaviour
{
    [Networked] private Vector3 startVector { get; set; } = Vector3.zero;
    [Networked] private Vector3 endVector { get; set; } = Vector3.zero;
    private bool isRendered = false;

    public override void Spawned()
    {
        SoundManager.Instance.Play("sniper_shoot", Sound.Effect);
    }

    public override void FixedUpdateNetwork()
    {
        if (isRendered || startVector == Vector3.zero || startVector == Vector3.zero)
        {
            return;
        }

        RenderingLine();
    }

    private void RenderingLine()
    {
        isRendered = true;
        
        var line = GetComponent<LineRenderer>();
        line.SetPosition(0, startVector);
        line.SetPosition(1, endVector);

        line.DOColor(
            new Color2(new Color(140, 0, 0, 1), new Color(255, 0, 0, 0.4f)),
            new Color2(Color.clear, Color.clear),
            0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                if (HasStateAuthority)
                {
                    Runner.Despawn(Object);
                }
            });
    }

    public void SetPosition(Vector3 start, Vector3 end)
    {
        startVector = start;
        endVector = end;
    }
}
