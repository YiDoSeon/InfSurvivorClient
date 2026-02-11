using System;
using System.Collections.Generic;
using System.Net;
using InfSurvivor.Runtime.Network;
using Shared.Packet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InfSurvivor.Runtime.Manager
{
    using Stopwatch = System.Diagnostics.Stopwatch;
    public class NetworkManager
    {
        private ServerSession serverSession = new ServerSession();
        private float lastTimeRequestTimeSync;
        public bool IsSessionValidated = false;
        public event Action onConnectSuccess;
        public event Action<string> onConnectFailed;
        public Queue<Action> ExecuteQueue = new Queue<Action>();
        public void Init()
        {
            Application.quitting += OnQuitting;
        }

        public void ConnectToGame(string ip, int port)
        {
            IPAddress ipAddr = IPAddress.Parse(ip);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, port);
            Connector connector = new Connector();
            connector.Connect(endPoint, () => serverSession);
        }

        public void OnConnectFailed(string reason)
        {
            onConnectFailed?.Invoke(reason);
        }

        public void OnDestroy()
        {
            onConnectSuccess = null;
            onConnectFailed = null;
            Close();
        }

        private void OnQuitting()
        {
            Application.quitting -= OnQuitting;
            Close();
        }

        private void Close()
        {
            serverSession?.Close();
            serverSession = null;
        }

        /// <summary>
        /// 처음 서버와 소켓 연결한뒤, 서버로부터 연결되었다는 패킷을 받았을 때 호출 <br/>
        /// </summary>
        public void OnSuccessReceiveConnectPacket()
        {
            IsSessionValidated = true;
            RequestTimeSync();
        }

        public void Update()
        {
            ExecuteEvents();
            ProcessPackets();
            if (Time.realtimeSinceStartup > lastTimeRequestTimeSync + HEART_BEAT_SECONDS)
            {
                RequestTimeSync();
            }

            offset = Mathf.Lerp(offset, targetOffset, 0.05f);
            
            if (IsSessionValidated && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                C_EnterGame enterGamePacket = new C_EnterGame();
                enterGamePacket.Name = "플레이어";
                Send(enterGamePacket);
            }
        }

        private void ProcessPackets()
        {
            List<PacketMessage> packets = PacketQueue.Instance.PopAll();
            foreach (PacketMessage packet in packets)
            {
                PacketManager.PacketProcessor processor =
                    PacketManager.Instance.GetPacketProcessor(packet.Id);
                processor?.Invoke(serverSession, packet.Packet);
            }
        }

        public void Send<TPacket>(TPacket packet) where TPacket : IPacket
        {
            serverSession.Send(packet);
        }

        public void PushEvent(Action action)
        {
            lock (ExecuteQueue)
            {
                ExecuteQueue.Enqueue(action);
            }
        }
        
        public void ExecuteEvents()
        {
            lock (ExecuteQueue)
            {
                while (ExecuteQueue.Count > 0)
                {
                    ExecuteQueue.Dequeue()?.Invoke();
                }
            }
        }

        #region 시간 관련
        private const float HEART_BEAT_SECONDS = 5f;
        private Stopwatch clock = Stopwatch.StartNew();
        private float offset;
        private float targetOffset;
        private float displayPing;
        private long lastRTT;

        private bool isTimeSyncWarmupDone;        
        private int warmupCount;
        private bool isSynced = false;

        public void RequestTimeSync()
        {
            if (IsSessionValidated)
            {
                lastTimeRequestTimeSync = Time.realtimeSinceStartup;
                C_TimeSync timeSyncPacket = new C_TimeSync
                {
                    ClientTime = GetLocalTick()
                };
                Send(timeSyncPacket);
            }
        }
        
        public void HandleSyncTime(S_TimeSync packet)
        {
            if (isTimeSyncWarmupDone == false)
            {
                warmupCount++;
                // Warmup을 위해 첫 번째 시간 동기화는 버리고 3번정도 바로 다시 시간 동기화 즉시 요청
                RequestTimeSync();
                if (warmupCount < 3)
                {
                    return;
                }
                isTimeSyncWarmupDone = true;
                return;
            }

            if (isSynced == false)
            {
                onConnectSuccess?.Invoke();
                //Debug.Log("시간 동기화 완료.");
            }

            long t1 = packet.ClientTime; // 패킷을 보낸 시간
            long ts = packet.ServerTime; // 서버 현재 시간 2000
            long t2 = GetLocalTick(); // 로컬 시간
            lastRTT = t2 - t1;

            float rawPing = lastRTT / 1000f;
            displayPing = displayPing <= 0
                ? rawPing
                : Mathf.Lerp(displayPing, rawPing, 0.15f); // 기존 측정값을 85%신뢰한다.


            targetOffset = (ts - t2) / 1000f + rawPing * 0.5f;
            if (offset == 0)
            {
                offset = targetOffset;
            }
            //Debug.Log($"RTT: {rawPing * 1000f}ms, Offset: {offset}, TargetOffset: {targetOffset}, Ping: {displayPing * 1000}ms");

            isSynced = true;
            // 검증
            long serverTime = GetServerTime();
            long clientTime = t2;
            long gap = serverTime - clientTime;

            //Debug.Log($"현재 서버시간: ({GetServerTime()}) 현재 클라시간: ({t2}) 차이1:({gap/1000f}), 차이2:({offset})");
        }
        public long GetRTT() => lastRTT;
        public float GetDisplayPing() => displayPing;
        public long GetServerTime()
        {
            if (isSynced == false)
            {
                return 0;
            }
            return GetLocalTick() + (long)(offset * 1000);
        }
        public long GetLocalTick()
        {
            return clock.ElapsedMilliseconds;
        }
        #endregion
    }
}
