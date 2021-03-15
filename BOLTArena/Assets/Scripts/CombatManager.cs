using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CombatManager {
    public static PlayerObject ownerPlayerObject = null;
    static List<PlayerObject> allPlayerObjectList = new List<PlayerObject>();
    private static List<PlayerObject> realPlayerList = new List<PlayerObject>();
    private static List<PlayerObject> npcPlayerList = new List<PlayerObject>();


    public static void SetOwnerPlayerObject(PlayerObject po) {
        Debug.Log("CombatManager :: SetOwnerPlayerObject");
        ownerPlayerObject = po;
    }
    
    public static void GenerateNpc(int count=1) {
        for (int i = 0; i < count; i++) {
            var resKey = StartMain.GetRandomResKey();
            var data = new PlayerData() {m_playerName = "NPC", m_resKey = resKey};
            var npc_player = new Player(data);
            GeneratePlayerObject(npc_player, true);			
        }
    }
    
    public static void GeneratePlayerObject(Player player, bool isNpc) {
        Debug.Log("CombatManager :: GeneratePlayerObject Start");
        player.InstantiateEntity(player.playerData, isNpc);
        var playerObj = player.playerObject;
        AddPlayerObject(playerObj);

        Debug.Log("CombatManager :: GeneratePlayerObject End");
    }
    
    public static void AddPlayerObject(PlayerObject player) {
        Debug.Log("Add Player :: "+(player.state.IsNpc ? "NPC" : "REAL PLAYER"));
        if (allPlayerObjectList.Contains(player)) {
            return;
        }

        if (player.state.IsNpc) {
            npcPlayerList.Add(player);
        }
        else {
            realPlayerList.Add(player);
        }
        allPlayerObjectList.Add(player);
    }

    public static void RemovePlayerObject(PlayerObject player) {
        if (allPlayerObjectList.Contains(player)) {
            allPlayerObjectList.Remove(player);
            if (player.state.IsNpc) {
                npcPlayerList.Remove(player);
            }
            else {
                realPlayerList.Remove(player);
            }
            Debug.Log(player.name + " player object has added");
        }
    }

    public static List<PlayerObject> GetScoreBaseSortedPlayerList() {
        return realPlayerList.OrderByDescending(a=>a.state.score).ToList();
    }
    
    public static bool CheckTargetInRange(PlayerObject attacker, PlayerObject target) {
        if (attacker.IsAlive() == false || target.IsAlive() == false) {
            return false;
        }

        Vector2 atk_pos = attacker.GetCurPosition();
        Vector2 target_pos = target.GetCurPosition();
        var dist = Vector2.Distance(atk_pos, target_pos);
        
        if (dist > attacker.range) {
            return false;
        }

        if (attacker.dir == 1 && atk_pos.x > target_pos.x) {
            return false;
        }
        if (attacker.dir == -1 && atk_pos.x < target_pos.x) {
            return false;
        }

        return true;
    }
    
    public static bool DoAttack(PlayerObject attacker, PlayerObject target) {
        target.TakeDmg(attacker.atkDmg);
        return true;
    }

    public static void DoAttack(PlayerObject attacker) {
        bool isStun = false;
        foreach (var target in allPlayerObjectList) {
            if (target == attacker) {
                continue;
            }
            /*
             if (attacker.IsNpc == target.IsNpc) {
                continue;
            }
            
            if (CheckTargetInRange(attacker, target)) {
                // target.TakeDmg(attacker.atkDmg);
            }
            */

            if (CheckTargetInRange(attacker, target)) {
                if (attacker.state.IsNpc == target.state.IsNpc) {
                    int amount = target.playerScore;
                    attacker.AddScore(amount);
                    target.MinusScore(amount);
                }
                else {
                    int amount = 1;
                    attacker.AddScore(amount);
                    target.MinusScore(amount);
                    isStun = true;
                }
            }
        }

        if (isStun) {
            attacker.SetStun();
        }
    }

    public static PlayerObject GetNearestPlayer(PlayerObject player) {
        PlayerObject result = null;
        float min_dist = 0;
        foreach (var target in allPlayerObjectList) {
            if (target == player) {
                continue;
            }

            if (player.state.IsNpc == target.state.IsNpc) {
                continue;
            }

            if (target.IsAlive() == false) {
                continue;
            }
            
            if (result == null) {
                result = target;
                min_dist = Vector2.SqrMagnitude(player.transform.position - target.transform.position);
            }
            else {
                var new_dist = Vector2.SqrMagnitude(player.transform.position - target.transform.position);
                if (new_dist < min_dist) {
                    result = target;
                    min_dist = new_dist;
                }
            }
        }

        return result;
    }

    public static Vector2 GetDirection(PlayerObject player) {
        Vector2 result = Vector2.zero;

        if (player != null) {
        }

        return result;
    }
}
