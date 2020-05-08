// ----------------------------------------------------------------------------
// <copyright file="PhotonHandler.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// PhotonHandler is a runtime MonoBehaviour to include PUN into the main loop.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using UnityEngine;

    #if UNITY_5_5_OR_NEWER
    using UnityEngine.Profiling;
    #endif


    /// <summary>
    /// Internal MonoBehaviour that allows Photon to run an Update loop.
    /// </summary>
    public class PhotonHandler : ConnectionHandler, IInRoomCallbacks, IMatchmakingCallbacks
    {

        private static PhotonHandler instance;
        internal static PhotonHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PhotonHandler>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = "PhotonMono";
                        instance = obj.AddComponent<PhotonHandler>();
                    }
                }

                return instance;
            }
        }


        /// <summary>Limits the number of datagrams that are created in each LateUpdate.</summary>
        /// <remarks>Helps spreading out sending of messages minimally.</remarks>
        public static int MaxDatagrams = 10;

        /// <summary>Signals that outgoing messages should be sent in the next LateUpdate call.</summary>
        /// <remarks>Up to MaxDatagrams are created to send queued messages.</remarks>
        public static bool SendAsap;

        /// <summary>This corrects the "next time to serialize the state" value by some ms.</summary>
        /// <remarks>As LateUpdate typically gets called every 15ms it's better to be early(er) than late to achieve a SerializeRate.</remarks>
        private const int SerializeRateFrameCorrection = 8;

        protected internal int UpdateInterval; // time [ms] between consecutive SendOutgoingCommands calls

        protected internal int UpdateIntervalOnSerialize; // time [ms] between consecutive RunViewUpdate calls (sending syncs, etc)

        private int nextSendTickCount;

        private int nextSendTickCountOnSerialize;

        private SupportLogger supportLoggerComponent;


        protected override void Awake()
        {
            if (instance == null || ReferenceEquals(this, instance))
            {
                instance = this;
                base.Awake();
            }
            else
            {
                Destroy(this);
            }
        }

        protected virtual void OnEnable()
        {
            if (Instance != this)
            {
                Debug.LogError("PhotonHandler is a singleton but there are multiple instances. this != Instance.");
                return;
            }

            this.Client = PhotonNetwork.NetworkingClient;

            if (PhotonNetwork.PhotonServerSettings.EnableSupportLogger)
            {
                SupportLogger supportLogger = this.gameObject.GetComponent<SupportLogger>();
                if (supportLogger == null)
                {
                    supportLogger = this.gameObject.AddComponent<SupportLogger>();
                }
                if (this.supportLoggerComponent != null)
                {
                    if (supportLogger.GetInstanceID() != this.supportLoggerComponent.GetInstanceID())
                    {
                        Debug.LogWarningFormat("Cached SupportLogger component is different from the one attached to PhotonMono GameObject");
                    }
                }
                this.supportLoggerComponent = supportLogger;
                this.supportLoggerComponent.Client = PhotonNetwork.NetworkingClient;
            }

            this.UpdateInterval = 1000 / PhotonNetwork.SendRate;
            this.UpdateIntervalOnSerialize = 1000 / PhotonNetwork.SerializationRate;

            PhotonNetwork.AddCallbackTarget(this);
            this.StartFallbackSendAckThread();  // this is not done in the base class
        }

        protected void Start()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
            {
                PhotonNetwork.NewSceneLoaded();
            };
        }

        protected override void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            base.OnDisable();
        }


        /// <summary>Called in intervals by UnityEngine. Affected by Time.timeScale.</summary>
        protected void FixedUpdate()
        {
            this.Dispatch();
        }

        /// <summary>Called in intervals by UnityEngine, after running the normal game code and physics.</summary>
        protected void LateUpdate()
        {
            // see MinimalTimeScaleToDispatchInFixedUpdate and FixedUpdate for explanation:
            if (Time.timeScale <= PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate)
            {
                this.Dispatch();
            }


            int currentMsSinceStart = (int)(Time.realtimeSinceStartup * 1000); // avoiding Environment.TickCount, which could be negative on long-running platforms
            if (PhotonNetwork.IsMessageQueueRunning && currentMsSinceStart > this.nextSendTickCountOnSerialize)
            {
                PhotonNetwork.RunViewUpdate();
                this.nextSendTickCountOnSerialize = currentMsSinceStart + this.UpdateIntervalOnSerialize - SerializeRateFrameCorrection;
                this.nextSendTickCount = 0; // immediately send when synchronization code was running
            }

            currentMsSinceStart = (int)(Time.realtimeSinceStartup * 1000);
            if (SendAsap || currentMsSinceStart > this.nextSendTickCount)
            {
                SendAsap = false;
                bool doSend = true;
                int sendCounter = 0;
                while (PhotonNetwork.IsMessageQueueRunning && doSend && sendCounter < MaxDatagrams)
                {
                    // Send all outgoing commands
                    Profiler.BeginSample("SendOutgoingCommands");
                    doSend = PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
                    sendCounter++;
                    Profiler.EndSample();
                }

                this.nextSendTickCount = currentMsSinceStart + this.UpdateInterval;
            }
        }

        /// <summary>Dispatches incoming network messages for PUN. Called in FixedUpdate or LateUpdate.</summary>
        /// <remarks>
        /// It may make sense to dispatch incoming messages, even if the timeScale is near 0.
        /// That can be configured with PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate.
        ///
        /// Without dispatching messages, PUN won't change state and does not handle updates.
        /// </remarks>
        protected void Dispatch()
        {
            if (PhotonNetwork.NetworkingClient == null)
            {
                Debug.LogError("NetworkPeer broke!");
                return;
            }

            //if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated || PhotonNetwork.NetworkClientState == ClientState.Disconnected || PhotonNetwork.OfflineMode)
            //{
            //    return;
            //}


            bool doDispatch = true;
            while (PhotonNetwork.IsMessageQueueRunning && doDispatch)
            {
                // DispatchIncomingCommands() returns true of it found any command to dispatch (event, result or state change)
                Profiler.BeginSample("DispatchIncomingCommands");
                doDispatch = PhotonNetwork.NetworkingClient.LoadBalancingPeer.DispatchIncomingCommands();
                Profiler.EndSample();
            }
        }


        public void OnCreatedRoom()
        {
            PhotonNetwork.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName);
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            PhotonNetwork.LoadLevelIfSynced();
        }

        public void OnJoinedRoom(){}

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps){}

        public void OnMasterClientSwitched(Player newMasterClient){}

        public void OnFriendListUpdate(System.Collections.Generic.List<FriendInfo> friendList){}

        public void OnCreateRoomFailed(short returnCode, string message){}

        public void OnJoinRoomFailed(short returnCode, string message){}

        public void OnJoinRandomFailed(short returnCode, string message){}

        public void OnLeftRoom(){}

        public void OnPlayerEnteredRoom(Player newPlayer){}

        public void OnPlayerLeftRoom(Player otherPlayer){}
    }
}