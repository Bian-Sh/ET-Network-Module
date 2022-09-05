using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace ET
{
    public static class SessionStreamDispatcher 
    {
        public static ISessionStreamDispatcher[] Dispatchers;
        public static void Init()
        {
            Dispatchers = new ISessionStreamDispatcher[100];
            List<Type> types = EventSystem.GetTypes(typeof(SessionStreamDispatcherAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(SessionStreamDispatcherAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                SessionStreamDispatcherAttribute sessionStreamDispatcherAttribute = attrs[0] as SessionStreamDispatcherAttribute;
                if (sessionStreamDispatcherAttribute == null)
                {
                    continue;
                }
                if (sessionStreamDispatcherAttribute.Type >= 100)
                {
                    Debug.LogError("session dispatcher type must < 100");
                    continue;
                }

                ISessionStreamDispatcher iSessionStreamDispatcher = Activator.CreateInstance(type) as ISessionStreamDispatcher;
                if (iSessionStreamDispatcher == null)
                {
                    Debug.LogError($"sessionDispatcher {type.Name} 需要继承 ISessionDispatcher");
                    continue;
                }
                Dispatchers[sessionStreamDispatcherAttribute.Type] = iSessionStreamDispatcher;
            }
        }
        public static void Dispatch(int type, Session session, MemoryStream memoryStream)
        {
            ISessionStreamDispatcher sessionStreamDispatcher = Dispatchers[type];
            if (sessionStreamDispatcher == null)
            {
                throw new Exception("maybe your NetInnerComponent or NetOuterComponent not set SessionStreamDispatcherType");
            }
            sessionStreamDispatcher.Dispatch(session, memoryStream);
        }
    }

    #region Assistance Type
    public interface ISessionStreamDispatcher
    {
        void Dispatch(Session session, MemoryStream stream);
    }
    public class SessionStreamDispatcherAttribute : BaseAttribute
    {
        public int Type;
        public SessionStreamDispatcherAttribute(int type) => this.Type = type;
    }
    public static class SessionStreamDispatcherType
    {
        public const int SessionStreamDispatcherClientOuter = 1;
        public const int SessionStreamDispatcherServerOuter = 2;
        public const int SessionStreamDispatcherServerInner = 3;
    }

    [SessionStreamDispatcher(SessionStreamDispatcherType.SessionStreamDispatcherClientOuter)]
    public class SessionStreamDispatcherClientOuter : ISessionStreamDispatcher
    {
        public void Dispatch(Session session, MemoryStream memoryStream)
        {
            ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
            if (!OpcodeTypeManager.TryGetType(opcode,out var type))
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
    #endregion
}