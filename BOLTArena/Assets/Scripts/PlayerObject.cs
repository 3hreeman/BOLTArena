using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Event = Bolt.Event;

public class PlayerObject : EntityEventListener<IPlayerState> {
    
    private SpineMechanimController m_spine;
    public GamePadController m_pad = null;
    public UnitPanel m_unitPanel;
    
    public Vector2 inputVector; 
    private bool isRun;
    private bool isAttack;

    private float MOVE_SPD = 3;
    private CharacterController m_charController;

    private float nextAtkTime = 0;
    private float atkCooltime = 1;

    public float MAX_HP = 100;
    float m_atkDmg = 10;
    float m_hp = 100;
    float m_range = 1;
    int m_dir = 1;

    private float reviveInterval = 3;
    private float reviveAt = 0;

    public float hp {
        get { return m_hp; }
    }

    public float range {
        get { return m_range; }
    }

    public float dir {
        get { return m_dir; }
    }

    public float atkDmg {
        get { return m_atkDmg; }
    }
    
    public NpcController m_npcController;

    public bool IsNpc {
        get { return m_npcController != null; }
    }
    
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
        m_hp = state.Hp;
        m_dir = state.Direction;
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

        if (state.IsDead) {
            return;
        }

        if (m_npcController != null) {
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
            SetInputVector((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position));
        }
        
        if (Input.GetMouseButtonUp(1)) {
            inputVector = Vector2.zero;
            isMouseInput = false;
        }

        if (isMouseInput == false) {
            if (m_pad != null) {
                if (m_pad.isInput) {
                    SetInputVector(m_pad.inputVector);
                }
                else {
                    SetInputVector(Vector2.zero);
                }
            }
        }
        
        if (Input.GetKey(KeyCode.Space)) {
            SetAttack(true);
        }
        else {
            SetAttack(false);
        }

        isRun = Input.GetKey(KeyCode.LeftShift);
    }

    public void SetInputVector(Vector2 dir) {
        dir.x = Mathf.Clamp(dir.x, -1, 1);
        dir.y = Mathf.Clamp(dir.y, -1, 1);

        inputVector = dir;
    }

    public void SetAttack(bool flag) {
        isAttack = flag;
    }

    public override void Attached() {
        //최초 동기화가 이루어진다음 처리할 내용
        state.SetTransforms(state.Transform, transform);
        state.SetAnimator(GetComponentInChildren<Animator>());
        var data = entity.AttachToken as PlayerData;
        // Debug.Log("attached token = "+ data.m_playerName+ " / " +data.m_resKey);

        m_unitPanel.init(data.m_playerName, entity.HasControl, state.IsNpc);
        m_spine.init(data.m_resKey);
        if (BoltNetwork.IsServer) {
            m_spine.SetAttackAction( () => { CombatManager.DoAttack(this); } );
        }
        // if (IsNpc == false) {
        //     CameraManager.SetTarget(this);
        // }
    }

    public override void SimulateOwner() {
        CheckDeadUnitRevive();
    }

    void CheckDeadUnitRevive() {
        if (state.IsDead) {
            if (reviveAt <= BoltNetwork.ServerTime) {
                SetAlive(true);
            }
        }
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
        if (state.IsDead) {
            return;
        }
        Debug.Log(gameObject.name + " take dmg " + dmg);
        m_hp -= dmg;
        if (m_hp <= 0) {
            Debug.Log("DEADEADEADEADEAD");
        }

        state.Hp = m_hp;
        if (state.Hp <= 0) {
            state.Hp = 0;
            SetInputVector(Vector2.zero);
            SetAlive(false);
        }
        var hitEvent = EventPlayerHit.Create(entity, EntityTargets.Everyone);
        hitEvent.DmgAmount = dmg;
        hitEvent.Send();
    }

    void SetAlive(bool isAlive) {
        if (isAlive) {
            state.Hp = 100;
            state.damage = false;
            state.IsDead = false;
        }
        else {
            state.Hp = 0;
            reviveAt = BoltNetwork.ServerTime + reviveInterval;
            state.damage = true;
            state.IsDead = true;
        }
    }

    public Vector2 GetDmgTextPos() {
        return m_spine.GetDmgTextPos();
    }

    public Vector2 GetCanvasPosition() {
        return m_spine.GetCanvasPosition();
    }

    public float GetCurHpRatio() {
        return Mathf.Clamp(state.Hp / state.MaxHp, 0, 1);
    }
    
    public override void OnEvent(EventPlayerHit evnt) {
        CombatDmgFontObject.PrintDmgFont(GetDmgTextPos(), evnt.DmgAmount.ToString(), CombatDmgFontObject.DmgTxtType.NormalAtk, 0);
        m_spine.PlayHitEffect();
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


