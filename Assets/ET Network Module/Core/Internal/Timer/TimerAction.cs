namespace ET
{
    public class TimerAction
    {
        public TimerClass TimerClass;
        public long Id;
        public bool IsDisposed => this.Id == 0;

        public object Object;
        public long Time;
        public int Type;

        public void Init(TimerClass timerClass, long time, int type, object obj)
        {
            Id = IdGenerater.Instance.GenerateId();
            TimerClass = timerClass;
            Object = obj;
            Time = time;
            Type = type;
        }
        public void Destroy()
        {
            Id = 0;
            Object = null;
            Time = 0;
            TimerClass = TimerClass.None;
            Type = 0;
        }

        internal void Dispose()
        {
            if (!IsDisposed)
            {
                Id = 0;
            }
        }
    }
}
