using System;
using UnityEngine;

namespace ET
{
    [MessageHandler]
    public abstract class AMHandler<Message>: IMHandler where Message : class
    {
        protected abstract void Run(Session session, Message message);

        public void Handle(Session session, object msg)
        {
            Message message = msg as Message;
            if (message == null)
            {
               Debug.LogError($"消息类型转换错误: {msg.GetType().Name} to {typeof (Message).Name}");
                return;
            }

            if (session.IsDisposed)
            {
                Debug.LogError($"session disconnect {msg}");
                return;
            }

            this.Run(session, message);
        }

        public Type GetMessageType()
        {
            return typeof (Message);
        }

        public Type GetResponseType()
        {
            return null;
        }
    }
}