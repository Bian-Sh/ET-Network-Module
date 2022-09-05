using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ET
{
    public static class MessageDispatcher
    {
        public static readonly Dictionary<ushort, List<IMHandler>> Handlers = new Dictionary<ushort, List<IMHandler>>();
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
                if (!OpcodeTypeManager.TryGetOpcode(messageType, out var opcode))
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
                Handlers.Add(opcode, new List<IMHandler>());
            }
            Handlers[opcode].Add(handler);
        }

        public static void Handle(Session session, ushort opcode, object message)
        {
            List<IMHandler> actions;
            if (!Handlers.TryGetValue(opcode, out actions))
            {
                Debug.LogError($"消息没有处理: {opcode} {message}");
                return;
            }
            foreach (IMHandler ev in actions)
            {
                try
                {
                    ev.Handle(session, message);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
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
                foreach (var handler in kv.Value)
                {
                    sb.AppendLine($"\t {handler.GetResponseType().Name} - {handler.GetMessageType().Name}");
                }
            }
            return sb.ToString();
        }
    }
}
