using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

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
                                 .Where(v => v.FullName.StartsWith("Unity.ThirdParty") || (v.FullName.StartsWith("Assembly-CSharp") && !v.FullName.StartsWith("Assembly-CSharp-Editor")))
                                 .ToArray();
            Add(asm); 
            OpcodeTypeManager.Init();  // 一定是先初始化 Opcode Manager，因为消息分发依赖他
            SessionStreamDispatcher.Init();
            MessageDispatcher.Init();
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
                    //     UnityEngine.Debug.Log($"{nameof(EventSystem)}:   asm {asm.FullName}");
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
        public static string Log()
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
            return sb.ToString();
        }

    }
}
