using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Random = System.Random;
using UE = UnityEngine;

public partial class Player : IDisposable
{
	public string name;
	public BoltEntity entity;
	public BoltConnection connection;
	public PlayerObject playerObject;
	public PlayerData playerData;
	
	public IPlayerState state {
		get { return entity.GetState<IPlayerState>(); }
	}

	public bool isServer {
		get { return connection == null; }
	}

	public Player(PlayerData data) {
		playerData = data;
		name = playerData.m_playerName;
		m_PlayerList.Add(this);
	}

	public void Kill() {
		if (entity) {
			state.IsDead = true;
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

	public void InstantiateEntity(PlayerData data, bool isNpc) {
		entity = BoltNetwork.Instantiate(BoltPrefabs.PlayerObject, data, RandomPosition(isNpc), Quaternion.identity);

		if (entity) {
			Debug.Log("-----------------------Player :: SpawnPlayerObject");
			// entity.transform.position = RandomPosition(isNpc);
			playerObject = entity.GetComponent<PlayerObject>();
			playerObject.name = name;
			playerObject.MAX_HP = isNpc ? UnityEngine.Random.Range(30, 50) : 100;
			state.IsNpc = isNpc;
			state.MaxHp = playerObject.MAX_HP;
			state.Hp = playerObject.MAX_HP;
			state.Direction = 1;
			if (isNpc) {
				// Debug.Log("-----------------------Player :: added NpcController Component");
				playerObject.m_npcController = playerObject.gameObject.AddComponent<NpcController>();
			}
		}
		
		state.Name = data.m_playerName;
		entity.gameObject.name = data.m_playerName;
		if (isServer) {
			entity.TakeControl();
		}
		else {
			entity.AssignControl(connection);
		}
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
		CombatManager.GeneratePlayerObject(Player.ServerPlayer, false);
	}

	static Vector3 RandomPosition(bool isNpc) {
		float half_height = isNpc ? 5 : Camera.main.orthographicSize;
		float half_width = isNpc ? 5 : half_height * Camera.main.aspect;
		float x = UE.Random.Range(-half_width, half_width);
		float y = UE.Random.Range(-half_height, half_height);
		return new Vector3(x, y, y);
	}

}