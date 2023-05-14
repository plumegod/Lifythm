using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioSystem._Scripts;
using AudioSystem._Scripts.Data;
using cakeslice;
using Unity.VisualScripting;
using UnityEngine;

public class AudioEffectManager : MonoBehaviour
{
    [SerializeField] protected EasyAudioType audioType;
    [HideInInspector][SerializeField] protected Animator animator;
    [HideInInspector][SerializeField] protected Outline outline;

    protected AudioGameController audioGameController;

    protected bool _isCreate = false;
    [HideInInspector][SerializeField] protected string audioName = null;
    
    [SerializeField] protected string laneName = null;

    protected virtual void Start()
    {
        audioName = EasyAudioSystem.Instance.CreateAudioSource(audioType);
        _isCreate = true;

        animator = GetComponent<Animator>();
        outline = GetComponent<Outline>();

        if(audioName != null)
            return;
        
        _isCreate = false;
        Debug.LogError("音效创建失败");
    }

    protected virtual async Task OnClick()
    { 
        await EasyAudioSystem.Instance.ChargeAudioSource(audioName, AudioState.Play);
        TogglePlayAnimator();

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Note");
        foreach (var obj in objects)
        {
            if(!obj.activeSelf)
                return;
            
            obj.SendMessage("OnHit",laneName);
        }
    }

    protected virtual void OnMouseEnter()
    {
        ToggleOutlineEffect(false);
    }

    protected virtual void OnMouseExit()
    {
        ToggleOutlineEffect(true);
    }

    protected virtual void OnDestroy()
    {
        if(_isCreate)
            EasyAudioSystem.Instance.RemoveAudioSource(audioName);
    }

    protected virtual void TogglePlayAnimator()
    {
        animator.Play($"Play",0,0);
    }

    protected virtual void ToggleOutlineEffect(bool value)
    {
        outline.eraseRenderer = value;
    }
}
