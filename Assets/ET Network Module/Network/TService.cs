using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using static zFramework.Misc.Loom;
namespace ET
{
    public sealed class TService : AService
    {
        private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();
        private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
        public HashSet<long> NeedStartSend = new HashSet<long>();
        private Socket acceptor;
        private bool m_DisposeCalled;
        public TService(ServiceType serviceType)
        {
            this.ServiceType = serviceType;
        }

        public TService(IPEndPoint ipEndPoint, ServiceType serviceType)
        {
            this.ServiceType = serviceType;
            this.acceptor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.innArgs.Completed += this.OnComplete;
            this.acceptor.Bind(ipEndPoint);
            this.acceptor.Listen(1000);
            PostNext(this.AcceptAsync);
        }

        private void OnComplete(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    SocketError socketError = e.SocketError;
                    Socket acceptSocket = e.AcceptSocket;
                    Post(() => { this.OnAcceptComplete(socketError, acceptSocket); });
                    break;
                default:
                    throw new Exception($"socket error: {e.LastOperation}");
            }
        }

        #region 网络线程

        private void OnAcceptComplete(SocketError socketError, Socket acceptSocket)
        {
            if (this.acceptor == null) return;
            if (socketError != SocketError.Success)
            {
                Debug.LogError($"accept error {socketError}");
                return;
            }
            try
            {
                long id = this.CreateAcceptChannelId(0);
                TChannel channel = new TChannel(id, acceptSocket, this);
                this.idChannels.Add(channel.Id, channel);
                long channelId = channel.Id;
                this.OnAccept(channelId, channel.RemoteAddress);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
            this.AcceptAsync();// 开始新的accept
        }
        private void AcceptAsync()
        {
            this.innArgs.AcceptSocket = null;
            if (this.acceptor.AcceptAsync(this.innArgs))
            {
                return;
            }
            OnAcceptComplete(this.innArgs.SocketError, this.innArgs.AcceptSocket);
        }

        private TChannel Create(IPEndPoint ipEndPoint, long id)
        {
            TChannel channel = new TChannel(id, ipEndPoint, this);
            this.idChannels.Add(channel.Id, channel);
            return channel;
        }

        protected override void Get(long id, IPEndPoint address)
        {
            if (this.idChannels.TryGetValue(id, out TChannel _))
            {
                return;
            }
            this.Create(address, id);
        }

        private TChannel Get(long id)
        {
            this.idChannels.TryGetValue(id, out var channel);
            return channel;
        }

        public override void Dispose()
        {
            m_DisposeCalled=true;
            this.acceptor?.Close();
            this.acceptor = null;
            this.innArgs.Dispose();
            foreach (long id in this.idChannels.Keys.ToArray())
            {
                TChannel channel = this.idChannels[id];
                channel.Dispose();
            }
            this.idChannels.Clear();
        }

        public override void Remove(long id)
        {
            if (this.idChannels.TryGetValue(id, out TChannel channel))
            {
                channel.Dispose();
            }
            this.idChannels.Remove(id);
        }

        protected override void Send(long channelId, long actorId, MemoryStream stream)
        {
            try
            {
                TChannel aChannel = this.Get(channelId);
                if (aChannel == null)
                {
                    this.OnError(channelId, ErrorCore.ERR_SendMessageNotFoundTChannel);
                    return;
                }
                aChannel.Send(actorId, stream);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public override void Update()
        {
            foreach (var channelId in NeedStartSend)
            {
                TChannel tChannel = this.Get(channelId);
                tChannel?.Update();
            }
            this.NeedStartSend.Clear();
        }
        public override bool IsDispose() =>m_DisposeCalled;
        #endregion
    }
}