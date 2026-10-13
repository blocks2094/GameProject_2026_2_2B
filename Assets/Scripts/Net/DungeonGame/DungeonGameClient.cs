using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DungeonGameClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "http://127.0.0.1:3001";
    [SerializeField] private Text playerText;
    [SerializeField] private Text roomText;
    [SerializeField] private Text messageText;
    [SerializeField] private Button enterButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button chestButton;
    [SerializeField] private Button restButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button returnButton;
    private bool requesting;
    private DungeonStateData currentState;

    void Start()
    {
        // Use an installed Korean font so server messages are readable in the editor.
        string[] installedFonts = Font.GetOSInstalledFontNames();
        foreach (string fontName in new[] { "Malgun Gothic", "Apple SD Gothic Neo", "Noto Sans CJK KR" })
        {
            if (Array.IndexOf(installedFonts, fontName) < 0) continue;
            Font koreanFont = Font.CreateDynamicFontFromOSFont(fontName, 26);
            playerText.font = koreanFont;
            roomText.font = koreanFont;
            messageText.font = koreanFont;
            break;
        }
        enterButton.onClick.AddListener(() => StartCoroutine(Post("/api/dungeon/enter", "{}")));
        attackButton.onClick.AddListener(() => SendAction("ATTACK"));
        chestButton.onClick.AddListener(() => SendAction("OPEN_CHEST"));
        restButton.onClick.AddListener(() => SendAction("REST"));
        nextButton.onClick.AddListener(() => SendAction("NEXT_ROOM"));
        returnButton.onClick.AddListener(() => SendAction("RETURN"));
        StartCoroutine(GetState());

    }

    private void SendAction(string action)
    {
        string json = JsonUtility.ToJson(new DungeonActionRequest { action = action});
        StartCoroutine(Post("/api/dungeon/action", json));
    }

    private IEnumerator GetState()
    {
        if (requesting) yield break;
        BeginRequest();
        using UnityWebRequest request = UnityWebRequest.Get(serverUrl.TrimEnd('/') + "/api/game/state");
        request.timeout = 10;

        yield return request.SendWebRequest();
        ReadResponse(request);
        EndRequest();
    }

    private IEnumerator Post(string path, string json)
    {
        if (requesting) yield break;
        BeginRequest();
        using UnityWebRequest request = new UnityWebRequest(serverUrl.TrimEnd('/') + path, "POST");
        request.timeout = 10;
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        ReadResponse(request);
        EndRequest();
    }

    private void BeginRequest()
    {
        requesting = true;
        messageText.text = "서버에 요청 중입니다...";
        enterButton.interactable = false;
        SetActionButtons(false, false, false, false, false);
    }

    private void EndRequest()
    {
        requesting = false;
        if (currentState != null) Draw(currentState);
        else enterButton.interactable = true;
    }


    private void ReadResponse(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.DataProcessingError)
        {
            messageText.text = "서버에 연결할 수 없습니다. Node 폴더에서 npm start로 서버를 실행해 주세요.";
            Debug.LogWarning($"Dungeon server: {request.error}");
            return;
        }

        string json = request.downloadHandler == null ? "" : request.downloadHandler.text;
        DungeonResponse response;
        try
        {
            if (!json.TrimStart().StartsWith("{")) throw new ArgumentException("Expected JSON object");
            response = JsonUtility.FromJson<DungeonResponse>(json);
        }
        catch (ArgumentException)
        {
            messageText.text = $"서버 응답이 JSON 형식이 아닙니다. DungeonServer.js 실행 여부를 확인해 주세요. (HTTP {request.responseCode})";
            Debug.LogWarning($"Invalid dungeon response (HTTP {request.responseCode}): {json}");
            return;
        }

        if (response == null || string.IsNullOrEmpty(response.message))
        {
            messageText.text = "서버 응답을 읽을 수 없습니다.";
            return;
        }
        messageText.text = response.message;
        Debug.Log(response.message);

        if(!response.success || response.data == null)
        {
            return;
        }

        if (response.data.player == null ||
            (response.data.run != null && response.data.run.currentRoom == null))
        {
            messageText.text = "서버 응답에 플레이어 또는 방 정보가 없습니다.";
            return;
        }
        currentState = response.data;
    }

    private void Draw(DungeonStateData state)
    {
        DungeonPlayerData player = state.player;
        playerText.text = $"{player.name} Lv.{player.level}\n" +
                          $"HP {player.hp}/{player.maxHp}\n" +
                          $"Gold {player.gold}";

        if(state.run == null)
        {
            roomText.text = "진행 중인 던전 없음";
            enterButton.interactable = true;
            SetActionButtons(false, false, false, false, false);
            return;
        }

        DungeonRoomData room = state.run.currentRoom;
        roomText.text = $"방 {room.index + 1}\n\n" +
                        $"{GetRoomDescription(room)}\n\n" +
                        $"방 상태 : {room.state}\n" +
                        $"탐험 골드 : {state.run.runGold}";

        UpdateButtons(room, state.run.state);
    }


    private void UpdateButtons(DungeonRoomData room, string runState)
    {
        bool playing = runState == "IN_PROGRESS";
        bool active = room.state == "ACTIVE";
        bool cleared = room.state == "CLEARED";

        enterButton.interactable = !playing;
        SetActionButtons(
            playing && active && room.type == "MONSTER",
            playing && active && room.type == "TREASURE",
            playing && active && room.type == "HEAL",
            playing && cleared,
            playing && active && room.type == "EXIT"
        );
    }



    private string GetRoomDescription(DungeonRoomData room)
    {
        switch(room.type)
        {
            case "MONSTER":
                return $"{room.monsterName} 출현! \n" +
                       $"몬스터 HP : {room.monsterHp}\n" +
                       $"몬스터 공격력 : {room.monsterAttack}\n" +
                       $"처치 보상 : {room.rewardGold} Gold";
            case "TREASURE":
                return $"보물 상자를 발견 했습니다. \n 보상 : {room.rewardGold} Gold";
            case "HEAL":
                return $"회복의 샘을 발견했습니다. \n 휴식하면 HP를 30 회복합니다.";
            case "EXIT":
                return $"던전 출구를 발견했습니다. \n 귀환하면 탐험 골드를 획득합니다.";
            default:
                return "알 수 없는 방입니다.";
        }
    }

    private void SetActionButtons(bool attack, bool chest, bool rest, bool next, bool returnToTown)
    {
        attackButton.interactable = attack;
        chestButton.interactable = chest;
        restButton.interactable = rest;
        nextButton.interactable = next;
        returnButton.interactable = returnToTown;
    }
}
