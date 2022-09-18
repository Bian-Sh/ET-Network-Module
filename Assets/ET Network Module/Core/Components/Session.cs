using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace ET
{
    public readonly struct RpcInfo
    {
        public readonly IRequest Request;
        public readonly ETTask<IResponse> Tcs;
        public RpcInfo(IRequest request)
        {
            this.Request = request;
            this.Tcs = ETTask<IResponse>.Create(true);
        }
    }


    public sealed class Session
    {
        public bool IsDisposed => this.Id == 0;
        public int AcceptTimeout = 5000;
        public int IdleChecker = 2000;
        public AService AService;
        public Ping ping;
        public readonly Dictionary<int, RpcInfo> requestCallbacks = new Dictionary<int, RpcInfo>();
        public long Id { get; set; }
        public static int RpcId { get; set; }
        public long LastRecvTime { get; set; }
        public long LastSendTime { get; set; }
        public int Error { get; set; }
        public IPEndPoint RemoteAddress { get; set; }

        public Session(long id, AService aService)
        {
            this.Id = id;
            AService = aService;
            long timeNow = TimeHelper.ClientNow();
            LastRecvTime = timeNow;
            LastSendTime = timeNow;
            requestCallbacks.Clear();
            Debug.Log($"session create, id: {Id} {timeNow} ");
        }

        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            AService.RemoveChannel(Id);
            ping?.Dispose();
            ping = null;
            var temp = Id;
            Id = 0;
            foreach (RpcInfo responseCallback in requestCallbacks.Values.ToArray())
            {
                responseCallback.Tcs.SetException(new RpcException(Error, $"session dispose: {temp} - {RemoteAddress} -  rpcid={responseCallback.Request.RpcId}"));
            }
            Debug.Log($"session dispose: {RemoteAddress} id: {temp} ErrorCode: {Error}, please see ErrorCode.cs! {TimeHelper.ClientNow()}");
            requestCallbacks.Clear();
        }
        public void OnRead(ushort opcode, IResponse response)
        {
            OpcodeHelper.LogMsg(0, opcode, response);
            if (!requestCallbacks.TryGetValue(response.RpcId, out var action))
            {
                return;
            }
            requestCallbacks.Remove(response.RpcId);
            if (ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                action.Tcs.SetException(new Exception($"Rpc error, request: {action.Request} response: {response}"));
                return;
            }
            action.Tcs.SetResult(response);
        }

        public async ETTask<IResponse> Call(IRequest request, ETCancellationToken cancellationToken)
        {
            int rpcId = ++RpcId;
            RpcInfo rpcInfo = new RpcInfo(request);
            requestCallbacks[rpcId] = rpcInfo;
            request.RpcId = rpcId;
            Send(request);
            void CancelAction()
            {
                if (!requestCallbacks.TryGetValue(rpcId, out RpcInfo action))
                {
                    return;
                }
                requestCallbacks.Remove(rpcId);
                Type responseType = OpcodeTypeManager.GetResponseType(action.Request.GetType());
                IResponse response = (IResponse)Activator.CreateInstance(responseType);
                response.Error = ErrorCore.ERR_Cancel;
                action.Tcs.SetResult(response);
            }

            IResponse ret;
            try
            {
                cancellationToken?.Add(CancelAction);
                ret = await rpcInfo.Tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
            return ret;
        }

        public async ETTask<IResponse> Call(IRequest request)
        {
            int rpcId = ++RpcId;
            RpcInfo rpcInfo = new RpcInfo(request);
            requestCallbacks[rpcId] = rpcInfo;
            request.RpcId = rpcId;
            Send(request);
            return await rpcInfo.Tcs;
        }
        public void Reply(IResponse message) => Send(0, message);
        public void Send(IMessage message) => Send(0, message);
        public void Send(long actorId, IMessage message)
        {
            (ushort opcode, MemoryStream stream) = MessageSerializeHelper.MessageToStream(message);
            OpcodeHelper.LogMsg(0, opcode, message);
            Send(actorId, stream);
        }

        public void Send(long actorId, MemoryStream memoryStream)
        {
            LastSendTime = TimeHelper.ClientNow();
            AService.SendStream(Id, actorId, memoryStream);
        }
    }
}