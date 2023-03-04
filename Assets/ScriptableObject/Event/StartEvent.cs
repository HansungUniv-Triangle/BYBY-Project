// using Observer;
// using Type;
// using UnityEngine;
//
// [ CreateAssetMenu(fileName = "StartEvent", menuName = "SynergyEvent/StartEvent" )]
// public class StartEvent : SynergyEvent
// {
//     public bool CheckCondition()
//     {
//         var loadStat = GameManager.player.GetComponent<Move>().GetStat(subStat);
//         return loadStat >= rangeStart && loadStat < rangeEnd;
//     }
//
//     public override void Open()
//     {
//         if(subStat.Equals(Stat.None)) return;
//         GameManager.Attach(subStat, this);
//     }
//
//     public override void Close()
//     {
//         if(subStat.Equals(Stat.None)) return;
//         GameManager.Detach(subStat, this);
//     }
//
//     public override void ReceiveNotify()
//     {
//         if (CheckCondition())
//         {
//             
//         }
//     }
// }