using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace ET
{
    public static class SessionStreamDispatcherManager 
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
    #endregion
}