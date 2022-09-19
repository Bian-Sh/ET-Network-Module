using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [MessageHandler]
    public abstract class AMHandler<Message>: IMHandler where Message : class
    {
        public List<Action<Session, Message>> tasks = new List<Action<Session, Message>>();
        public  void Register(Action<Session, Message> task)
        {
            if (!tasks.Contains(task))
            {
                tasks.Add(task);
            }
        }
        public  void UnRegister(Action<Session, Message> task) => tasks.Remove(task);
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
            Debug.Log($"{nameof(AMHandler<Message>)}: 收到消息 {msg} ");
            tasks.ForEach(v=>v?.Invoke(session,message));
        }
        public Type GetMessageType() => typeof(Message);
        public Type GetResponseType() => null;
    }
}