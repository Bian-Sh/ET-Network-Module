using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class TimerManager
    {
        public const int TimeTypeMax = 10000;
        public static long timeNow;
        // 记录最小时间，不用每次都去MultiMap取第一个值
        public static long minTime;

        /// <summary>
        /// key: time, value: timer id
        /// </summary>
        public static readonly MultiMap<long, long> TimeId = new MultiMap<long, long>();
        public static readonly Queue<long> timeOutTime = new Queue<long>();
        public static readonly Queue<long> timeOutTimerIds = new Queue<long>();
        public static readonly Queue<long> everyFrameTimer = new Queue<long>();

        public static ITimer[] timerActions;
        static Dictionary<long, TimerAction> timers = new Dictionary<long, TimerAction>();

        public static void Init()
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
        public static void Update()
        {
            #region 每帧执行的timer，不用foreach TimeId，减少GC

            int count = everyFrameTimer.Count;
            for (int i = 0; i < count; ++i)
            {
                long timerId = everyFrameTimer.Dequeue();
                if (timers.TryGetValue(timerId, out var action))
                {
                    Run(action);
                }
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
                if (timers.TryGetValue(timerId, out var action))
                {
                    Run(action);
                }
            }
        }

        public static long NewRepeatedTimer(long time, int type, object args)
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
        private static long NewRepeatedTimerInner(long time, int type, object args)
        {
            long tillTime = TimeHelper.ServerNow() + time;
            var timer = Allocate(TimerClass.RepeatedTimer, time, type, args);
            // 每帧执行的不用加到timerId中，防止遍历
            Add(tillTime, timer);
            return timer.Id;
        }
        private static void Run(TimerAction timerAction)
        {
            if (timerAction == null) return;
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
                        Add(tillTime, timerAction);

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
        private static void Add(long tillTime, TimerAction timer)
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
        public static bool Remove(ref long id)
        {
            long i = id;
            id = 0;
            return Remove(i);
        }
        private static bool Remove(long id)
        {
            if (timers.TryGetValue(id, out var action))
            {
                if (null != action)
                {
                    action.Reset();
                    Pool.Recycle(action);
                    return true;
                }
            }
            return false;
        }
        public static long NewFrameTimer(int type, object args) => NewRepeatedTimerInner(0, type, args);
        public static async ETTask<bool> WaitAsync(long time, ETCancellationToken cancellationToken = null)
        {
            if (time == 0) return true;
            long tillTime = TimeHelper.ServerNow() + time;
            ETTask<bool> tcs = ETTask<bool>.Create(true);
            TimerAction timer = Allocate(TimerClass.OnceWaitTimer, time, 0, tcs);
            Add(tillTime, timer);
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
        public static async ETTask<bool> WaitFrameAsync(ETCancellationToken cancellationToken = null) => await WaitAsync(1, cancellationToken);
        public static async ETTask<bool> WaitUntilAsync(long tillTime, ETCancellationToken cancellationToken = null)
        {
            if (timeNow >= tillTime) return true;
            ETTask<bool> tcs = ETTask<bool>.Create(true);
            TimerAction timer = Allocate(TimerClass.OnceWaitTimer, tillTime - timeNow, 0, tcs);
            Add(tillTime, timer);
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
        private static TimerAction Allocate(TimerClass onceWaitTimer, long value, int v1, object tcs)
        {
            var timer = Pool.Get();
            timer ??= new TimerAction();
            timer.Init(IdGenerater.Instance.GenerateId(), onceWaitTimer, value, v1, tcs);
            timers.Add(timer.Id, timer);
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
                if (t != null)
                {
                    t.Reset();
                    queue.Enqueue(t);
                }
            }
        }
    }
}
