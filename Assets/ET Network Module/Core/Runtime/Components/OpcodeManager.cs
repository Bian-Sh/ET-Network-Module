using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ET
{
    public static class OpcodeManager
    {
        public static HashSet<ushort> outrActorMessage = new HashSet<ushort>();
        public static readonly Dictionary<ushort, Type> opcodeTypes = new Dictionary<ushort, Type>();
        public static readonly Dictionary<Type, ushort> typeOpcodes = new Dictionary<Type, ushort>();
        public static readonly Dictionary<Type, Type> requestResponse = new Dictionary<Type, Type>();

        public static void Init()
        {
            opcodeTypes.Clear();
            typeOpcodes.Clear();
            requestResponse.Clear();
            List<Type> types = EventSystem.GetTypes(typeof(MessageAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
                if (attrs.Length == 0) continue;
                if (!(attrs[0] is MessageAttribute messageAttribute)) continue;
                opcodeTypes.Add(messageAttribute.Opcode, type);
                typeOpcodes.Add(type, messageAttribute.Opcode);
                if (OpcodeHelper.IsOuterMessage(messageAttribute.Opcode) && typeof(IActorMessage).IsAssignableFrom(type))
                {
                    outrActorMessage.Add(messageAttribute.Opcode);
                }
                // 检查request response
                if (typeof(IRequest).IsAssignableFrom(type))
                {
                    if (typeof(IActorLocationMessage).IsAssignableFrom(type))
                    {
                        requestResponse.Add(type, typeof(ActorResponse));
                        continue;
                    }
                    attrs = type.GetCustomAttributes(typeof(ResponseTypeAttribute), false);
                    if (attrs.Length == 0)
                    {
                        Debug.LogError($"not found responseType: {type}");
                        continue;
                    }
                    ResponseTypeAttribute responseTypeAttribute = attrs[0] as ResponseTypeAttribute;
                    requestResponse.Add(type, EventSystem.GetType($"ET.{responseTypeAttribute.Type}"));
                }
            }
        }
        public static bool IsOutrActorMessage(ushort opcode) => outrActorMessage.Contains(opcode);
        public static bool TryGetOpcode(Type type, out ushort opcode) => typeOpcodes.TryGetValue(type, out opcode);
        public static bool TryGetType(ushort opcode, out Type type) => opcodeTypes.TryGetValue(opcode, out type);

        public static Type GetResponseType(Type request)
        {
            if (!requestResponse.TryGetValue(request, out Type response))
            {
                throw new Exception($"not found response type, request type: {request.GetType().Name}");
            }
            return response;
        }

        public static void Log()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("opcodeTypes : ");
            foreach (var kv in opcodeTypes)
            {
                if (kv.Value == null)
                {
                    continue;
                }
                sb.AppendLine($"\t{kv.Key} - {kv.Value.Name}");
            }

            sb.AppendLine("requestResponse  ");
            foreach (var kv in requestResponse)
            {
                if (kv.Value == null)
                {
                    continue;
                }

                sb.AppendLine($"\t{kv.Key.Name} -  {kv.Value.Name}");
            }
            Debug.Log($"{nameof(OpcodeManager)}: {sb}");
        }
    }

}