using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;
using UnityEngine.SceneManagement;

[BoltGlobalBehaviour("Stage")]
public class ClientCallbacks : Bolt.GlobalEventListener {
    public override void SceneLoadLocalBegin(string scene, IProtocolToken token) {
        Debug.Log("ClientCallbacks :: SceneLoadLocalBegin");
        GamePadController.Instantiate();
    }

    public override void SceneLoadLocalDone(string scene, IProtocolToken token) {
        Debug.Log("ClientCallbacks :: SceneLoadLocalDone");
        GamePadController.instance.transform.SetParent(GameObject.Find("Canvas").transform);
    }

    public override void ControlOfEntityGained(BoltEntity entity) {
        Debug.Log("ClientCallbacks :: ControlOfEntityGained - "+entity.gameObject.name);

        var po = entity.GetComponent<PlayerObject>();
        if (po.state.IsNpc == false) {
            CameraManager.SetTarget(po);
            po.SetPad(GamePadController.instance);
        }
    }

    public override void Disconnected(BoltConnection connection) {
        Debug.Log("ClientCallbacks :: Connection has Disconnected by "+connection.DisconnectReason);
        SceneManager.LoadScene("Start");
    }
}
