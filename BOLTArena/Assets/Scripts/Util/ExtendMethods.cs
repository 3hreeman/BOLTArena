using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtendMethods {
    public static Player GetPlayer(this BoltConnection connection) {
        if (connection == null) {
            return null;
        }

        return (Player) connection.UserData;
    }
}