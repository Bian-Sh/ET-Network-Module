using ET;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public enum TimerClass
    {
        None,
        OnceTimer,
        OnceWaitTimer,
        RepeatedTimer,
    }

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
public class TimerComponent : MonoBehaviour
{
    public static TimerComponent Instance { get; set; }
    public long timeNow;
    /// <summary>
    /// key: time, value: timer id
    /// </summary>
    public readonly MultiMap<long, long> TimeId = new MultiMap<long, long>();
    public readonly Queue<long> timeOutTime = new Queue<long>();
    public readonly Queue<long> timeOutTimerIds = new Queue<long>();
    public readonly Queue<long> everyFrameTimer = new Queue<long>();

    // 记录最小时间，不用每次都去MultiMap取第一个值
    public long minTime;

    public const int TimeTypeMax = 10000;

    public ITimer[] timerActions;
    Dictionary<long, TimerAction> timers = new Dictionary<long, TimerAction>();

    public void Awake()
    {
        Instance = this;
        Init();
    }
    private void Init()
    {
        timerActions = new ITimer[TimeTypeMax];
        List<Type> types = EventSystem.GetTypes(typeof(TimerAttribute));
        foreach (Type type in types)
        {
            ITimer iTimer = Activator.CreateInstance(type) as ITimer;
            if (iTimer == null)
            {
                Debug.LogError($"Timer Action {type.Name} 需要继承 ITimer");
                continue;
            }

            object[] attrs = type.GetCustomAttributes(typeof(TimerAttribute), false);
            if (attrs.Length == 0)
            {
                continue;
            }

            foreach (object attr in attrs)
            {
                TimerAttribute timerAttribute = attr as TimerAttribute;
                timerActions[timerAttribute.Type] = iTimer;
            }
        }
    }
    public void OnDestroy()
    {
        Instance = null;
    }
    public void Update()
    {
        #region 每帧执行的timer，不用foreach TimeId，减少GC

        int count = everyFrameTimer.Count;
        for (int i = 0; i < count; ++i)
        {
            long timerId = everyFrameTimer.Dequeue();
            TimerAction timerAction = GetChild(timerId);
            if (timerAction == null)
            {
                continue;
            }
            Run(timerAction);
        }

        #endregion

        if (TimeId.Count == 0)
        {
            return;
        }
        timeNow = TimeHelper.ServerNow();
        if (timeNow < minTime)
        {
            return;
        }
        foreach (var kv in TimeId)
        {
            if (kv.Key > timeNow)
            {
                minTime = kv.Key;
                break;
            }
            timeOutTime.Enqueue(kv.Key);
        }
        while (timeOutTime.Count > 0)
        {
            long time = timeOutTime.Dequeue();
            var list = TimeId[time];
            for (int i = 0; i < list.Count; ++i)
            {
                long timerId = list[i];
                timeOutTimerIds.Enqueue(timerId);
            }
            TimeId.Remove(time);
        }

        while (timeOutTimerIds.Count > 0)
        {
            long timerId = timeOutTimerIds.Dequeue();
            TimerAction timerAction = GetChild(timerId);
            if (timerAction == null)
            {
                continue;
            }
            Run(timerAction);
        }
    }

    private TimerAction GetChild(long timerId)
    {
        if (!timers.TryGetValue(timerId, out var timer))
        {
            return null;
        }
        return timer;
    }
    public long NewRepeatedTimer(long time, int type, object args)
    {
        if (time < 100)
        {
            Debug.LogError($"time too small: {time}");
            return 0;
        }
        return NewRepeatedTimerInner(time, type, args);
    }

    /// <summary>
    /// 创建一个RepeatedTimer
    /// </summary>
    private long NewRepeatedTimerInner(long time, int type, object args)
    {
#if NOT_UNITY
			if (time < 100)
			{ 
				throw new Exception($"repeated timer < 100, timerType: time: {time}");
			}
#endif
        long tillTime = TimeHelper.ServerNow() + time;
        TimerAction timer = AddChild(TimerClass.RepeatedTimer, time, type, args);

        // 每帧执行的不用加到timerId中，防止遍历
        AddTimer(tillTime, timer);
        return timer.Id;
    }

