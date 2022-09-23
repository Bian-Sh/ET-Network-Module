using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace ET
{
    public static class EventSystem
    {
        private static readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, Type> allTypes = new Dictionary<string, Type>();
        private static readonly UnOrderMultiMap<Type, Type> types = new UnOrderMultiMap<Type, Type>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void InitEnv()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                                 .Where(v => v.FullName.StartsWith("com.network") || v.FullName.StartsWith("Assembly-CSharp"))
                                 .ToArray();
            Add(asm);
            OpcodeManager.Init();  // 一定是先初始化 Opcode Manager，因为消息分发依赖他
            SessionStreamDispatcherManager.Init();
            MessageDispatcher.Init();
            TimerManager.Init();
            SetPlayerLoop();
        }

        private static void SetPlayerLoop()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var idx = Array.FindIndex(playerLoop.subSystemList, v => v.type == typeof(UnityEngine.PlayerLoop.Update));
            var update = playerLoop.subSystemList[idx];
            var updateSubSystems = update.subSystemList.ToList();
            updateSubSystems.Add(new PlayerLoopSystem()
            {
                type = typeof(EventSystem),
                updateDelegate = Update
            });
            update.subSystemList = updateSubSystems.ToArray();
            playerLoop.subSystemList[idx] = update;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        static void Update()
        {
            TimeInfo.Update();
            TimerManager.Update();
        }


        private static List<Type> GetBaseAttributes()
        {
            List<Type> attributeTypes = new List<Type>();
            foreach (var kv in allTypes)
            {
                Type type = kv.Value;
                if (type.IsAbstract)
                    continue;
                if (type.IsSubclassOf(typeof(BaseAttribute)))
                {
                    attributeTypes.Add(type);
                }
            }
            return attributeTypes;
        }

        public static void Add(Dictionary<string, Type> addTypes)
        {
            allTypes.Clear();
            foreach (var kv in addTypes)
            {
                allTypes[kv.Key] = kv.Value;
            }

            types.Clear();
            List<Type> baseAttributeTypes = GetBaseAttributes();
            foreach (Type baseAttributeType in baseAttributeTypes)
            {
                foreach (var kv in allTypes)
                {
                    Type type = kv.Value;
                    if (type.IsAbstract)
                    {
                        continue;
                    }
                    object[] objects = type.GetCustomAttributes(baseAttributeType, true);
                    if (objects.Length == 0)
                    {
                        continue;
                    }
                    types.Add(baseAttributeType, type);
                }
            }
        }

        public static void Add(Assembly[] asmarr)
        {
            if (null != asmarr && asmarr.Length > 0)
            {
                foreach (var asm in asmarr)
                {
                    assemblies[$"{asm.GetName().Name}.dll"] = asm;
                }
                Dictionary<string, Type> dictionary = new Dictionary<string, Type>();

                foreach (Assembly ass in assemblies.Values)
                {
                    foreach (Type type in ass.GetTypes())
                    {
                        dictionary[type.FullName] = type;
                    }
                }
                Add(dictionary);
            }
        }

        public static List<Type> GetTypes(Type systemAttributeType) => types[systemAttributeType];
        public static Dictionary<string, Type> GetTypes() => allTypes;
        public static Type GetType(string typeName) => allTypes[typeName];
        public static void Log()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"allTypes Count:  {allTypes.Count}");
            foreach (var kv in allTypes)
            {
                if (kv.Value == null) continue;
                sb.AppendLine($"\t{kv.Key}: {kv.Value.Name}");
            }
            sb.AppendLine($"types Count:  {types.Count}");
            foreach (var kv in types)
            {
                if (kv.Value == null) continue;
                sb.AppendLine($"\t{kv.Key}");
                foreach (var typ in kv.Value)
                {
                    sb.AppendLine($"\t\t{typ.Name}");
                }
            }
            Debug.Log($"{nameof(EventSystem)}: {sb}");
        }

    }
}
