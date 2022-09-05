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
        public static long ClientNow() => TimeInfo.Instance.ClientNow();
        public static long ClientNowSeconds() => ClientNow() / 1000;
        public static DateTime DateTimeNow() => DateTime.Now;
        public static long ServerNow() => TimeInfo.Instance.ServerNow();
        public static long ClientFrameTime() => TimeInfo.Instance.ClientFrameTime();
        public static long ServerFrameTime() => TimeInfo.Instance.ServerFrameTime();
    }
    public class TimeInfo : IDisposable
    {
        public static TimeInfo Instance = new TimeInfo();
        private int timeZone;
        public int TimeZone
        {
            get => this.timeZone;
            set
            {
                this.timeZone = value;
                dt = dt1970.AddHours(value);
            }
        }

        private readonly DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public long ServerMinusClientTime { private get; set; }
        public long FrameTime;
        private TimeInfo() => this.FrameTime = this.ClientNow();
        public void Update() => this.FrameTime = this.ClientNow();

        /// <summary> 
        /// 根据时间戳获取时间 
        /// </summary>  
        public DateTime ToDateTime(long timeStamp) => dt.AddTicks(timeStamp * 10000);
        // 线程安全
        public long ClientNow() => (DateTime.UtcNow.Ticks - this.dt1970.Ticks) / 10000;
        public long ServerNow() => ClientNow() + Instance.ServerMinusClientTime;
        public long ClientFrameTime() => this.FrameTime;
        public long ServerFrameTime() => this.FrameTime + Instance.ServerMinusClientTime;
        public long Transition(DateTime d) => (d.Ticks - dt.Ticks) / 10000;
        public void Dispose() => Instance = null;
    }
}