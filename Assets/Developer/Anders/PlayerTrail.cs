using Alteruna;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrail : MonoBehaviour
{
    private TrailRenderer[] trails;
    [SerializeField] private AnimationCurve alphaCurve;
    [SerializeField] private float fadeOutTime = 1f;
   

    private TrailAttributesSync trailAttributes;
    private Color color = new Color(0.25f, 1, 0.25f);
    public bool IsActive;
    private bool isOwner;
    
    // Start is called before the first frame update
    void Start()
    {
        trails = GetComponentsInChildren<TrailRenderer>();
        trailAttributes = GetComponent<TrailAttributesSync>();
        if (trails.Length > 0)
            UpdateTrails(0);
    }

    public void Initialize(bool owningPlayer)
    {
        color = owningPlayer ? new Color(0.25f, 1, 0.25f) : new Color(1, 0.25f, 0.25f);
        isOwner = owningPlayer;
    }

    public void UpdateTrails(float dashProgress)
    {
        float alpha = alphaCurve.Evaluate(dashProgress);
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0, 0.0f), new GradientAlphaKey(alpha, 0.17f), new GradientAlphaKey(0, 1) }
        );
        foreach (var trail in trails)
        {
            trail.colorGradient = gradient;
        }
    }

    void Update()
    { 
        if (trailAttributes.currentLifeTime > 0f)
        {
            if (isOwner)
            {
                trailAttributes.currentLifeTime -= Time.deltaTime;
                IsActive = true;
                if (trailAttributes.currentLifeTime < 0f)
                {
                    trailAttributes.currentLifeTime = 0f;
                    IsActive = false;
                }
            }
            UpdateTrails((fadeOutTime - trailAttributes.currentLifeTime) / fadeOutTime);
        }
      

    }
    public void InactivateTrails()
    {
        foreach (var trail in trails)
        {
            trail.emitting = false;
        }
    }

    public void ActivateTrails()
    {
        if (isOwner)
            trailAttributes.currentLifeTime = fadeOutTime;
    }



    
}
