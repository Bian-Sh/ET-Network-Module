using System;

namespace ET
{
    public static class TimeHelper
    {
        public const long OneDay = 86400000;
        public const long Hour = 3600000;
        public const long Minute = 60000;

        /// <summary>
        /// 客户端时间
        /// </summary>
        /// <returns></returns>
        public static long ClientNow() => TimeInfo.Instance.ClientNow;
        public static long ClientNowSeconds() => ClientNow() / 1000;
        public static DateTime DateTimeNow() => DateTime.Now;
        public static long ServerNow() => TimeInfo.Instance.ServerNow();
        public static long ClientFrameTime() => TimeInfo.Instance.ClientFrameTime();
        public static long ServerFrameTime() => TimeInfo.Instance.ServerFrameTime();
    }
}
