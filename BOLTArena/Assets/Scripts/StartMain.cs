using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bolt;
using Bolt.Matchmaking;
using Bolt.Photon;
using TMPro;
using UdpKit;
using UdpKit.Platform.Photon;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class StartMain : Bolt.GlobalEventListener {
	public static bool IsInit = false;
	private enum State {
			Initializing,
			InputName,
			SelectMode,
			SelectRoom,
			StartServer,
			StartClient,
			Started,
	}

	private Rect labelRoom = new Rect(0, 0, 140, 75);
	private GUIStyle labelRoomStyle;
	private State currentState = State.Initializing;

	public GameObject nameInputObj;
	public TMP_InputField nameInputField;
	
	void Awake() {
		ScreenInfo.SetScreenSize();
		Screen.SetResolution(ScreenInfo.referenceWidth, ScreenInfo.referenceHeight, true);
		Application.targetFrameRate = 60;

		labelRoomStyle = new GUIStyle() {
			fontSize = 25,
			fontStyle = FontStyle.Bold,
			normal = {
				textColor = Color.white
			}
		};
		
		IsInit = true;
	}

	private IEnumerator Start() {
		yield return StartCoroutine(NetworkInfo.GetIPAddress());
		currentState = State.InputName;
		nameInputObj.SetActive(true);
	}

	void OnGUI() {
		Rect tex = new Rect(10, 10, 140, 75);
		Rect area = new Rect(10, 90, Screen.width - 20, Screen.height - 100);

		GUI.Box(tex, Resources.Load("BoltLogo") as Texture2D);

		if (BoltNetwork.IsRunning) {
			tex = new Rect(160, 10, 140, 75);
			GUILayout.BeginArea(tex);
			if (GUILayout.Button("Shutdown", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
				BoltNetwork.Shutdown();
			}
			GUILayout.EndArea();
		}

		GUILayout.BeginArea(area);

		switch (currentState) {
			case State.SelectMode: State_SelectMode(); break;
			case State.SelectRoom: State_SelectRoom(); break;
			case State.StartClient: State_StartClient(); break;
			case State.StartServer: State_StartServer(); break;
		}

		GUILayout.EndArea();
	}

	public void EnterPlayerName() {
		string data = nameInputField.text;
		if (String.IsNullOrEmpty(data) == false) {
			NetworkInfo.SetHostName(data);
		}
		else {
			data = "DEFAULT_PLAYER_" + UnityEngine.Random.Range(1000, 9999);
			NetworkInfo.SetHostName(data);
		}
		currentState = State.SelectMode;
		nameInputObj.SetActive(false);
	}
	
	void State_SelectRoom() {
		GUI.Label(labelRoom, "Looking for rooms:", labelRoomStyle);

		if (BoltNetwork.SessionList.Count > 0) {
			GUILayout.BeginVertical();
			GUILayout.Space(30);

			foreach (var session in BoltNetwork.SessionList) {
				var photonSession = session.Value as PhotonSession;

				if (photonSession.Source == UdpSessionSource.Photon) {
					var matchName = photonSession.HostName;
					var label = string.Format("Join: {0} | {1}/{2}", matchName, photonSession.ConnectionsCurrent, photonSession.ConnectionsMax);

					if (ExpandButton(label)) {
						int idx = UnityEngine.Random.Range(0, NetworkInfo.nameSkinDict.Count);
						var data = NetworkInfo.nameSkinDict.ToList()[idx];
						BoltMatchmaking.JoinSession(photonSession, new PlayerData() { m_playerName = NetworkInfo.HostName, m_resKey = data.Value });
						currentState = State.Started;
					}
				}
			}

			GUILayout.EndVertical();
		}
	}

	public static string GetRandomResKey() {
		int idx = UnityEngine.Random.Range(0, NetworkInfo.nameSkinDict.Count);
		return NetworkInfo.nameSkinDict.ToList()[idx].Value;
	}
	
	bool ExpandButton(string text) {
		return GUILayout.Button(text, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
	}
	
	void State_SelectMode() {
		if (ExpandButton("Server")) {
			State_StartServer();
		}
		if (ExpandButton("Client")) {
			currentState = State.StartClient;
		}
	}
	
	void State_StartServer() {
		BoltLauncher.StartServer();
		currentState = State.Started;
	}

	void State_StartClient() {
		BoltLauncher.StartClient();
		currentState = State.SelectRoom;
	}

	public override void BoltStartBegin() {
		// Register any Protocol Token that are you using
		BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
	}

	public override void BoltStartDone() {
		if (BoltNetwork.IsServer)
		{
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
			currentState = State.SelectMode;
		});
	}

	public override void SessionListUpdated(Map<Guid, UdpSession> sessionList) {
		BoltLog.Info("New session list");
	}
}
