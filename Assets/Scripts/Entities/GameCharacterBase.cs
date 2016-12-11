using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameCharacterBase : MonoBehaviour
{
    protected GameObject Player;

    private Transform frontTransform;

    bool canSeePlayer = false;

    public Transform FrontTransform
    {
        get { return frontTransform; }
    }    

    protected bool CanSeePlayer
    {
        get { return canSeePlayer; }
    }

    void Start()
    {
        this.frontTransform = this.GetComponentsInChildren<Transform>().FirstOrDefault(item => item.tag == "FrontCheck");

        this.Player = StageManager.Player;
        NPCAreaCheck[] areaChecks = this.GetComponentsInChildren<NPCAreaCheck>();
        if (areaChecks != null)
        {
            foreach (NPCAreaCheck checks in areaChecks)
            {
                checks.PlayerDetected += HandlePlayerDetected;
            }
        }

        this.OnStart();
    }

    void Awake()
    {
        this.OnAwake();
    }

    void FixedUpdate()
    {
        this.canSeePlayer = false;
        this.OnFixedUpdate();
    }

    void Update()
    {
        this.OnUpdate();
    }

    protected virtual void HandlePlayerDetected(object sender, ObjectDetectedEventArgs e)
    {
        this.canSeePlayer = true;
    }

    public virtual void OnHitBy(Collider2D other)
    {
    }

    protected virtual void OnUpdate()
    {

    }

    protected virtual void OnFixedUpdate()
    {

    }

    protected virtual void OnStart()
    {

    }

    protected virtual void OnAwake()
    {

    }

    public void Flip()
    {
        Vector3 scale = this.transform.localScale;        
        this.transform.localScale = new Vector3(scale.x * -1, scale.y, scale.z);        
    }
}

