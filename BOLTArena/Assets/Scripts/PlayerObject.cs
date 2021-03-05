using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerObject : EntityEventListener<IPlayerState> {
    
    private SpineMechanimController m_spine;
    public GamePadController m_pad = null;

    private Vector2 inputVector; 
    private bool isRun;
    private bool isAttack;

    private float MOVE_SPD = 1;
    private CharacterController m_charController;

    private float nextAtkTime = 0;
    private float atkCooltime = 1;
    private void Awake() {
        m_spine = GetComponentInChildren<SpineMechanimController>();
        m_charController = GetComponent<CharacterController>();
    }

    public void SetPad(GamePadController pad) {
        m_pad = pad;
    }

    void Update() {
        UpdateInputCommand();
        UpdateStateView();
        UpdateByState();
    }

    void UpdateByState() {
        m_spine.SetDir(state.Direction);
    }

    public int DirectionView = 0;
    public float HpView = 0;
    void UpdateStateView() {
        DirectionView = state.Direction;
        HpView = state.Hp;
    }

    private bool isMouseInput = false;
    void UpdateInputCommand() {
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
}

