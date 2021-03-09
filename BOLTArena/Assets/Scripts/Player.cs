using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UE = UnityEngine;

public partial class Player : IDisposable
{
	public string name;
	public BoltEntity entity;
	public BoltConnection connection;
	public PlayerObject playerObject;
	
	public IPlayerState state {
		get { return entity.GetState<IPlayerState>(); }
	}

	public bool isServer {
		get { return connection == null; }
	}

	public Player(PlayerData data) {
		name = data.m_playerName;
		m_PlayerList.Add(this);
	}

	public void Kill() {
		if (entity) {
			state.IsDead = true;
			state.RespawnFrame = BoltNetwork.ServerFrame + (15 * BoltNetwork.FramesPerSecond);
		}
	}

	void SpawnPlayerObject() {
		if (entity) {
			state.Hp = 100;
			state.Direction = 1;
			// teleport
			entity.transform.position = RandomPosition();
			playerObject = entity.GetComponent<PlayerObject>();
			playerObject.name = name;
		}
	}

	public void Dispose() {
		m_PlayerList.Remove(this);

		// destroy
		if (entity) {
			BoltNetwork.Destroy(entity.gameObject);
		}

		// while we have a team difference of more then 1 player
		while (Mathf.Abs(redPlayers.Count() - bluePlayers.Count()) > 1) {
			if (redPlayers.Count() < bluePlayers.Count()) {
				var player = bluePlayers.First();
				player.Kill();
			}
			else {
				var player = redPlayers.First();
				player.Kill();
			}
		}
	}

	public void InstantiateEntity(PlayerData data) {
		entity = BoltNetwork.Instantiate(BoltPrefabs.PlayerObject, data, RandomPosition(), Quaternion.identity);

		state.Name = data.m_playerName;
		
		if (isServer) {
			entity.TakeControl();
		}
		else {
			entity.AssignControl(connection);
		}

		SpawnPlayerObject();
	}
}

partial class Player {
	static List<Player> m_PlayerList = new List<Player>();

	public static IEnumerable<Player> redPlayers {
		get { return m_PlayerList.Where(x => x.entity); }
	}

	public static IEnumerable<Player> bluePlayers {
		get { return m_PlayerList.Where(x => x.entity); }
	}

	public static IEnumerable<Player> GetPlayerList {
		get { return m_PlayerList; }
	}

	public static bool serverIsPlaying {
		get { return ServerPlayer != null; }
	}

	public static Player ServerPlayer {
		get;
		private set;
	}

	public static void CreateServerPlayer() {
		var data = new PlayerData() {m_playerName = "ServerPlayer", m_resKey = "Enemy"};
		ServerPlayer = new Player(data);
	}

	static Vector3 RandomPosition() {
		float half_height = Camera.main.orthographicSize;
		float half_width = half_height * Camera.main.aspect;
		float x = UE.Random.Range(-half_width, half_width);
		float y = UE.Random.Range(-half_height, half_height);
		return new Vector3(x, y, y);
	}

}