using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GuyAnimationMode: byte { Idle, Walk, Run, Victory}
public enum GuyStatus : byte { Alive, Injured, Disabled}

[RequireComponent(typeof(Rigidbody))]
public sealed class SmallGuy : MonoBehaviour
{
    [SerializeField]private Animator animator;
    private GuyAnimationMode animationMode;
    public new Transform transform { get; private set; }
    public GuyStatus status { get; private set; }
    private float deathTimer;
    public const float HEIGHT = 0.3f, DEATH_TIME = 5f;

    private void Awake()
    {
        transform = GetComponent<Transform>();        
    }

    public void ChangeAnimationMode( GuyAnimationMode i_mode)
    {
        if (i_mode != animationMode)
        {
            string s = "Idle";
            if (i_mode != GuyAnimationMode.Idle)
            {
                switch (i_mode)
                {
                    case GuyAnimationMode.Run: s = "Run"; break;
                    case GuyAnimationMode.Walk: s = "Walk"; break;
                    case GuyAnimationMode.Victory: s = "VictoryIdle";break;
                }
            }
            animator.Play(s);
            animationMode = i_mode;
        }
    }
    public void RestartAnimator()
    {
        animator.enabled = true;
    }

    private void Update()
    {
        if (status == GuyStatus.Injured)
        {
            deathTimer -= Time.deltaTime;
            if (deathTimer < 0f)
            {
                var sm = SessionMaster.current;
                sm.playerController.MoveGuyFromInjuredToDisabled(this);
                status = GuyStatus.Disabled;
                if (GetComponentInChildren<Renderer>().isVisible) sm.effectsMaster.CorpseDisappearEffect(transform.position);
                gameObject.SetActive(false);
            }            
        }
    }

    public void Restore()
    {
        status = GuyStatus.Alive;
        ChangeAnimationMode(GuyAnimationMode.Run);
    }


    private void OnTriggerEnter(Collider c)
    {
        var tag = c.tag;
        if (tag == GameConstants.COLLECTIBLES_TAG)
        {
            var col = c.GetComponent<Collectible>();
            if (col != null) col.Collect(); else Debug.Log("Не добавлен компонент Collectible");
        }
        else
        {
            if (tag == GameConstants.DANGER_TAG)
            {
                var dc = c.GetComponent<DangerObject>();
                if (dc != null)
                {
                    transform.parent = null;
                    animator.enabled = false;
                    var sm = SessionMaster.current;
                    sm.playerController.MoveGuyToInjured(this);
                    var rg = GetComponent<Rigidbody>();
                    rg.isKinematic = false;
                    rg.useGravity = true;
                    dc.SpecialDeathEffects(this);
                    deathTimer = DEATH_TIME;
                    status = GuyStatus.Injured;
                    sm.effectsMaster.DeathEffect(transform.position);
                }
                else Debug.Log("Не добавлен компонент Collectible");
            }
        }
    }
}
