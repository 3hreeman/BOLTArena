using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BoltGlobalBehaviour]
public class TokenCallbacks : Bolt.GlobalEventListener {
    public override void BoltStartBegin() {
        BoltNetwork.RegisterTokenClass<PlayerData>();
    }
}
