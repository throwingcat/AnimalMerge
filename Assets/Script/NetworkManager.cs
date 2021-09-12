using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using BackEnd.Tcp;
using MessagePack;
using Packet;
using UnityEngine;
using Violet;

public class NetworkManager : MonoSingleton<NetworkManager>
{
    public void Update()
    {
        //일반 통신 업데이트
        for (int i = 0; i < _reservedPacket.Count; i++)
        {
            SendPacket(_reservedPacket[i]);
            _reservedPacket.RemoveAt(i--);
        }

        //매칭 업데이트
        if (Backend.Match != null)
        {
            if (Backend.Match.IsMatchServerConnect())
                Backend.Match.Poll();
        }
    }

    #region 로그인

    public bool Login()
    {
        Debug.Log("로그인 시작");
        Debug.Log(Backend.Utils.GetGoogleHash());
        if (GameManager.Instance.GUID.IsNullOrEmpty())
            GameManager.Instance.GUID = SystemInfo.deviceUniqueIdentifier;

        bool isLoginComplete = false;

        //액세스 토큰 로그인 시도
        var bro = Backend.BMember.LoginWithTheBackendToken();
        if (bro.IsSuccess())
        {
            Debug.Log("로그인 성공");
            isLoginComplete = true;
        }
        else
        {
            Debug.LogFormat("로그인 실패 : {0} / {1}", bro.GetErrorCode(), bro.GetMessage());

            //회원가입 진행
            bro = Backend.BMember.CustomSignUp(GameManager.Instance.GUID, "1234");
            if (bro.IsSuccess() == false)
                Debug.LogFormat("회원가입 실패 : {0} / {1}", bro.GetErrorCode(), bro.GetMessage());
            isLoginComplete = false;
        }

        return isLoginComplete;
    }

    public bool CreateNickname(string nickname)
    {
        BackendReturnObject bro = Backend.BMember.CreateNickname(nickname);
        if (bro.IsSuccess())
        {
            Debug.Log("닉네임 생성 성공");
            return true;
        }

        return false;
    }

    #endregion

    #region 매칭

    public enum eMatchingStep
    {
        DISCONNECTED,
        MATCHSERVER_CONNECT,
        MATCHSERVER_CONNECT_COMPLETE,
        WAITROOM_CREATE,
        WAITROOM_CREATE_COMPLETE,
        MATCHING,
        MATCHING_COMPLETE,
        INGAMESERVER_CONNECT,
        INGAMESERVER_CONNECT_COMPLETE,
        JOIN_GAMEROOM,
        JOIN_GAMEROOM_COMPLETE,
        WAIT_OTHER_PLAYER,
        COMPLETE,
        AREADY_CONNECT,
        ERROR
    }

    public eMatchingStep MatchingStep = eMatchingStep.DISCONNECTED;
    private MatchMakingResponseEventArgs _matchMakingResponseEventArgs;

    public void OnMatchingStart()
    {
        if (MatchingStep == eMatchingStep.DISCONNECTED)
            StartCoroutine(MatchingProcess());
    }

    private IEnumerator MatchingProcess()
    {
        //매치 서버 접속
        yield return StartCoroutine(ConnectionMatchServer());

        //서버 연결 실패
        if (MatchingStep != eMatchingStep.MATCHSERVER_CONNECT_COMPLETE)
        {
            Debug.LogError("매치서버 연결 실패");
            yield break;
        }

        //대기방 생성
        yield return StartCoroutine(CreateRoom());

        //방 생성 실패
        if (MatchingStep != eMatchingStep.WAITROOM_CREATE_COMPLETE)
        {
            Debug.LogError("대기방 생성 실패");
            yield break;
        }

        //매칭 신청
        yield return StartCoroutine(MatchMaking());

        //매칭 실패
        if (MatchingStep != eMatchingStep.MATCHING_COMPLETE)
        {
            Debug.LogError("매칭 실패");
            yield break;
        }

        //인게임 서버 접속
        yield return StartCoroutine(JoinInGameServer());
        //인게임 서버 실패
        if (MatchingStep != eMatchingStep.INGAMESERVER_CONNECT_COMPLETE)
        {
            Debug.LogError("인게임 서버 접속 실패");
            yield break;
        }

        //게임룸 접속
        yield return StartCoroutine(JoinGameRoom());
        //게임룸 접속 실패
        if (MatchingStep != eMatchingStep.JOIN_GAMEROOM_COMPLETE)
        {
            Debug.LogError("게임룸 접속 실패");
            yield break;
        }
    }

    #region 매치서버 접속

