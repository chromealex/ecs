// ----------------------------------------------------------------------------
// <copyright file="PhotonPingClasses.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Provides implementations of the PhotonPing for various platforms and
//   use cases.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
#define SUPPORTED_UNITY
#endif

#if SUPPORTED_UNITY
namespace Photon.Realtime
{
    using System;
    using System.Net.Sockets;
    using ExitGames.Client.Photon;

    #if UNITY_WEBGL
    // import WWW class
    using UnityEngine;
    #endif


    #if UNITY_WEBGL
    public class PingHttp : PhotonPing
    {
        private WWW webRequest;

        public override bool StartPing(string address)
        {
            base.Init();

            address = "https://" + address + "/photon/m/?ping&r=" + UnityEngine.Random.Range(0, 10000);
            this.webRequest = new WWW(address);
            return true;
        }

        public override bool Done()
        {
            if (this.webRequest.isDone)
            {
                Successful = true;
                return true;
            }

            return false;
        }

        public override void Dispose()
        {
            this.webRequest.Dispose();
        }
    }
    #endif
}
#endif