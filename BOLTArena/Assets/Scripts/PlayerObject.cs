using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class PlayerObject : EntityEventListener<IPlayerState> {
    
    private SpineMechanimController m_spine;
    public GamePadController m_pad = null;

    private Vector2 inputVector; 
    private bool isRun;
    private bool isAttack;

    private float MOVE_SPD = 3;
    private CharacterController m_charController;

    private float nextAtkTime = 0;
    private float atkCooltime = 1;

    public float m_atkDmg;
    public float m_hp;
    public float m_range;
    public int m_dir;
    
    private void Awake() {
        m_spine = GetComponentInChildren<SpineMechanimController>();
        m_charController = GetComponent<CharacterController>();
    }

    public void SetPad(GamePadController pad) {
        m_pad = pad;
    }

    void Update() {
        UpdateInputCommand();
        UpdateByState();
        Test_UpdateStateView();
    }

    void UpdateByState() {
        m_spine.SetDir(state.Direction);
    }

    public int DirectionView = 0;
    public float HpView = 0;
    void Test_UpdateStateView() {
        DirectionView = state.Direction;
        HpView = state.Hp;
    }

    private float m_nextBlinkTime = 0;
    private float m_nextAtkTime = 0;
    private float m_emojiTime = 0;
    private bool isMouseInput = false;
    void UpdateInputCommand() {
        if (entity.HasControl == false) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            if (m_nextBlinkTime < BoltNetwork.ServerTime) {
                m_nextBlinkTime = BoltNetwork.ServerTime + 1;
                var blink = TestEventPlayerBlink.Create(entity, EntityTargets.Everyone);
                blink.Send();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            if (m_nextAtkTime < BoltNetwork.ServerTime) {
                m_nextAtkTime = BoltNetwork.ServerTime + 0.5f;
                var eventAtk = EventPlayerAttack.Create(entity, EntityTargets.Everyone);
                eventAtk.Send();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            if (m_emojiTime < BoltNetwork.ServerTime) {
                m_emojiTime = BoltNetwork.ServerTime + 0.5f;
                var eventEmoji = EventPlayerEmoji.Create(entity, EntityTargets.Everyone);
                eventEmoji.Send();
            }
        }
        
        if (Input.GetMouseButton(1)) {
            isMouseInput = true;
            inputVector = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
            inputVector.x = Mathf.Clamp(inputVector.x, -1, 1);
            inputVector.y = Mathf.Clamp(inputVector.y, -1, 1);
        }
        
        if (Input.GetMouseButtonUp(1)) {
            inputVector = Vector2.zero;
            isMouseInput = false;
        }

        if (isMouseInput == false) {
            if (m_pad != null) {
                if (m_pad.isInput) {
                    inputVector = m_pad.inputVector;
                }
                else {
                    inputVector = Vector2.zero;
                }
            }
        }
        
        if (Input.GetKey(KeyCode.Space)) {
            isAttack = true;
        }
        else {
            isAttack = false;
        }

        isRun = Input.GetKey(KeyCode.LeftShift);
    }
    

    public override void Attached() {
        //최초 동기화가 이루어진다음 처리할 내용
        state.SetTransforms(state.Transform, transform);
        state.SetAnimator(GetComponentInChildren<Animator>());
        var data = entity.AttachToken as PlayerData;
        Debug.Log("attached token = "+ data.m_playerName+ " / " +data.m_resKey);
        m_spine.init(data.m_resKey, null);
    }

    public override void SimulateOwner() {
    }

    public override void SimulateController() {
        UpdateInputCommand();
        IPlayerCommandInput input = PlayerCommand.Create();
        
        input.inputVector = inputVector;
        input.isRun = isRun;
        input.attack = isAttack;
        
        entity.QueueInput(input);
    }

    public override void ExecuteCommand(Command command, bool resetState) {
        PlayerCommand cmd = (PlayerCommand) command;
        if (resetState) {
            //do something...
        }
        else {
            if (cmd.IsFirstExecution) {
                // animation
                if (cmd.Input.inputVector.x > 0) {
                    state.Direction = 1;
                }
                else if (cmd.Input.inputVector.x < 0) {
                    state.Direction = -1;
                }

                m_charController.Move(MOVE_SPD * cmd.Input.inputVector * BoltNetwork.FrameDeltaTime);                
                if (cmd.Input.inputVector != Vector3.zero) {
                    state.move = true;
                }
                else {
                    state.move = false;
                }

                if (cmd.Input.attack) {
                    UpdateAttack(cmd);
                }
            }
        }
    }

    void UpdateAttack(PlayerCommand cmd) {
        if (nextAtkTime <= BoltNetwork.ServerTime) { 
            state.attack();
            nextAtkTime = BoltNetwork.ServerTime + atkCooltime;
        }
    }

    public bool IsAlive() {
        return m_hp > 0;
    }

    public Vector3 GetCurPosition() {
        return transform.position;
    }

    public void TakeDmg(float dmg) {
        m_hp -= dmg;
        if (m_hp <= 0) {
            Debug.Log("DEADEADEADEADEAD");
        }
        if (entity.HasControl) {
            var hitEvent = EventPlayerHit.Create(GlobalTargets.Everyone, ReliabilityModes.ReliableOrdered);
            hitEvent.Send();
        }
    }
    
    public override void OnEvent(TestEventPlayerBlink evnt) {
        m_spine.PlayHitEffect();
    }

    public override void OnEvent(EventPlayerAttack evnt) {
        m_spine.PlayAnimTrigger("attack");
    }
    
    public override void OnEvent(EventPlayerEmoji evnt) {
        m_spine.PlayAnimTrigger("victory");
    }

}

