namespace ET
{
    public class TimerAction
    {
        public TimerClass TimerClass;
        public long Id;
        public object Object;
        public long Time;
        public int Type;

        public void Init(long id, TimerClass timerClass, long time, int type, object obj)
        {
            Id = id;
            TimerClass = timerClass;
            Object = obj;
            Time = time;
            Type = type;
        }
        public void Reset()
        {
            Id = 0;
            Object = null;
            Time = 0;
            TimerClass = TimerClass.None;
            Type = 0;
        }
    }
}