    private IEnumerator ConnectionMatchServer()
    {
        MatchingStep = eMatchingStep.MATCHSERVER_CONNECT;

        ErrorInfo errorInfo = new ErrorInfo();

        var isSocketConnect = Backend.Match.JoinMatchMakingServer(out errorInfo);

        //서버 커넥션 이벤트 등록
        Backend.Match.OnJoinMatchMakingServer -= OnJoinMatchMakingServer;
        Backend.Match.OnJoinMatchMakingServer += OnJoinMatchMakingServer;

        //소켓 연결 확인
        if (isSocketConnect == false)
            Debug.LogErrorFormat("매치서버 접속실패 {0}", errorInfo.Reason);

        //서버 커넥션 완료대기
        while (true)
        {
            if (MatchingStep == eMatchingStep.MATCHSERVER_CONNECT_COMPLETE || MatchingStep == eMatchingStep.ERROR)
                break;
            yield return null;
        }
    }

    private void OnJoinMatchMakingServer(JoinChannelEventArgs args)
    {
        if (args.ErrInfo == ErrorInfo.Success)
            MatchingStep = eMatchingStep.MATCHSERVER_CONNECT_COMPLETE;
        else
            MatchingStep = eMatchingStep.ERROR;
    }

    #endregion

    #region 대기방 생성

    private IEnumerator CreateRoom()
    {
        MatchingStep = eMatchingStep.WAITROOM_CREATE;

        //대기방 생성
        Backend.Match.CreateMatchRoom();
        //이벤트 등록
        Backend.Match.OnMatchMakingRoomCreate -= OnMatchMakingRoomCreate;
        Backend.Match.OnMatchMakingRoomCreate += OnMatchMakingRoomCreate;

        //완료대기
        while (true)
        {
            if (MatchingStep == eMatchingStep.WAITROOM_CREATE_COMPLETE || MatchingStep == eMatchingStep.ERROR)
                break;
            yield return null;
        }
    }

    private void OnMatchMakingRoomCreate(MatchMakingInteractionEventArgs args)
    {
        if (args.ErrInfo == ErrorCode.Success)
            MatchingStep = eMatchingStep.WAITROOM_CREATE_COMPLETE;
        else
            MatchingStep = eMatchingStep.ERROR;
    }

    #endregion

    #region 매칭 신청

    public float MatchMakingElapsed = 0f;

    private IEnumerator MatchMaking()
    {
        MatchMakingElapsed = 0f;
        MatchingStep = eMatchingStep.MATCHING;

        //매치 메이킹 신청
        BackendReturnObject bro = Backend.Match.GetMatchList();
        string indate = bro.GetInDate();
        Debug.LogFormat("Match Making InDate : {0}", indate);
        Backend.Match.RequestMatchMaking(MatchType.Random, MatchModeType.OneOnOne, indate);
        //매치 메이킹 응답 이벤트 등록
        Backend.Match.OnMatchMakingResponse -= OnMatchMakingResponse;
        Backend.Match.OnMatchMakingResponse += OnMatchMakingResponse;

        //완료대기
        while (true)
        {
            MatchMakingElapsed += Time.deltaTime;

            if (5f <= MatchMakingElapsed)
            {
                MatchMakingElapsed = 0f;

                int index = 0;
                while (true)
                {
                    var panel = SUIPanel.GetPanel(index++);
                    if (panel == null) break;
                    if (panel is PanelLobby)
                    {
                        (panel as PanelLobby).MatchMakingAI();
                        break;
                    }
                }
            }

            if (MatchingStep == eMatchingStep.DISCONNECTED) break;

            if (_matchMakingResponseEventArgs != null)
            {
                if (_matchMakingResponseEventArgs.ErrInfo == ErrorCode.Success)
                {
                    MatchingStep = eMatchingStep.MATCHING_COMPLETE;
                    break;
                }

                if (_matchMakingResponseEventArgs.ErrInfo != ErrorCode.Success &&
                    _matchMakingResponseEventArgs.ErrInfo != ErrorCode.Match_InProgress)
                {
                    MatchingStep = eMatchingStep.ERROR;
                    break;
                }
            }

            yield return null;
        }
    }

    private void OnMatchMakingResponse(MatchMakingResponseEventArgs args)
    {
        _matchMakingResponseEventArgs = args;
    }

    #endregion

    #region 인게임서버 접속

    private IEnumerator JoinInGameServer()
    {
        MatchingStep = eMatchingStep.INGAMESERVER_CONNECT;

        //인게임서버 접속 신청
        var serverAddress = _matchMakingResponseEventArgs.RoomInfo.m_inGameServerEndPoint.m_address;
        var serverPort = _matchMakingResponseEventArgs.RoomInfo.m_inGameServerEndPoint.m_port;
        var isReconnect = false;
        ErrorInfo errorInfo = null;

        bool isDone = false;
        if (Backend.Match.JoinGameServer(serverAddress, serverPort, isReconnect, out errorInfo))
        {
            //인게임서버 접속 응답 이벤트 등록
            Backend.Match.OnSessionJoinInServer -= OnSessionJoinInServer;
            Backend.Match.OnSessionJoinInServer += OnSessionJoinInServer;

            //완료대기
            while (true)
            {
                if (MatchingStep == eMatchingStep.INGAMESERVER_CONNECT_COMPLETE || MatchingStep == eMatchingStep.ERROR)
                    break;
                yield return null;
            }
        }
        else
        {
            MatchingStep = eMatchingStep.ERROR;
            yield break;
        }
    }

