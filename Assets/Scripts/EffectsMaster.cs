using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EffectsMaster : MonoBehaviour
{
    public static EffectsMaster current { get; private set; }
    private Transform collectEffectTransform, deathEffectTransform, disappearEffectTransform;
    private ParticleSystem collectEffect, deathEffect, disappearEffect, salute;
    private const int COLLECT_EFFECT_VOLUME = 30, DEATH_EFFECT_VOLUME = 15, DISAPPEAR_EFFECT_VOLUME = 30, SALUTE_EMISSION_VOLUME = 120;
    private bool saluting = false;
    private const float SALUTE_TIME = 1.2f; private float saluteTimer;
    private Vector3 salutePosition;

    void Start()
    {
        if (current != null)
        {
            Destroy(this);
            return;
        }
        else current = this;
        //
        GameObject g = Instantiate(Resources.Load<GameObject>("Effects/collectEffectPE"));
        collectEffectTransform = g.transform;
        collectEffect = g.GetComponent<ParticleSystem>();
        //
        g = Instantiate(Resources.Load<GameObject>("Effects/deathEffectPE"));
        deathEffectTransform = g.transform;
        deathEffect = g.GetComponent<ParticleSystem>();
        //
        g = Instantiate(Resources.Load<GameObject>("Effects/disappearEffectPE"));
        disappearEffectTransform = g.transform;
        disappearEffect = g.GetComponent<ParticleSystem>();
    }


    private void Update()
    {
        if (saluting)
        {
            saluteTimer -= Time.deltaTime;
            if (saluteTimer < 0f)
            {
                salute.transform.position = salutePosition + Random.onUnitSphere * 2f;
                salute.Emit(SALUTE_EMISSION_VOLUME);
                saluteTimer = SALUTE_TIME;
            }
        }
    }

    public void CollectEffect(Vector3 point)
    {
        collectEffectTransform.position = point;
        collectEffect.Emit(COLLECT_EFFECT_VOLUME);
    }

    public void DeathEffect(Vector3 point)
    {
        deathEffectTransform.position = point;
        deathEffect.Emit(DEATH_EFFECT_VOLUME);
    }
    public void CorpseDisappearEffect(Vector3 point)
    {
        disappearEffectTransform.position = point;
        disappearEffect.Emit(DISAPPEAR_EFFECT_VOLUME);
    }

    public void FinishEffect(Vector3 point)
    {
        salute = Instantiate(Resources.Load<GameObject>("Effects/salute")).GetComponent<ParticleSystem>();
        saluting = true;
        salutePosition = point;
    }
    
}
