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

        var atk_pos = attacker.GetCurPosition();
        var target_pos = target.GetCurPosition();
        var dist = Vector2.Distance(atk_pos, target_pos);
        // Console.WriteLine("atk pos : "+atk_pos.ToString());
        // Console.WriteLine("target pos : "+target_pos.ToString());
        // Console.WriteLine("distance : "+dist);
        if (Vector2.Distance(atk_pos, target_pos) > attacker.m_range) {
            return false;
        }

        if (attacker.m_dir == 1 && atk_pos.x > target_pos.x) {
            return false;
        }
        if (attacker.m_dir == -1 && atk_pos.x < target_pos.x) {
            return false;
        }

        return true;
    }
    
    public static bool DoAttack(PlayerObject attacker, PlayerObject target) {
        target.TakeDmg(attacker.m_atkDmg);
        return true;
    }
}
