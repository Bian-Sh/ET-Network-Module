using System;
using UnityEngine;

namespace ET
{
    public class Ping
    {
        public C2G_Ping C2G_Ping = new C2G_Ping();
        public long Id { get; set; }
        public long InstanceId { get; private set; }
        public static TimeInfo TimeInfo => TimeInfo.Instance;

        public bool IsDisposed => InstanceId == 0;

        public long delay; //延迟值
        Session session;
        public Ping(Session session)
        {
            InstanceId = IdGenerater.Instance.GenerateInstanceId();
            this.session = session;
            _ = PingAsync();
        }

        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            InstanceId = 0;
        }

        private async ETTask PingAsync()
        {
            long instanceId = InstanceId;
            while (true)
            {
                if (InstanceId != instanceId)
                {
                    return;
                }
                long time1 = TimeHelper.ClientNow();
                try
                {
                    G2C_Ping response = await session.Call(C2G_Ping) as G2C_Ping;
                    if (InstanceId != instanceId)
                    {
                        return;
                    }
                    long time2 = TimeHelper.ClientNow();
                    delay = time2 - time1;
                    TimeInfo.ServerMinusClientTime = response.Time + (time2 - time1) / 2 - time2;
                    Debug.Log($"{nameof(Ping)}:  ping = {delay} - {response.Time} - {response.Message} - {TimeInfo.ServerFrameTime()}");
                    await TimerComponent.Instance.WaitAsync(2000);
                }
                catch (RpcException e)
                {
                    // session断开导致ping rpc报错，记录一下即可，不需要打成error
                    Debug.Log($"ping error: {Id} {e.Error}");
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError($"ping error: \n{e}");
                }
            }
        }
    }
}