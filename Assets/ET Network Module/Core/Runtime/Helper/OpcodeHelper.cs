using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class OpcodeHelper
    {
        private static readonly HashSet<ushort> ignoreDebugLogMessageSet = new HashSet<ushort>
        {
            Opcode.C2G_Ping,
            Opcode.G2C_Ping,

        };

        private static bool IsNeedLogMessage(ushort opcode) => !ignoreDebugLogMessageSet.Contains(opcode);
        public static bool IsOuterMessage(ushort opcode) => opcode < OpcodeRangeDefine.OuterMaxOpcode;
        public static bool IsInnerMessage(ushort opcode) => opcode >= OpcodeRangeDefine.InnerMinOpcode;

        public static void LogMsg(int zone, ushort opcode, object message)
        {
            if (!Application.isEditor&&!Debug.isDebugBuild) // 不是编辑器并且不是调试包的情况下不输出log
            {
                return;
            }
            if (!IsNeedLogMessage(opcode))
            {
                return;
            }
            Debug.Log($"zone: {zone} {message}");
        }

        public static void LogMsg(ushort opcode, long actorId, object message)
        {
            if (!Application.isEditor && !Debug.isDebugBuild) // 不是编辑器并且不是调试包的情况下不输出log
            {
                return;
            }
            if (!IsNeedLogMessage(opcode))
            {
                return;
            }
            Debug.Log($"opcode -  actorId -  message:{opcode} -  {actorId} -  {message}");
        }
    }
}