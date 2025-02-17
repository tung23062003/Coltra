using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : CharacterBase
{
    private float inputX;

    SpriteRenderer sr;

    [Header("---------Shoot---------")]
    public Transform shootPoint;
    public GameObject bulletPrefab;
    [SerializeField] private float _shootTime;
    float _shootTimeCount = 0;

    [Header("---------Bullet available---------")]
    public Dictionary<CharacterColor, int> AbsorbBullettDict = new();

    [Header("---------Check Player is Gray Color?---------")]
    public bool isGray = true;

    [Header("---------Player---------")]
    public Player player;

    public float bulletForce = 500.0f;

    protected override void Start()
    {
        base.Start();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead)
            return;
        InputControl();
        FlipController();
        PlayerShoot();

        UpdateAnimation();
    }

    private void InputControl()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 velocity = rb.velocity;
        velocity.x = inputX * speed * Time.fixedDeltaTime;
        rb.velocity = velocity;
    }

    private void Jump()
    {
        if(IsGround())
            rb.AddForce(new Vector2(rb.velocity.x, jumpForce), ForceMode2D.Impulse);
    }

    private void FlipController()
    {
        if (inputX == 1 && !facing_right)
            Flip();
        else if (inputX == -1 && facing_right)
            Flip();
    }

    private void PlayerShoot()
    {
        if (_shootTimeCount > 0)
        {
            _shootTimeCount -= Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (!isGray && AbsorbBullettDict[currentColor] > 0)
            {
                anim.SetTrigger("IsAttacking");

                AbsorbBullettDict[currentColor]--;
                SpawnBullet();

                if (AbsorbBullettDict[currentColor] <= 0)
                {
                    currentColor = CharacterColor.GRAY;
                    isGray = true;
                    UpdatePlayerColor();
                }
            }
        }
    }

    private void SpawnBullet()
    {
        GameObject g = ObjectPool.Instance.GetObject(bulletPrefab);
        if (g == null)
            return;
        g.transform.position = shootPoint.position;

        float dir = character_dir;
        g.transform.rotation = Quaternion.Euler(0, (dir == 1 ? 0 : -180), 0);

        BulletBase bulletBase = g.GetComponent<BulletBase>();
        bulletBase.SetBulletColor(currentColor);
        bulletBase.ChangeBulletColor();
        g.SetActive(true);

        bulletBase._rigid.AddForce(bulletForce * this.transform.right);

        _shootTimeCount = _shootTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(GameConstants.enemyBullet))
        {
            TakeDamage(collision.GetComponent<EnemyBulletController>().bulletColor);
            collision.gameObject.SetActive(false);
        }

        if (collision.gameObject.CompareTag(GameConstants.gateEnd))
        {
            rb.velocity = Vector2.zero;
            anim.SetTrigger("Player_disapear");
            StartCoroutine(DelaySecond());
        }
    }

    IEnumerator DelaySecond()
    {
        yield return new WaitForSeconds(1.0f);
        SceneController.Instance.NextLevel();
    }

    void TakeDamage(CharacterColor bulletColor)
    {
        if (isDead) return;

        anim.SetTrigger("IsHurt");
        if (isGray)
        {
            AbsorbBullett(bulletColor);
        }
        else if (bulletColor != currentColor)
        {
            DecreaseHealth();
            GameEvent.OnPlayerIsShot?.Invoke(player.health);
            if (player.health == 0)
            {
                Die();
                return;
            }
            AbsorbBullett(bulletColor);
        }
        else
        {
            AbsorbBullettDict[bulletColor]++;
        }
    }

    protected void DecreaseHealth() => player.health--;

    protected void IncreaseHealth() => player.health++;

    private void Die()
    {
        GameEvent.OnPlayerDead?.Invoke();
        isDead = true;
        Debug.Log("You are dead!!!");
        Time.timeScale = 0;
    }

    void AbsorbBullett(CharacterColor bulletColor)
    {
        AbsorbBullettDict.Clear();
        isGray = false;
        currentColor = bulletColor;
        AbsorbBullettDict.Add(currentColor, 1);
        UpdatePlayerColor();
    }

    void UpdatePlayerColor()
    {
        GetComponent<SpriteRenderer>().color = ColorData.GetColor(currentColor);
    }

    void UpdateAnimation()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("IsDead", isDead);
    }

    public void LockAlpha()
    {
        Color color = sr.color;
        color.a = 1f;
        sr.color = color;
    }
}
