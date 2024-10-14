using System;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using static GameConfig;

public static class GameConfig
{    
    //게임 버전 
    public static readonly string GameVersion = "1.1";

    //화면 해상도
    //public static readonly int Width = 2560;
    //public static readonly int Height = 1440;
    //
    public static readonly int Width = 1920;
    public static readonly int Height = 1080;

    //최대 가능 인원수
    public static readonly int MaxPlayersInRoom = 6;

    //난수
   public static System.Random rand = new System.Random();

    //웹사이트
    public static readonly string WebsiteURL = "http://j11b309.p.ssafy.io/signup";

    //유저 닉네임
    public static string UserNickName = string.Empty;

    //프레임 30, 60, 144 중 세팅
    public static readonly int FPS = 144;

    //서버 응답 대기 시간
    public static readonly float ServerWaitingTime = 10f;

    //방 이름 최대 길이
    public static readonly int maxRoomNameLength = 20;

    //씬 종류
    //씬 추가 시 SceneChanger.cs의 OnSceneLoaded에도 추가 바람
    public static string titleScene = "Title";
    public static readonly string lobbyScene = "Lobby";
    public static readonly string roomScene = "Room";
    public static readonly string inGameScene = "InGame";    
    public static readonly string loadingScene = "Loading";

    //맵의 개수
    public static int mapCount = 3;

    //튜토리얼 UI
    public static bool isShownTutorial = false;

    //플레이어 컬러
    public enum PlayerColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Indigo
    }

    //플레이어 프로퍼티
    public static readonly string isReady = "IsReady"; //서버에서 레디여부를 확인
    public static readonly string isInGameLoaded = "IsInGameLoaded";
    public static readonly string playerIdx = "playerIdx"; //플레이어 인덱스
    public static readonly string playerColor = "playerColor"; //플레이어 색상
    public static readonly string playerCharacter = "playerCharacter"; //플레이어 아바타
    public static readonly string isDead = "IsDead";

    public static Color GetPlayerColor(PlayerColor playerColor)
    {
        switch (playerColor)
        {
            case PlayerColor.Red:
                return Color.red;
            case PlayerColor.Orange:
                return new Color(1f, 0.5f, 0f); // 주황색 (RGB)
            case PlayerColor.Yellow:
                return Color.yellow;
            case PlayerColor.Green:
                return Color.green;
            case PlayerColor.Blue:
                return Color.blue;
            case PlayerColor.Indigo:
                return new Color(0.29f, 0f, 0.51f); // 남색 (RGB)
            default:
                return Color.white;  // 기본값으로 흰색
        }
    }

    private static readonly string[] randomRoomName = new string[]
    {
        "같이 게임하자냥!",
        "다 덤벼라냥! 드루와 드루와",
        "매너게임해애옹!",
        "너 냥못하잖냥 ㅋㅋ",
        "최강의 로얄캣은 바로 냥!",
        "즐거운 게임 하자냥!"
    };
    public static string SetRandomRoomName()
    {
        return randomRoomName[UnityEngine.Random.Range(0, randomRoomName.Length)];
    }
    public static string GenerateRoomCode(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        char[] uid = new char[length];
                
        for (int i = 0; i < length; i++)
        {
            uid[i] = chars[rand.Next(chars.Length)];
        }
        return new string(uid);
    }

    //방 이름과 방 코드를 분리하는 함수
    public static string[] ParseString(string roomName)
    {
        string[] parts = roomName.Split('#');

        //반드시 2개로 나눠지기
        if (parts.Length == 2)
        {
            return parts;
        }

        //Debug.Log("방 이름 파싱 오류! : " + roomName);
        return null;
    }
    
    //n부터 m까지의 수를 섞는 함수
    public static List<int> GetShuffledList(int n, int m)
    {
        List<int> list = new List<int>();

        for(int i = n; i<=m; i++)
        {
            list.Add(i);
        }

        for(int i = list.Count -1; i> 0; i--)
        {
            int j = rand.Next(0, i+1);

            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        return list;
    }
}
