using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ET
{
    public static class MessageDispatcher
    {
        public static readonly Dictionary<ushort, IMHandler> Handlers = new Dictionary<ushort, IMHandler>();
        public static void Init()
        {
            Handlers.Clear();
            List<Type> types = EventSystem.GetTypes(typeof(MessageHandlerAttribute));
            foreach (Type type in types)
            {
                IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler;
                if (iMHandler == null)
                {
                    Debug.LogError($"message handle {type.Name} 需要继承 IMHandler");
                    continue;
                }
                Type messageType = iMHandler.GetMessageType();
                if (!OpcodeManager.TryGetOpcode(messageType, out var opcode))
                {
                    throw new Exception($"消息 {messageType.GetType().Name} 未指定绑定 opcode !");
                }
                if (opcode == 0)
                {
                    Debug.LogError($"消息opcode为0: {messageType.Name}");
                    continue;
                }
                RegisterHandler(opcode, iMHandler);
            }
        }

        static void RegisterHandler(ushort opcode, IMHandler handler)
        {
            if (!Handlers.ContainsKey(opcode))
            {
                Handlers.Add(opcode, handler);
            }
            else
            {
                OpcodeManager.TryGetType(opcode, out var type);
                Debug.LogError($"{nameof(MessageDispatcher)}: {type.Name} - {opcode}  存在多个消息处理器，同一个消息只需要有一个处理器即可！");
            }
        }

        public static void Handle(Session session, ushort opcode, object message)
        {
            if (!Handlers.TryGetValue(opcode, out var handler))
            {
                Debug.LogError($"消息没有处理: {opcode} {message}");
                return;
            }
            try
            {
                handler.Handle(session, message);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        public static string Log()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HAHAHHA");
            foreach (var kv in Handlers)
            {
                if (kv.Value == null)
                {
                    continue;
                }
                sb.AppendLine($"Handler : {kv.Key} ");
                var handler = kv.Value;
                sb.AppendLine($"\t {handler.GetResponseType().Name} - {handler.GetMessageType().Name}");
            }
            return sb.ToString();
        }

        public static AMHandler<Message> GetHandler<Message>() where Message : class, IMessage
        {
            var type = typeof(Message);
            if (OpcodeManager.TryGetOpcode(type, out var opcode))
            {
                if (Handlers.TryGetValue(opcode, out var handler))
                {
                    return handler as AMHandler<Message>;
                }
                Debug.LogError($"{nameof(MessageDispatcher)}: 请留意 {type.Name} 未生成对应的 Handler !\n请使用菜单栏“Tools/生成非RPC消息处理器”生成对应的 Handler");
            }
            else
            {
                Debug.LogError($"{nameof(MessageDispatcher)}: 请留意 {type.Name} 未绑定 opcode !");
            }
            return null;
        }
    }
}
