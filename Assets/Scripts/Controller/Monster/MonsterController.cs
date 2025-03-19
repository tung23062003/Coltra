using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public enum SpriteDirection
    {
        LEFT = -1,
        RIGHT = 1
    }

    [Header("---------- Monster ----------")]
    [SerializeField] protected Monster monster;

    internal SpriteRenderer spriteRenderer;
    internal Rigidbody2D rb2D;
    internal Animator anim;
    internal List<Color> colorList;

    [SerializeField] internal PlayerController target;

    [Header("---------- Movement ----------")]
    public SpriteDirection spriteDirection;
    public LayerMask obstacleLayers;
    public float obstacleDistance;
    internal Vector2 moveDirection;

    [Header("---------- Attack ----------")]
    public GameObject bulletPrefab;
    public List<GameObject> bulletPool;
    public int amountBulletsToPool;

    public Transform attackPoint;
    public float attacksPerSec;
    public float bulletForce;

    [SerializeField] protected int meleeDamage = 1;
    protected float lastTimeAttack;
    protected bool isAttacking;

    [Header("---------- State ----------")]
    public float liveTime;
    private float timer;

    public CharacterColor bulletColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        //target = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

        moveDirection = new Vector2(target.transform.position.x - transform.position.x, 0);
        timer = 0;

        InitForAttack();
        InitStats();
    }

    protected virtual void InitStats()
    {

    }

    protected virtual void InitForAttack()
    {
        if (bulletPrefab != null)
        {
            Transform pool = GameObject.Find("ObjectPool").transform;
            bulletPool = new List<GameObject>();
            GameObject bullet;
            EnemyBulletController bulletCtrl;
            for (int i = 0; i < amountBulletsToPool; i++)
            {
                bullet = Instantiate(bulletPrefab, pool);
                bulletCtrl = bullet.GetComponent<EnemyBulletController>();
                bulletCtrl._renderer.color = ColorData.GetColor(bulletColor);
                bulletCtrl.bulletColor = bulletColor;
                bullet.SetActive(false);
                bulletPool.Add(bullet);
            }
        }
    }

    void Update()
    {
        MonsterState();

        UpdateAnimation();
    }

    internal virtual void MonsterMove()
    {
        Vector2 velocity = moveDirection.normalized * monster.velocity;
        rb2D.velocity = velocity;
        
        RaycastHit2D hitObstacle = Physics2D.Raycast(transform.position, moveDirection.normalized, obstacleDistance, obstacleLayers);
        if (hitObstacle.collider != null)
        {
            moveDirection = new Vector2(target.transform.position.x - transform.position.x, 0);
            rb2D.velocity = velocity * -1;
        }

        Flip(rb2D.velocity.x);
    }

    protected void Flip(float xVel)
    {
        Vector3 scale = transform.localScale;
        if (xVel * scale.x * (int)spriteDirection < 0) scale.x *= -1f;
        transform.localScale = scale;
    }

    internal void MonsterAttack()
    {
        if (Time.time < lastTimeAttack + 1 / attacksPerSec)
        {
            return;
        }

        anim.SetTrigger("IsAttacking");

        lastTimeAttack = Time.time;
    }

    public void RangedAttack()
    {
        Vector2 shootDir = target.transform.position - attackPoint.position;
        if (shootDir.x * moveDirection.x <= 0) return;
        ShootBullet(shootDir);
    }

    public virtual void ShootBullet(Vector2 direction)
    {
        GameObject bullet = GetPooledBullet();
        var bulletCtrl = bullet.GetComponent<EnemyBulletController>();
        if (bullet != null)
        {
            bullet.transform.position = attackPoint.position;
            float rotateValue = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, rotateValue + (int)bulletCtrl.spriteDirection);

            //if (spriteRenderer.color != null)
            //    bulletCtrl.SetBulletColor(bulletColor);

            bullet.SetActive(true);
            bulletCtrl._rigid.AddForce(direction.normalized * bulletForce);
        }
    }

    public virtual GameObject GetPooledBullet()
    {
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (!bulletPool[i].activeInHierarchy)
            {
                return bulletPool[i];
            }
        }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerEnter2D(collision);
    }

    protected virtual void TriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(GameConstants.playerBullet))
        {
            if (collision.GetComponent<BulletBase>().bulletColor == bulletColor) {
                DecreaseHealth();
                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                VFXManager.Instance.PlayVFX("Boom", hitPoint, Quaternion.identity);
            } 

            if (IsDead())
            {
                gameObject.SetActive(false);
                GameEvent.OnEnemyKill?.Invoke(bulletColor, false);
            }

            collision.gameObject.SetActive(false);
        }
    }

    protected virtual void DecreaseHealth()
    {
        monster.health--;
    }

    protected bool IsDead()
    {
        return monster.health == 0;
    }

    internal virtual void MonsterState()
    {
        timer += Time.deltaTime;
        if (timer > liveTime)
        {
            Destroy(gameObject);
            Time.timeScale = 0;
            GameEvent.OnPlayerDead?.Invoke();
        }
    }

    protected virtual void UpdateAnimation()
    {
        anim.SetFloat("xVelocity", rb2D.velocity.x);
        anim.SetFloat("yVelocity", rb2D.velocity.y);
        anim.SetBool("IsDead", IsDead());
    }
}
