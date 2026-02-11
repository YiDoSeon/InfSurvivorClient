using System;
using System.Net;
using System.Net.Sockets;
using InfSurvivor.Runtime.Manager;
using Shared.Session;
using UnityEngine;

namespace InfSurvivor.Runtime.Network
{
    public class Connector
    {
        private Func<SessionBase> sessionFactory;

        public void Connect(IPEndPoint endPoint, Func<SessionBase> sessionFactory)
        {
            Socket socket = new Socket(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            this.sessionFactory = sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);
        }

        private void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
            {
                // TODO: 초기화 실패
                return;
            }

            try
            {
                bool pending = socket.ConnectAsync(args);
                if (pending == false)
                {
                    OnConnectCompleted(null, args);
                }
            }
            catch (System.Exception e)
            {
                Managers.Network.PushEvent(() =>
                {
                    Managers.Network.OnConnectFailed($"{nameof(RegisterConnect)} Failed: {e}");
                });
            }
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                if (args.SocketError == SocketError.Success)
                {
                    SessionBase session = sessionFactory?.Invoke();
                    session.Start(args.ConnectSocket);
                    session.OnConnected(args.RemoteEndPoint);
                }
                else
                {
                    Managers.Network.PushEvent(() =>
                    {
                        Managers.Network.OnConnectFailed($"{nameof(OnConnectCompleted)} Failed: {args.SocketError}");
                    });
                }
            }
            catch (System.Exception e)
            {
                Managers.Network.PushEvent(() =>
                {
                    Managers.Network.OnConnectFailed($"{nameof(OnConnectCompleted)} Failed: {e}");
                });
            }
        }
    }
}
