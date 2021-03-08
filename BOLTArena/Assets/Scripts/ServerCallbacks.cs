using System.Collections;
using System.Collections.Generic;
using Bolt;
using UdpKit;
using UnityEngine;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Stage")]
public class ServerCallbacks : Bolt.GlobalEventListener {
    public static bool ListenServer = true;

	public override bool PersistBetweenStartupAndShutdown()
	{
		return base.PersistBetweenStartupAndShutdown();
	}

	void Awake() {
		if (ListenServer) {
			Player.CreateServerPlayer();
			Player.ServerPlayer.name = "SERVER";
		}
	}

	void FixedUpdate() {
		// foreach (Player p in Player.allPlayers) {
		// 	// if we have an entity, it's dead but our spawn frame has passed
		// 	if (p.entity && p.state.Dead && p.state.respawnFrame <= BoltNetwork.ServerFrame) {
		// 		p.Spawn();
		// 	}
		// }
	}

	public override void ConnectRequest(UdpKit.UdpEndPoint endpoint, Bolt.IProtocolToken token) {
		
		if (token != null) {
			BoltLog.Info("Token Received");
			var recv_token = (PlayerData) token;
			BoltLog.Info("111111111111111111ConnectRequest - "+recv_token.m_playerName+ " -- "+recv_token.m_resKey);
		}

		BoltNetwork.Accept(endpoint);
	}

	public override void ConnectAttempt(UdpEndPoint endpoint, IProtocolToken token) {
		BoltLog.Warn("22222222222222222222ConnectAttempt");
		base.ConnectAttempt(endpoint, token);
	}

	public override void Disconnected(BoltConnection connection) {
		BoltLog.Warn("Disconnected");
		base.Disconnected(connection);
	}

	public override void ConnectRefused(UdpEndPoint endpoint, IProtocolToken token) {
		BoltLog.Warn("ConnectRefused");
		base.ConnectRefused(endpoint, token);
	}

	public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token) {
		BoltLog.Warn("ConnectFailed");
		base.ConnectFailed(endpoint, token);
	}

	public override void Connected(BoltConnection connection) {
		BoltLog.Warn("Player Connected");

		var playerToken = connection.ConnectToken as PlayerData;
		string player_name = "CLIENT:" + connection.RemoteEndPoint.Port;
		
		if (playerToken != null) {
			player_name = "CLIENT:" + playerToken.m_playerName;
		}
		connection.UserData = new Player();
		connection.GetPlayer().connection = connection;
		connection.GetPlayer().name = player_name;

		connection.SetStreamBandwidth(1024 * 1024);
	}

	public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token) {
		//remote player의 scene loading이 끝났을 때
		if (token != null) {
			var data = token as PlayerData;
			connection.GetPlayer().InstantiateEntity(data);
		}
		else {
			Debug.LogError("SceneLoadRemoteDone****Token is NULL****");
		}
	}

	public override void SceneLoadLocalDone(string scene, IProtocolToken token) {
		Debug.Log("ServerManager :: Server scene [" + scene + "] load is done");
		if (Player.serverIsPlaying) {
			var data = new PlayerData() {m_playerName = "Server", m_resKey = "Enemy"};
			Player.ServerPlayer.InstantiateEntity(data);
		}
	}

	public override void SceneLoadLocalBegin(string scene, IProtocolToken token) {
		foreach (Player p in Player.GetPlayerList) {
			p.entity = null;
		}
	}
}