    private void OnSessionJoinInServer(JoinChannelEventArgs args)
    {
        if (args.ErrInfo == ErrorInfo.Success)
            MatchingStep = eMatchingStep.INGAMESERVER_CONNECT_COMPLETE;
        else
            MatchingStep = eMatchingStep.ERROR;
    }

    #endregion

    #region 게임룸 접속

    private IEnumerator JoinGameRoom()
    {
        MatchingStep = eMatchingStep.JOIN_GAMEROOM;

        //게임룸 접속 신청
        Backend.Match.JoinGameRoom(_matchMakingResponseEventArgs.RoomInfo.m_inGameRoomToken);
        //게임룸 접속 이벤트 등록
        Backend.Match.OnSessionListInServer -= OnSessionListInServer;
        Backend.Match.OnSessionListInServer += OnSessionListInServer;

        //완료대기
        while (true)
        {
            if (MatchingStep == eMatchingStep.JOIN_GAMEROOM_COMPLETE || MatchingStep == eMatchingStep.ERROR)
                break;
            yield return null;
        }
    }

    private void OnSessionListInServer(MatchInGameSessionListEventArgs args)
    {
        if (args.ErrInfo == ErrorCode.Success)
            MatchingStep = eMatchingStep.JOIN_GAMEROOM_COMPLETE;
        else
            MatchingStep = eMatchingStep.ERROR;
    }

    #endregion

    #endregion

    #region 일반

    private List<PacketBase> _reservedPacket = new List<PacketBase>();
    private Dictionary<ulong, Action<PacketBase>> _waitingPacket = new Dictionary<ulong, Action<PacketBase>>();

    public void Request(PacketBase packet, Action<PacketBase> onReceive)
    {
        bool isContains = false;
        foreach (var r in _reservedPacket)
        {
            if (r.PacketType == packet.PacketType)
            {
                isContains = true;
                break;
            }
        }

        if (isContains)
            return;
        ulong packetGUID = GameManager.Guid.NewGuid();

        packet.hash.Add("player_guid", GameManager.Instance.GUID);
        packet.hash.Add("packet_guid", packetGUID);

        //패킷 송신 예약
        _reservedPacket.Add(packet);

        //응답 대기열 등록
        if (_waitingPacket.ContainsKey(packetGUID) == false)
            _waitingPacket.Add(packetGUID, onReceive);
    }

    private void SendPacket(PacketBase packet)
    {
        //서버로 데이터 송신
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        var bytes = MessagePackSerializer.Serialize(packet, lz4Options);

        Server.AnimalMergeServer.Instance.OnReceivePacket(bytes);
    }

    public void ReceivePacket(byte[] bytes)
    {
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        PacketBase packet = MessagePackSerializer.Deserialize<Packet.PacketBase>(bytes, lz4Options);

        ulong guid = (ulong) packet.hash["packet_guid"];

        if (_waitingPacket.ContainsKey(guid))
        {
            switch (packet.PacketType)
            {
                case ePACKET_TYPE.CHEST_COMPLETE:
                case ePACKET_TYPE.QUEST_COMPLETE:
                case ePACKET_TYPE.DAILY_QUEST_REWARD:
                {
                    PacketReward res = MessagePackSerializer.Deserialize<Packet.PacketReward>(bytes, lz4Options);
                    _waitingPacket[guid]?.Invoke(res);
                }
                    break;
                default:
                    _waitingPacket[guid]?.Invoke(packet);
                    break;
            }

            _waitingPacket.Remove(guid);
        }
    }

    #endregion

    #region 종료

    public void ClearEvent()
    {
        Backend.Match.OnJoinMatchMakingServer -= OnJoinMatchMakingServer;
        Backend.Match.OnMatchMakingRoomCreate -= OnMatchMakingRoomCreate;
        Backend.Match.OnMatchMakingResponse -= OnMatchMakingResponse;
        Backend.Match.OnSessionJoinInServer -= OnSessionJoinInServer;
        Backend.Match.OnSessionListInServer -= OnSessionListInServer;
    }

    public void DisconnectMatchServer()
    {
        MatchingStep = eMatchingStep.DISCONNECTED;
        _matchMakingResponseEventArgs = null;
        if (Backend.Match.IsMatchServerConnect())
            Backend.Match.LeaveMatchMakingServer();
    }

    public void DisconnectIngameServer()
    {
        if (Backend.Match.IsInGameServerConnect())
            Backend.Match.LeaveGameServer();
    }

    public void DisconnectGameRoom()
    {
        if (eMatchingStep.WAITROOM_CREATE_COMPLETE <= MatchingStep)
            Backend.Match.LeaveMatchRoom();
    }

    #endregion
}