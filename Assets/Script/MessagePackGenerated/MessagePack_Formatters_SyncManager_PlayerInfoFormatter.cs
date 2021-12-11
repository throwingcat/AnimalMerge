// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1200 // Using directives should be placed correctly
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters
{
    using global::System.Buffers;
    using global::MessagePack;

    public sealed class SyncManager_PlayerInfoFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::SyncManager.PlayerInfo>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::SyncManager.PlayerInfo value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(4);
            formatterResolver.GetFormatterWithVerify<global::SyncManager.ePacketType>().Serialize(ref writer, value.PacketType, options);
            formatterResolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroKey, options);
            writer.Write(value.MMR);
            formatterResolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
        }

        public global::SyncManager.PlayerInfo Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var ____result = new global::SyncManager.PlayerInfo();

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        ____result.PacketType = formatterResolver.GetFormatterWithVerify<global::SyncManager.ePacketType>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        ____result.HeroKey = formatterResolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        ____result.MMR = reader.ReadInt32();
                        break;
                    case 3:
                        ____result.Name = formatterResolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.Depth--;
            return ____result;
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1200 // Using directives should be placed correctly
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name
