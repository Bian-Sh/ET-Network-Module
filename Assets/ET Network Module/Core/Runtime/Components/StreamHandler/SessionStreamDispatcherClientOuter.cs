using System;
using System.IO;
namespace ET
{
    [SessionStreamDispatcher(SessionStreamDispatcherType.SessionStreamDispatcherClientOuter)]
    public class SessionStreamDispatcherClientOuter : ISessionStreamDispatcher
    {
        public void Dispatch(Session session, MemoryStream memoryStream)
        {
            ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
            if (!OpcodeManager.TryGetType(opcode,out var type))
            {
                throw new Exception($"opcode : {opcode} 未映射有效消息！");
            }
            object message = MessageSerializeHelper.DeserializeFrom(opcode, type, memoryStream);
            if (message is IResponse response)
            {
                session.OnRead(opcode, response);
                return;
            }
            OpcodeHelper.LogMsg(0, opcode, message);
            // 普通消息或者是Rpc请求消息
            MessageDispatcher.Handle(session, opcode, message);
        }
    }
}