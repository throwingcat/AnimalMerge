using System.Collections;
using BackEnd;
using BackEnd.Tcp;
using Define;
using UnityEngine;
using Violet;

public class PanelLobby : SUIPanel
{
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
        ERROR
    }

    private MatchMakingResponseEventArgs _matchMakingResponseEventArgs;
    public GameObject Matching;

    public eMatchingStep MatchingStep = eMatchingStep.DISCONNECTED;

    public void OnMatchingStart()
    {
        if (MatchingStep == eMatchingStep.DISCONNECTED) StartCoroutine(MatchingProcess());
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

        Matching.SetActive(true);

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

        //매칭 성공 , 게임 시작
        Backend.Match.Poll();
        Backend.Match.OnMatchInGameStart -= OnMatchInGameStart;
        Backend.Match.OnMatchInGameStart += OnMatchInGameStart;
    }

    public void OnMatchingCancel()
    {
        Backend.Match.LeaveMatchMakingServer();
        Backend.Match.OnLeaveMatchMakingServer -= OnLeaveMatchMakingServer;
        Backend.Match.OnLeaveMatchMakingServer += OnLeaveMatchMakingServer;

        Backend.Match.Poll();
    }

    private void OnLeaveMatchMakingServer(LeaveChannelEventArgs args)
    {
        Matching.SetActive(false);
    }


    public void OnClickGameStart()
    {
        OnMatchingStart();
    }

    public void OnClickMatchCancel()
    {
        OnMatchingCancel();
    }

    #region 매치서버 접속

    private IEnumerator ConnectionMatchServer()
    {
        MatchingStep = eMatchingStep.MATCHSERVER_CONNECT;

        ErrorInfo errorInfo;

        var isSocketConnect = Backend.Match.JoinMatchMakingServer(out errorInfo);

        //서버 커넥션 이벤트 등록
        Backend.Match.OnJoinMatchMakingServer -= OnJoinMatchMakingServer;
        Backend.Match.OnJoinMatchMakingServer += OnJoinMatchMakingServer;

        //소켓 연결 확인
        if (isSocketConnect == false)
            Debug.LogErrorFormat("매치서버 접속실패 {0}", errorInfo.Reason);
        //서버 커넥션 메세지 송출
        else
            Backend.Match.Poll();

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
        //메세지 송출
        Backend.Match.Poll();

        //완료대기
        while (true)
        {
            if (MatchingStep == eMatchingStep.MATCHSERVER_CONNECT_COMPLETE || MatchingStep == eMatchingStep.ERROR)
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

    private IEnumerator MatchMaking()
    {
        MatchingStep = eMatchingStep.MATCHING;

        //매치 메이킹 신청
        Backend.Match.RequestMatchMaking(MatchType.Random, MatchModeType.OneOnOne, "2021-07-09T14:49:03.759Z");
        //매치 메이킹 응답 이벤트 등록
        Backend.Match.OnMatchMakingResponse -= OnMatchMakingResponse;
        Backend.Match.OnMatchMakingResponse += OnMatchMakingResponse;
        //메세지 송출
        Backend.Match.Poll();

        //완료대기
        while (true)
        {
            if (MatchingStep == eMatchingStep.MATCHING_COMPLETE || MatchingStep == eMatchingStep.ERROR)
                break;
            yield return null;
        }
    }

    private void OnMatchMakingResponse(MatchMakingResponseEventArgs args)
    {
        _matchMakingResponseEventArgs = args;
        if (args.ErrInfo == ErrorCode.Success)
            MatchingStep = eMatchingStep.MATCHING_COMPLETE;
        else
            MatchingStep = eMatchingStep.ERROR;
    }

    #endregion

    #region 인게임서버 접속

    private IEnumerator JoinInGameServer()
    {
        MatchingStep = eMatchingStep.INGAMESERVER_CONNECT;

        //인게임서버 접속 신청
        var serverAddress = _matchMakingResponseEventArgs.RoomInfo.m_inGameServerEndPoint.m_address;
        var serverPort = _matchMakingResponseEventArgs.RoomInfo.m_inGameServerEndPoint.m_port;
        var isReconnect = true;
        ErrorInfo errorInfo = null;

        if (Backend.Match.JoinGameServer(serverAddress, serverPort, isReconnect, out errorInfo))
        {
            MatchingStep = eMatchingStep.ERROR;
            yield break;
        }

        //인게임서버 접속 응답 이벤트 등록
        Backend.Match.OnSessionJoinInServer -= OnSessionJoinInServer;
        Backend.Match.OnSessionJoinInServer += OnSessionJoinInServer;
        //메세지 송출
        Backend.Match.Poll();

        //완료대기
        while (true)
        {
            if (MatchingStep == eMatchingStep.INGAMESERVER_CONNECT_COMPLETE || MatchingStep == eMatchingStep.ERROR)
                break;
            yield return null;
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
        //메세지 송출
        Backend.Match.Poll();

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
    
    #region 게임 시작
    private void OnMatchInGameStart()
    {
        GameManager.Instance.ChangeGameState(eGAME_STATE.Battle);
    }
    #endregion
}