    private void Run(TimerAction timerAction)
    {
        switch (timerAction.TimerClass)
        {
            case TimerClass.OnceTimer:
                {
                    int type = timerAction.Type;
                    ITimer timer = timerActions[type];
                    if (timer == null)
                    {
                        Debug.LogError($"not found timer action: {type}");
                        return;
                    }
                    timer.Handle(timerAction.Object);
                    break;
                }
            case TimerClass.OnceWaitTimer:
                {
                    ETTask<bool> tcs = timerAction.Object as ETTask<bool>;
                    Remove(timerAction.Id);
                    tcs.SetResult(true);
                    break;
                }
            case TimerClass.RepeatedTimer:
                {
                    int type = timerAction.Type;
                    long tillTime = TimeHelper.ServerNow() + timerAction.Time;
                    AddTimer(tillTime, timerAction);

                    ITimer timer = timerActions[type];
                    if (timer == null)
                    {
                        Debug.LogError($"not found timer action: {type}");
                        return;
                    }
                    timer.Handle(timerAction.Object);
                    break;
                }
        }
    }
    private void AddTimer(long tillTime, TimerAction timer)
    {
        if (timer.TimerClass == TimerClass.RepeatedTimer && timer.Time == 0)
        {
            everyFrameTimer.Enqueue(timer.Id);
            return;
        }
        TimeId.Add(tillTime, timer.Id);
        if (tillTime < minTime)
        {
            minTime = tillTime;
        }
    }
    public long NewFrameTimer(int type, object args)
    {
#if NOT_UNITY
			return NewRepeatedTimerInner(100, type, args);
#else
        return NewRepeatedTimerInner(0, type, args);
#endif
    }

    // 用这个优点是可以热更，缺点是回调式的写法，逻辑不连贯。WaitTillAsync不能热更，优点是逻辑连贯。
    // wait时间短并且逻辑需要连贯的建议WaitTillAsync
    // wait时间长不需要逻辑连贯的建议用NewOnceTimer
    public long NewOnceTimer(long tillTime, int type, object args)
    {
        if (tillTime < TimeHelper.ServerNow())
        {
            Debug.LogWarning($"new once time too small: {tillTime}");
        }
        TimerAction timer = AddChild(TimerClass.OnceTimer, tillTime, type, args);
        AddTimer(tillTime, timer);
        return timer.Id;
    }
    public async ETTask<bool> WaitAsync(long time, ETCancellationToken cancellationToken = null)
    {
        if (time == 0) return true;
        long tillTime = TimeHelper.ServerNow() + time;
        ETTask<bool> tcs = ETTask<bool>.Create(true);
        TimerAction timer = AddChild(TimerClass.OnceWaitTimer, time, 0, tcs);
        AddTimer(tillTime, timer);
        long timerId = timer.Id;
        void CancelAction()
        {
            if (Remove(timerId))
            {
                tcs.SetResult(false);
            }
        }
        bool ret;
        try
        {
            cancellationToken?.Add(CancelAction);
            ret = await tcs;
        }
        finally
        {
            cancellationToken?.Remove(CancelAction);
        }
        return ret;
    }

    public async ETTask<bool> WaitFrameAsync(ETCancellationToken cancellationToken = null) => await WaitAsync(1, cancellationToken);

    public bool Remove(ref long id)
    {
        long i = id;
        id = 0;
        return Remove(i);
    }

    private bool Remove(long id)
    {
        if (id == 0) return false;
        TimerAction timerAction = GetChild(id);
        if (timerAction == null) return false;
        timerAction.Dispose();
        Pool.Recycle(timerAction);
        return true;
    }

    public async ETTask<bool> WaitTillAsync(long tillTime, ETCancellationToken cancellationToken = null)
    {
        if (timeNow >= tillTime) return true;
        ETTask<bool> tcs = ETTask<bool>.Create(true);
        TimerAction timer = AddChild(TimerClass.OnceWaitTimer, tillTime - timeNow, 0, tcs);
        AddTimer(tillTime, timer);
        long timerId = timer.Id;
        void CancelAction()
        {
            if (Remove(timerId))
            {
                tcs.SetResult(false);
            }
        }

        bool ret;
        try
        {
            cancellationToken?.Add(CancelAction);
            ret = await tcs;
        }
        finally
        {
            cancellationToken?.Remove(CancelAction);
        }
        return ret;
    }

    private TimerAction AddChild(TimerClass onceWaitTimer, long value, int v1, object tcs)
    {
        var timer = Pool.Get();
        timer ??= new TimerAction();
        timer.Init(onceWaitTimer, value, v1, tcs);
        timers.Add(timer.Id,timer);
        return timer;
    }

    //TimerAction 对象池
    static class Pool
    {
        static Queue<TimerAction> queue = new Queue<TimerAction>();
        public static TimerAction Get()
        {
            if (queue.Count > 0)
            {
                return queue.Dequeue();
            }
            return null;
        }

        public static void Recycle(TimerAction t)
        {
            if (t.Id != 0)
            {
                t.Id = 0;
                queue.Enqueue(t);
            }
        }
    }


}
