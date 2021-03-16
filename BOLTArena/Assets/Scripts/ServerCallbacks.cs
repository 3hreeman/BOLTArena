using System;
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
			CombatManager.GenerateNpc(1);
		}
	}

	private const float GEN_INTERVAL = 1;
	private float nextGenerateAt = 0;
	private void FixedUpdate() {
		if (nextGenerateAt <= BoltNetwork.ServerTime) {
			nextGenerateAt = BoltNetwork.ServerTime + GEN_INTERVAL;
			CombatManager.GenerateNpc(1);
		}
		CheckGameEnd();
	}

	void CheckGameEnd() {
		if (NetworkInfo.IsExpired()) {
			StageMain.StageEnd();
		}
	}

	public override void ConnectRequest(UdpKit.UdpEndPoint endpoint, Bolt.IProtocolToken token) {
		
		if (token != null) {
			BoltLog.Info("Token Received");
			var recv_token = (PlayerData) token;
			// BoltLog.Info("111111111111111111ConnectRequest - "+recv_token.m_playerName+ " -- "+recv_token.m_resKey);
		}

		BoltNetwork.Accept(endpoint);
	}

	public override void ConnectAttempt(UdpEndPoint endpoint, IProtocolToken token) {
		base.ConnectAttempt(endpoint, token);
	}

	public override void Disconnected(BoltConnection connection) {
		BoltLog.Warn("Disconnected");
		var player = connection.GetPlayer();
		if (player != null) {
			CombatManager.RemovePlayerObject(player.playerObject);
		}
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

		var playerToken = connection.ConnectToken as PlayerData;
		string player_name = "CLIENT:" + connection.RemoteEndPoint.Port;
		
		if (playerToken != null) {
			player_name = "CLIENT:" + playerToken.m_playerName;
		}
		connection.UserData = new Player(playerToken);
		connection.GetPlayer().connection = connection;
		connection.GetPlayer().name = player_name;
		BoltLog.Warn("Player Connected : "+player_name);

		connection.SetStreamBandwidth(1024 * 1024);
	}
	
	

	public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token) {
		//remote player의 scene loading이 끝났을 때
		if (token != null) {
			var data = token as PlayerData;
			var player = connection.GetPlayer();
			player.playerData = data;
			CombatManager.GeneratePlayerObject(player, false);
		}
		else {
			Debug.LogError("SceneLoadRemoteDone****Token is NULL****");
		}
	}

	public override void SceneLoadLocalDone(string scene, IProtocolToken token) {
		// Debug.Log("ServerManager :: Server scene [" + scene + "] load is done");
		if (Player.serverIsPlaying) {
			CameraManager.SetTarget(Player.ServerPlayer.playerObject);
		}
	}

	public override void SceneLoadLocalBegin(string scene, IProtocolToken token) {
		foreach (Player p in Player.GetPlayerList) {
			p.entity = null;
		}
	}
}
