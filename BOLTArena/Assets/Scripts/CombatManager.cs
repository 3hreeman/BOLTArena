using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager {
    static List<PlayerObject> playerObjectList = new List<PlayerObject>();

    public static void AddPlayer(PlayerObject player) {
        if (playerObjectList.Contains(player)) {
            return;
        }
        playerObjectList.Add(player);
        Debug.Log(player.name + " player object has added");    
    }

    public static void RemovePlayer(PlayerObject player) {
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
        // Debug.Log("atk pos : "+atk_pos.ToString());
        // Debug.Log("target pos : "+target_pos.ToString());
        // Debug.Log("distance : "+dist);
        Debug.Log(dist + " > "+attacker.range);
        if (dist > attacker.range) {

            Debug.Log("Not In Range");
            return false;
        }

        if (attacker.dir == 1 && atk_pos.x > target_pos.x) {
            Debug.Log("target is behind");
            return false;
        }
        if (attacker.dir == -1 && atk_pos.x < target_pos.x) {
            Debug.Log("target is behind");
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

            if (CheckTargetInRange(attacker, target)) {
                target.TakeDmg(attacker.atkDmg);
            }
        }
    }
}
