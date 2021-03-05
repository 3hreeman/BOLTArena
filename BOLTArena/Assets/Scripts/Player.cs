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

	public IPlayerState state {
		get { return entity.GetState<IPlayerState>(); }
	}

	public bool isServer {
		get { return connection == null; }
	}

	public Player() {
		m_PlayerList.Add(this);
	}

	public void Kill() {
		if (entity) {
			state.IsDead = true;
			state.RespawnFrame = BoltNetwork.ServerFrame + (15 * BoltNetwork.FramesPerSecond);
		}
	}

	internal void Spawn() {
		if (entity) {
			state.Hp = 100;
			state.Direction = 1;
			// teleport
			entity.transform.position = RandomPosition();
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

	public void InstantiateEntity()
	{
		entity = BoltNetwork.Instantiate(BoltPrefabs.PlayerObject, RandomPosition(), Quaternion.identity);

		state.Name = name;

		if (isServer) {
			entity.TakeControl();
		}
		else {
			entity.AssignControl(connection);
		}

		Spawn();
	}
}

partial class Player
{
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
		ServerPlayer = new Player();
	}

	static Vector3 RandomPosition() {
		float half_height = Camera.main.orthographicSize;
		float half_width = half_height * Camera.main.aspect;
		Debug.Log(Camera.main.aspect + " :: " + half_width + " , "+ half_height);
		float x = UE.Random.Range(-half_width, half_width);
		float y = UE.Random.Range(-half_height, half_height);
		return new Vector3(x, y, y);
	}

}