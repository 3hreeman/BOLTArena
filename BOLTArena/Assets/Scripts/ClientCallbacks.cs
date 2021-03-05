using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

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
        Debug.Log("ClientCallbacks :: ControlOfEntityGained");

        var po = entity.GetComponent<PlayerObject>();
        po.SetPad(GamePadController.instance);
    }
}
