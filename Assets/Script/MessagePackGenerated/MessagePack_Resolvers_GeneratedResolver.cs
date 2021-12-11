// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

#pragma warning disable SA1200 // Using directives should be placed correctly
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Resolvers
{
    using System;

    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        private GeneratedResolver()
        {
        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            internal static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    Formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        private static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(18)
            {
                { typeof(global::System.Collections.Generic.Dictionary<string, object>), 0 },
                { typeof(global::System.Collections.Generic.List<byte[]>), 1 },
                { typeof(global::System.Collections.Generic.List<global::ItemInfo>), 2 },
                { typeof(global::Define.eItemType), 3 },
                { typeof(global::Packet.ePACKET_TYPE), 4 },
                { typeof(global::SyncManager.ePacketType), 5 },
                { typeof(global::Common.PacketBase), 6 },
                { typeof(global::Common.PacketReward), 7 },
                { typeof(global::ItemInfo), 8 },
                { typeof(global::SyncManager.AttackDamage), 9 },
                { typeof(global::SyncManager.GameResult), 10 },
                { typeof(global::SyncManager.PlayerInfo), 11 },
                { typeof(global::SyncManager.Ready), 12 },
                { typeof(global::SyncManager.SVector3), 13 },
                { typeof(global::SyncManager.SyncPacket), 14 },
                { typeof(global::SyncManager.SyncPacketBase), 15 },
                { typeof(global::SyncManager.UnitData), 16 },
                { typeof(global::SyncManager.UpdateUnit), 17 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.DictionaryFormatter<string, object>();
                case 1: return new global::MessagePack.Formatters.ListFormatter<byte[]>();
                case 2: return new global::MessagePack.Formatters.ListFormatter<global::ItemInfo>();
                case 3: return new MessagePack.Formatters.Define.eItemTypeFormatter();
                case 4: return new MessagePack.Formatters.Packet.ePACKET_TYPEFormatter();
                case 5: return new MessagePack.Formatters.SyncManager_ePacketTypeFormatter();
                case 6: return new MessagePack.Formatters.Common.PacketBaseFormatter();
                case 7: return new MessagePack.Formatters.Common.PacketRewardFormatter();
                case 8: return new MessagePack.Formatters.ItemInfoFormatter();
                case 9: return new MessagePack.Formatters.SyncManager_AttackDamageFormatter();
                case 10: return new MessagePack.Formatters.SyncManager_GameResultFormatter();
                case 11: return new MessagePack.Formatters.SyncManager_PlayerInfoFormatter();
                case 12: return new MessagePack.Formatters.SyncManager_ReadyFormatter();
                case 13: return new MessagePack.Formatters.SyncManager_SVector3Formatter();
                case 14: return new MessagePack.Formatters.SyncManager_SyncPacketFormatter();
                case 15: return new MessagePack.Formatters.SyncManager_SyncPacketBaseFormatter();
                case 16: return new MessagePack.Formatters.SyncManager_UnitDataFormatter();
                case 17: return new MessagePack.Formatters.SyncManager_UpdateUnitFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1200 // Using directives should be placed correctly
#pragma warning restore SA1649 // File name should match first type name
