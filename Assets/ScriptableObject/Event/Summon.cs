// using UnityEngine;
//
// public class Summon : SynergyEvent
// {
//     public SynergyEvent timing;
//     public GameObject summonObj;
//
//     public override void Activate()
//     {
//         GameManager.Instance.Attach(timing, this);
//     }
//
//     public override void Deactivate()
//     {
//         GameManager.Instance.Detach(timing, this);
//     }
//
//     public override void ReceiveNotify()
//     {
//         var pos = GameManager.Instance.player.transform;
//         Instantiate(summonObj, pos).SetActive(true);
//     }
// }