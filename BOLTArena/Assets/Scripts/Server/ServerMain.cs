using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bolt;
using Bolt.Matchmaking;
using Bolt.Photon;
using UdpKit;
using UnityEngine;

public class ServerMain : Bolt.GlobalEventListener {
	public static bool HeadlessMode = false;

	void Awake() {
		string[] args = System.Environment.GetCommandLineArgs ();
		string input = "";
		for (int i = 0; i < args.Length; i++) {
			Debug.Log ("ARG " + i + ": " + args [i]);
			if (args [i] == "-port") {
				input = args [i + 1];
				var port = int.Parse(input);
				Debug.Log("Set Port : "+NetworkInfo.port);
				NetworkInfo.SetPort(port);
			}
			else if (args[i] == "-server_name") {
				input = args[i + 1];
				var hostName = input;
				Debug.Log("Host Name : "+hostName);
				NetworkInfo.SetHostName(hostName);
			}
		}
		
		Application.targetFrameRate = 60; 
	}

	IEnumerator Start() {
		yield return StartCoroutine(NetworkInfo.GetIPAddress());
		HeadlessMode = true;
		State_StartServer();
	}

	void State_StartServer() {
		BoltLauncher.StartServer(NetworkInfo.port);
	}

	public override void BoltStartBegin() {
		// Register any Protocol Token that are you using
		BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
	}

	public override void BoltStartDone() {
		if (BoltNetwork.IsServer) {
			var id = Guid.NewGuid().ToString().Split('-')[0];
			var matchName = NetworkInfo.GetDefaultMatchName();

			BoltMatchmaking.CreateSession(
				sessionID: matchName,
				sceneToLoad: NetworkInfo.sceneName
			);
		}
	}

	public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason) {
		registerDoneCallback(() => {
		});
	}

	public override void SessionListUpdated(Map<Guid, UdpSession> sessionList) {
		BoltLog.Info("New session list");
	}
}
