using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

public class SpineMechanimController : MonoBehaviour {
    public Dictionary<string, string> animDict = new Dictionary<string, string>() {
        {"idle", "idle"}, {"attack", "attack"}, {"move", "move"}, {"skill", "skill"}
    }; 
    
    public string m_resKey;
    public Animator m_animator;
    public SkeletonMecanim m_mechanim;
    public MeshRenderer m_meshRenderer;
    public Vector2 org_scale;
    public Vector2 dir_scale;
    private bool isInit = false;
    List<Bone> m_boneList = new List<Bone>();
    public Vector2 m_bound;
    public void init(string resKey) {
        m_resKey = resKey;
    }

    public void SetAttackAction(Action act) {
        hitAction = act;
    }
    
    void Start() {
        m_animator = GetComponent<Animator>();
        m_mechanim = GetComponent<SkeletonMecanim>();
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_boneList = m_mechanim.skeleton.Bones.ToList();
        dir_scale = org_scale = Vector3.one;
        m_bound = m_meshRenderer.bounds.size;
        // m_mechanim.initialSkinName = m_resKey;
        // m_mechanim.Initialize(true);
        foreach (var bone in m_mechanim.skeleton.Bones) {
            m_boneList.Add(bone);
        }
        isInit = true;
        
    }

    private Action hitAction = null;
    public Vector3 GetDmgTextPos() {
        var result = transform.position;
        result.y += m_bound.y * 1.25f;
        return result;
    }

    public Vector2 GetCanvasPosition() {
        var result = transform.position;
        result.y += m_bound.y;
        return Camera.main.WorldToScreenPoint(result);
    }

    public void PlayAnimTrigger(string key) {
        m_animator.SetTrigger(key);
    }
    
    public void SetDir(int dir) {
        if (isInit == false) {
            return;
        }
        dir_scale = org_scale;
        dir_scale.x = dir * dir_scale.x;
        m_mechanim.skeleton.SetLocalScale(dir_scale);
    }
    
    IEnumerator hitCoroutine = null;

    public void PlayHitEffect() {
        if(gameObject.activeSelf == false) {
            return;
        }
        if (hitCoroutine != null) {
            StopCoroutine(hitCoroutine);
            EndHitEffect();
        }
        hitCoroutine = HitEffect();
        StartCoroutine(hitCoroutine);
    }

    readonly float hit_eff_time = 0.25f;
    IEnumerator HitEffect() {
        float time = 0;

        foreach (var bone in m_boneList) {
            bone.Skeleton.SetColor(new Color(1, 0, 0, 1f));
        }
        yield return new WaitForSeconds(hit_eff_time);

        EndHitEffect();
    }
    
    void EndHitEffect() {
        foreach (var bone in m_boneList) {
            bone.Skeleton.SetColor(new Color(1, 1, 1, 1));
        }
        if(hitCoroutine != null) {
            StopCoroutine(hitCoroutine);
        }
        // anim_state.SetAnimation(0, "idle", true);
        hitCoroutine = null;
    }

    void atk_eff() {
        if (hitAction != null) {
            hitAction();
        }
    }
}
