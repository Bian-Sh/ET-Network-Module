using System;
using UnityEngine;

namespace ET
{
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
        private TimeInfo()
        {
            FrameTime = this.ClientNow();
            var go = GameObject.FindObjectOfType<TimerDriver>();
            if (!go)
            {
                go = new GameObject(nameof(TimerDriver)).AddComponent<TimerDriver>();
                go.gameObject.hideFlags = HideFlags.HideAndDontSave;
                GameObject.DontDestroyOnLoad(go);
            }
            go.task = Update;
        }

        void Update() => this.FrameTime = this.ClientNow();

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

        class TimerDriver : MonoBehaviour
        {
            public Action task;
            private void Update() => task?.Invoke();
        }
    }
}