using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class DBManager : Singleton<DBManager>
{
    private FirebaseFirestore _db;
    private string _uuid;
    private DocumentReference _userRef;
    private DocumentReference _winRef;
    public string NickName { private set; get; }

    protected override void Initiate()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _uuid = SystemInfo.deviceUniqueIdentifier;
        _userRef = _db.Collection("Users").Document(_uuid);
        _winRef = _db.Collection("Ranking").Document(_uuid);
    }

    private void Start()
    {
        GetUserNickname().ContinueWithOnMainThread(task =>
        {
            NickName = task.Result;
        });
    }

    public async Task ChangeNickname(string nick)
    {
        var data = GetNickDto(nick);
        GameManager.Instance.ActiveLoadingUI();
        await _userRef.SetAsync(data);
        NickName = nick;
        GameManager.Instance.DeActiveLoadingUI();
    }

    public async Task<string> GetUserNickname()
    {
        return await GetNickname(_userRef);
    }
    
    public async Task<(int, int)> GetUserWin()
    {
        DocumentSnapshot snapshot = await _winRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            var data = snapshot.ToDictionary();
            var win = Convert.ToInt32(data["win"]);
            var defeat = Convert.ToInt32(data["defeat"]);
            return (win, defeat);
        } 
        else
        {
            return (0, 0);
        }
    }

    private async Task<string> GetNickname(DocumentReference docRef)
    {
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (snapshot.Exists) {
            var data = snapshot.ToDictionary();
            var nick = data["nickname"].ToString();
            return nick;
        }
        else
        {
            return "null";
        }
    }
    
    public void UpdatePlayerWin()
    {
        Dictionary<string, object> dto;
        
        _winRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists) {
                var data = snapshot.ToDictionary();
                var win = Convert.ToInt32(data["win"]) + 1;
                var defeat = Convert.ToInt32(data["defeat"]);
                var winRating = win / (double)(win + defeat) * 100;
                var winStraight = Convert.ToInt32(data["winStraight"]);
                var isWinStraight = (bool)data["isWinStraight"];
                var maxWinStraight = Convert.ToInt32(data["maxWinStraight"]);

                if (isWinStraight)
                {
                    winStraight += 1;
                }
                else
                {
                    winStraight = 1;
                }

                if (winRating > maxWinStraight)
                {
                    maxWinStraight = winStraight;
                }
                
                dto = GetWinDto(win, defeat, winRating, winStraight, true, maxWinStraight);
            } 
            else
            {
                dto = GetWinDto(1, 0, 100, 1, true, 1);
            }
            AddWinData(dto);
        });
    }
    
    public void UpdatePlayerDefeat()
    {
        Dictionary<string, object> dto;
        
        _winRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists) {
                var data = snapshot.ToDictionary();
                var win = Convert.ToInt32(data["win"]);
                var defeat = Convert.ToInt32(data["defeat"]) + 1;
                var winRating = win / (double)(win + defeat) * 100;
                var maxWinStraight = Convert.ToInt32(data["maxWinStraight"]);

                dto = GetWinDto(win, defeat, winRating, 0, false, maxWinStraight);
            } 
            else
            {
                dto = GetWinDto(0, 1, 0, 0, false, 0);
            }
            AddWinData(dto);
        });
    }

    public async Task<List<(string, int)>> GetManyWinRanking()
    {
        Query rankingQuery = _db.Collection("Ranking").OrderByDescending("win").Limit(10);
        List<(string, int)> rankingList = new List<(string, int)>();

        QuerySnapshot capitalQuerySnapshot = await rankingQuery.GetSnapshotAsync();

        foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
        {
            var data = documentSnapshot.ToDictionary();
            var nickRef = data["ref"] as DocumentReference;
            var nick = await GetNickname(nickRef);
            var win = Convert.ToInt32(data["win"]);
            rankingList.Add((nick, win));
        }

        return rankingList;
    }
    
    public async Task<List<(string, int)>> GetWinRatingRanking()
    {
        Query rankingQuery = _db.Collection("Ranking").OrderByDescending("winRating").Limit(10);
        List<(string, int)> rankingList = new List<(string, int)>();

        QuerySnapshot capitalQuerySnapshot = await rankingQuery.GetSnapshotAsync();

        foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
        {
            var data = documentSnapshot.ToDictionary();
            var nickRef = data["ref"] as DocumentReference;
            var nick = await GetNickname(nickRef);
            var winRating = Convert.ToInt32(data["winRating"]);
            rankingList.Add((nick, winRating));
        }

        return rankingList;
    }
    
    public async Task<List<(string, int)>> GetWinStraightRanking()
    {
        Query rankingQuery = _db.Collection("Ranking").OrderByDescending("maxWinStraight").Limit(10);
        List<(string, int)> rankingList = new List<(string, int)>();

        QuerySnapshot capitalQuerySnapshot = await rankingQuery.GetSnapshotAsync();

        foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
        {
            var data = documentSnapshot.ToDictionary();
            var nickRef = data["ref"] as DocumentReference;
            var nick = await GetNickname(nickRef);
            var maxWinStraight = Convert.ToInt32(data["maxWinStraight"]);
            rankingList.Add((nick, maxWinStraight));
        }

        return rankingList;
    }
    
    private void AddWinData(Dictionary<string, object> winDto)
    {
        GameManager.Instance.ActiveLoadingUI();
        _winRef.SetAsync(winDto).ContinueWithOnMainThread(_ => {
            GameManager.Instance.DeActiveLoadingUI();
        });
    }
    
    private Dictionary<string, object> GetNickDto(string nick)
    {
        var data = new Dictionary<string, object> {
            { "nickname", nick },
        };

        return data;
    }

    private Dictionary<string, object> GetWinDto(int win, int defeat, double winRating, int winStraight, bool isWinStraight, int maxWinStraight)
    {
        var data = new Dictionary<string, object> {
            { "win", win },
            { "defeat", defeat },
            { "winRating", winRating },
            { "winStraight", winStraight},
            { "isWinStraight", isWinStraight},
            { "maxWinStraight", maxWinStraight},
            { "ref", _userRef }
        };

        return data;
    }
}
