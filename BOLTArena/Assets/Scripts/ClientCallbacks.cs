using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

[BoltGlobalBehaviour("Stage")]
public class ClientCallbacks : Bolt.GlobalEventListener {
    public override void SceneLoadLocalBegin(string scene, IProtocolToken token) {
        // Debug.Log("ClientCallbacks :: SceneLoadLocalBegin");
        GamePadController.Instantiate();
    }

    public override void SceneLoadLocalDone(string scene, IProtocolToken token) {
        // Debug.Log("ClientCallbacks :: SceneLoadLocalDone");
        GamePadController.instance.padObj.transform.SetParent(GameObject.Find("Canvas").transform);
    }

    public override void ControlOfEntityGained(BoltEntity entity) {
        Debug.Log("--------------------ClientCallbacks :: ControlOfEntityGained - "+entity.gameObject.name);
    }
    
    public override void Disconnected(BoltConnection connection) {
        Debug.Log("ClientCallbacks :: Connection has Disconnected by "+connection.DisconnectReason);
        if (BoltNetwork.IsClient) {
            SceneManager.LoadScene("Start");
        }
    }
}
