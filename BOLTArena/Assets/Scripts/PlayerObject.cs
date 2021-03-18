using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Bolt;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Event = Bolt.Event;
using Random = System.Random;

public class PlayerObject : EntityEventListener<IPlayerState> {
    
    private SpineMechanimController m_spine;
    private Animator m_anim;
    public GamePadController m_pad = null;
    public UnitPanel m_unitPanel;
    
    public Vector2 inputVector; 
    private bool isRun;
    private bool isAttack;

    private float MOVE_SPD = 3;
    private CharacterController m_charController;

    private float ATK_COOLTIME = 1.5f;

    public float MAX_HP = 100;
    float m_atkDmg = 10;
    float m_hp = 100;
    float m_range = 1.5f;
    int m_dir = 1;

    private string m_playerName;
    public string playerName {
        get {
            return m_playerName;
        }
    }

    private int m_playerScore = 1;
    public int playerScore {
        get {
            return m_playerScore;
        }
    }

    public void AddScore(int amount) {
        m_playerScore += amount;
        state.score = m_playerScore;
    }

    public void MinusScore(int amount) {
        m_playerScore -= amount;
        if (m_playerScore < 0) {
            m_playerScore = 0;
        }

        state.score = m_playerScore;
        PlayHit(amount);
    }
    
    private float reviveInterval = 3;
    private float reviveAt = 0;

    private float stunEndAt = 0;
    private float STUN_TIME = 3;

    public float m_nextJumpTime = 0;
    public float m_nextAtkTime = 0;
    private float m_emojiTime = 0;
    private bool isMouseInput = false;

    public float hp {
        get { return m_hp; }
    }

    public float range {
        get { return m_range; }
    }

    public int dir {
        get { return m_dir; }
    }

    public float atkDmg {
        get { return m_atkDmg; }
    }
    
    public NpcController m_npcController;
    
    private void Awake() {
        m_spine = GetComponentInChildren<SpineMechanimController>();
        m_charController = GetComponent<CharacterController>();
        m_anim = GetComponent<Animator>();
        // Debug.Log("--------------------PlayerObject :: Awake() - "+state.IsNpc.ToString());
    }

    private void Start() {
        // Debug.Log("--------------------PlayerObject :: Start() - "+gameObject.name+ " :: "+state.IsNpc.ToString());

        if (BoltNetwork.IsServer) {
            if (this == Player.ServerPlayer.playerObject) {
                SetPad(GamePadController.instance);
                GamePadController.instance.RegisterBtnAction(TryAttack01, TryAttack02);
            }   
        }
        else if (BoltNetwork.IsClient) {
            if (entity.HasControl) {
                SetPad(GamePadController.instance);
                GamePadController.instance.RegisterBtnAction(TryAttack01, TryAttack02);
            }

            if (state.IsNpc == false) {
                CombatManager.AddPlayerObject(this);
            }
        }
    }

    public override void Attached() {
        // Debug.Log("--------------------PlayerObject :: Attached - "+gameObject.name+ " :: "+state.IsNpc.ToString());
        
        //Awake - Attached - Start 순으로 처리됨
        //최초 state동기화는 Start에서 처리하는게 맞음
        
        state.SetTransforms(state.Transform, transform);
        state.SetAnimator(m_spine.GetComponent<Animator>());
        var data = entity.AttachToken as PlayerData;
        // Debug.Log("attached token = "+ data.m_playerName+ " / " +data.m_resKey);
        m_playerName = data.m_playerName;
        
        m_unitPanel.init(data.m_playerName, entity.HasControl, state.IsNpc);
        m_spine.init(data.m_resKey);
        if (BoltNetwork.IsServer) {
            m_spine.SetAttackAction( () => { CombatManager.DoAttack(this); } );
        }
        else {
            if (entity.HasControl) {
                CameraManager.SetTarget(this);
            }            
        }
    }

    public override void Detached() {
        if (BoltNetwork.IsClient && state.IsNpc == false) {
            CombatManager.RemovePlayerObject(this);
        }
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
            TryAttack01();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
           TryAttack02();
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
            TryJump();
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

    public void TryJump() {
        if (m_nextJumpTime < BoltNetwork.ServerTime) {
            m_nextJumpTime = BoltNetwork.ServerTime + 1;
            SendAnimEvent("jump");
        }
    }
    public void TryAttack01() {
        if (m_nextAtkTime < BoltNetwork.ServerTime) {
            m_nextAtkTime = BoltNetwork.ServerTime + ATK_COOLTIME;
            SendAnimEvent("atk_01");
        }
    }

    public void TryAttack02() {
        if (m_nextAtkTime < BoltNetwork.ServerTime) {
            m_nextAtkTime = BoltNetwork.ServerTime + ATK_COOLTIME;
            SendAnimEvent("atk_02");
        }
    }

    void SendAnimEvent(string key) {
        var eventAnim = EventPlayerAnim.Create(entity, EntityTargets.Everyone);
        eventAnim.animKey = key;
        eventAnim.Send();
    } 
    
    public override void SimulateOwner() {
        CheckDeadUnitRevive();
    }

    void CheckDeadUnitRevive() {
        if (state.IsDead) {
            inputVector = Vector2.zero;
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
                    state.run = true;
                }
                else {
                    state.run = false;
                }

                if (cmd.Input.attack) {
                    UpdateAttack(cmd);
                }
            }
        }
    }

    void UpdateAttack(PlayerCommand cmd) {
        if (m_nextAtkTime <= BoltNetwork.ServerTime) { 
            state.atk_01();
            m_nextAtkTime = BoltNetwork.ServerTime + ATK_COOLTIME;
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
    
    public void SetStun() {
        SetAlive(false);
    }

    void PlayHit(int amount) {
        var hitEvent = EventPlayerHit.Create(entity, EntityTargets.Everyone);
        hitEvent.DmgAmount = amount;
        hitEvent.Send();
    }

    void SetAlive(bool isAlive) {
        if (isAlive) {
            state.Hp = MAX_HP;
            // state.stun();
            SendAnimEvent("stun");
            state.IsDead = false;
        }
        else {
            state.Hp = 0;
            reviveAt = BoltNetwork.ServerTime + reviveInterval;
            state.stun_end();
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
        int idx = UnityEngine.Random.Range(1, 3);
        m_anim.SetTrigger("hit_"+idx);
    }

    public override void OnEvent(EventPlayerAnim evnt) {
        m_spine.PlayAnimTrigger(evnt.animKey);
    }
}


