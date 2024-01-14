using System;
using System.Collections.Generic;
using MessagePack;
using SyncPacketCollection;

public class SyncManager
{
    public GameCore From;
    public Action<SyncPacketBase> OnSyncReceive;

    private readonly Dictionary<ePacketType, SyncPacketBase> packetQueue =
        new Dictionary<ePacketType, SyncPacketBase>();

    public GameCore To;

    public SyncManager(GameCore from)
    {
        From = from;
        From.MyReadyTime.Subscribe(value =>
        {
            Request(new Ready
            {
                ReadyTime = value
            });
        }, false);

        From.MyStackDamage.Subscribe(value =>
        {
            Request(new UpdateStackDamage
            {
                StackDamage = value
            });
        }, false);

        From.AttackDamage.Subscribe(value =>
        {
            Request(new AttackDamage
            {
                Damage = value
            });
        }, false);

        From.AttackComboValue.Subscribe(value =>
        {
            Request(new UpdateAttackCombo
            {
                Combo = value
            });
        }, false);

        From.isGameOver.Subscribe(value =>
        {
            Request(new GameResult
            {
                isGameOver = value,
                GameOverTime = From.GameOverTime
            });
        }, false);
    }

    public void SetTo(GameCore to)
    {
        To = to;
    }

    public void Request(SyncPacketBase packet)
    {
        if (packetQueue.ContainsKey((ePacketType) packet.PacketType) == false)
            packetQueue.Add((ePacketType) packet.PacketType, packet);
        else
            packetQueue[(ePacketType) packet.PacketType] = packet;
    }

    public void Capture()
    {
        //유닛 목록 업데이트
        var pUpdateUnit = new UpdateUnit();
        var units = new List<UnitBase>();
        units.AddRange(From.UnitsInField);
        units.AddRange(From.BadUnits);
        foreach (var unit in units)
        {
            var u = new UnitData();
            u.UnitKey = (sbyte) unit.Sheet.index;
            u.UnitPosition = new SVector3(unit.transform.localPosition);
            u.UnitRotation = new SVector3(unit.transform.localRotation.eulerAngles);

            pUpdateUnit.UnitDatas.Add(u);
        }

        Request(pUpdateUnit);

        From.AttackDamage.Clear();
        From.AttackComboValue.Clear();

        //var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        foreach (var packet in packetQueue)
        {
            var bytes = MessagePackSerializer.Serialize(packet.Value);
            //싱글 플레이의 경우 From < - > To 끼리 바로 통신
            if (GameManager.Instance.isSinglePlay)
            {
                To.SyncManager.Receive(packet.Value);
            }
            else
            {
                // //매치 서버로 송신
                // if (Backend.Match.IsMatchServerConnect() && Backend.Match.IsInGameServerConnect())
                //     Backend.Match.SendDataToInGameRoom(bytes);
            }
        }

        packetQueue.Clear();
    }

    public void Receive(SyncPacketBase packet)
    {
        OnSyncReceive?.Invoke(packet);
    }

    // public void OnReceiveMatchRelay(MatchRelayEventArgs args)
    // {
    //     var packet = Utils.Deserialize<SyncPacketBase>(args.BinaryUserData);
    //     if (args.From.NickName != Backend.UserNickName)
    //         Receive(packet);
    // }
}