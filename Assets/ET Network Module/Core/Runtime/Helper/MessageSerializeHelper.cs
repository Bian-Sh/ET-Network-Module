using System;
using System.IO;

namespace ET
{
    public static class MessageSerializeHelper
    {
        public static object DeserializeFrom(ushort opcode, Type type, MemoryStream memoryStream)
        {
            if (opcode < OpcodeRangeDefine.PbMaxOpcode)
            {
                return ProtobufHelper.FromStream(type, memoryStream);
            }
            throw new Exception($"client no message: {opcode}");
        }
        public static void SerializeTo(ushort opcode, object obj, MemoryStream memoryStream)
        {
            try
            {
                if (opcode < OpcodeRangeDefine.PbMaxOpcode)
                {
                    ProtobufHelper.ToStream(obj, memoryStream);
                    return;
                }
                throw new Exception($"client no message: {opcode}");
            }
            catch (Exception e)
            {
                throw new Exception($"SerializeTo error: {opcode}", e);
            }
        }

        public static MemoryStream GetStream(int count = 0)
        {
            MemoryStream stream;
            if (count > 0)
            {
                stream = new MemoryStream(count);
            }
            else
            {
                stream = new MemoryStream();
            }
            return stream;
        }

        public static (ushort, MemoryStream) MessageToStream(object message)
        {
            if (!OpcodeTypeManager.TryGetOpcode(message.GetType(),out var opcode))
            {
                throw new Exception($"消息 {message.GetType().Name} 未指定 opcode !");
            }
            int headOffset = Packet.ActorIdLength;
            MemoryStream stream = GetStream(headOffset + Packet.OpcodeLength);
            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(headOffset + Packet.OpcodeLength);
            stream.GetBuffer().WriteTo(headOffset, opcode);
            SerializeTo(opcode, message, stream);
            stream.Seek(0, SeekOrigin.Begin);
            return (opcode, stream);
        }
    }
}