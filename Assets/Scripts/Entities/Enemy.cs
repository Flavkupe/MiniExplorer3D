using UnityEngine;
using System.Collections;
using System;

[Flags]
public enum EnemyType
{
    FollowPlayer = 1,
    ShootAtPlayer = 2,
    FlyAround = 4,    
}

public class Enemy : GameCharacterBase
{
    public EnemyType Type = EnemyType.FollowPlayer;    
    public Rigidbody2D Projectile;				
    public float ProjectileSpeed = 10f;				

    public float Speed = 1.0f;

    public float GeneralCooldownBase = 1.0f;
    private float generalCooldown = 0.0f;

    public Transform ShootLocation;
    public float ShootCooldownBase = 5.0f;
    private float shootCooldown = 0.0f;

    private Animator animator;

    public GameObject DeathParticlePrefab;

    protected override void OnStart() 
    {
        base.OnStart();

        this.animator = this.GetComponent<Animator>();                
    }

    public override void OnHitBy(Collider2D other)
    {
        // TODO: generic projectile
        //if (other.GetComponent<Rocket>() != null)
        //{
        //    this.OnDeath();
        //}
    }

    protected virtual void OnDeath() 
    {
        Destroy(this.gameObject);
        if (this.DeathParticlePrefab != null)
        {
            GameObject particles = Instantiate(this.DeathParticlePrefab) as GameObject;
            particles.transform.position = this.transform.position;
            Destroy(particles, 2.0f);
        }
    }

    protected override void OnUpdate() 
    {
        base.OnUpdate();

        if (this.shootCooldown > 0)
        {
            this.shootCooldown -= Time.deltaTime;
        }

        if (this.generalCooldown > 0)
        {
            this.generalCooldown -= Time.deltaTime;
        }

        if (this.Type == EnemyType.FlyAround)
        {
            if (this.generalCooldown <= 0)
            {
                int solidObjectMask = ~(1 << 14);
                Collider2D[] frontHits = Physics2D.OverlapPointAll(this.FrontTransform.position, solidObjectMask);
                if (frontHits != null && frontHits.Length > 0)
                {                    
                    this.generalCooldown = this.GeneralCooldownBase;
                    this.Flip();
                }
            }

            float xVelocity = this.Speed;
            if (this.transform.localScale.x < 0) 
            {
                xVelocity *= -1;
            }
            
            if (this.GetComponent<Rigidbody2D>().linearVelocity.x != xVelocity) 
            {
                this.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(xVelocity, 0.0f);
            }
        }


        if (this.CanSeePlayer)
        {
            float direction = this.Player.transform.position.x < this.transform.position.x ? -1.0f : 1.0f;
            bool left = direction == -1.0f;
            Vector3 scale = this.transform.localScale;
            if (left && this.transform.localScale.x > 0 ||
                !left && this.transform.localScale.x < 0)
            {
                this.Flip();
            }

            if (this.Type == EnemyType.FollowPlayer)
            {
                float xVelocity = 0.0f;                            
                xVelocity = direction * this.Speed;
                this.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(xVelocity, this.GetComponent<Rigidbody2D>().linearVelocity.y);
            }
            else if (this.Type == EnemyType.ShootAtPlayer)
            {                
                if (this.ShootLocation != null && this.shootCooldown <= 0.0f)
                {
                    this.shootCooldown = this.ShootCooldownBase;

                    Rigidbody2D body = Instantiate(this.Projectile) as Rigidbody2D;
                    body.transform.position = this.ShootLocation.position;

                    float xVelocity = direction * this.ProjectileSpeed;
                    body.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(xVelocity, this.ProjectileSpeed);

                    
                    if (this.animator != null)
                    {
                        this.animator.SetTrigger("Throwing");
                    }

                }
            }
        }
    }    
}
