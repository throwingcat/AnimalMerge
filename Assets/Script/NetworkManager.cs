using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using BackEnd.Tcp;
using UnityEngine;
using Violet;

public class NetworkManager : MonoSingleton<NetworkManager>
{
    #region 로그인

    public bool Login()
    {
        BackendReturnObject bro = null;
        if (GameManager.Instance.GUID.IsNullOrEmpty())
        {
            bro = Backend.BMember.GuestLogin();
        }
        else
        {
            bro = Backend.BMember.CustomSignUp(GameManager.Instance.GUID, "1234");
            bro = Backend.BMember.CustomLogin(GameManager.Instance.GUID, "1234");
        }
        if (bro.IsSuccess())
        {
            Debug.Log("로그인 성공");
            return true;
        }

        return false;
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

    public void Update()
    {
        if (Backend.Match != null)
        {
            if (Backend.Match.IsMatchServerConnect())
                Backend.Match.Poll();
        }
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

    private IEnumerator MatchMaking()
    {
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
}