using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager {
    static List<PlayerObject> playerObjectList = new List<PlayerObject>();

    public static void GenerateNpc() {
        
    }
    public static void GeneratePlayerObject(Player player, PlayerData token, bool isNpc) {
        player.InstantiateEntity(token, isNpc);
        var playerObj = player.playerObject;
        AddPlayerObject(playerObj);
    }
    
    static void AddPlayerObject(PlayerObject player) {
        if (playerObjectList.Contains(player)) {
            return;
        }
        playerObjectList.Add(player);
        Debug.Log(player.name + " player object has added");    
    }

    public static void RemovePlayerObject(PlayerObject player) {
        if (playerObjectList.Contains(player)) {
            playerObjectList.Remove(player);
            Debug.Log(player.name + " player object has added");
        }
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
        foreach (var target in playerObjectList) {
            if (target == attacker) {
                continue;
            }
            if (attacker.IsNpc == target.IsNpc) {
                continue;
            }
            
            if (CheckTargetInRange(attacker, target)) {
                
                target.TakeDmg(attacker.atkDmg);
            }
        }
    }

    public static PlayerObject GetNearestPlayer(PlayerObject player) {
        PlayerObject result = null;
        float min_dist = 0;
        foreach (var target in playerObjectList) {
            if (target == player) {
                continue;
            }

            if (player.IsNpc == target.IsNpc) {
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
