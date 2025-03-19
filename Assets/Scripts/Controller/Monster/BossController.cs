using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonsterController
{
    [Header("---------- Stats ----------")]
    [SerializeField] private int maxHP;

    public CharacterColor BossColor;

    protected override void InitStats()
    {
        maxHP = monster.health;
        IngameController.Instance.panelBoss.SetActive(true);
        IngameController.Instance.SetBossHP(monster.health, maxHP);
    }

    protected override void InitForAttack()
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
                CharacterColor temp = ColorData.GetRandomColor();
                bulletCtrl._renderer.color = ColorData.GetColor(temp);
                bulletCtrl.bulletColor = temp;
                bullet.SetActive(false);
                bulletPool.Add(bullet);
            }
        }
    }

    public override GameObject GetPooledBullet()
    {
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (!bulletPool[i].activeInHierarchy)
            {
                bulletPool[i].GetComponent<EnemyBulletController>().bulletColor = ColorData.GetRandomColor();
                return bulletPool[i];
            }
        }
        return null;
    }

    public override void ShootBullet(Vector2 direction)
    {
        GameObject bullet = GetPooledBullet();
        var bulletCtrl = bullet.GetComponent<EnemyBulletController>();
        if (bullet != null)
        {
            bullet.transform.position = attackPoint.position;
            float rotateValue = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, rotateValue + (int)bulletCtrl.spriteDirection);

            if (spriteRenderer.color != null)
            {
                var color = bullet.GetComponent<EnemyBulletController>().bulletColor;
                bulletCtrl.SetBulletColor(color);
                bullet.GetComponent<SpriteRenderer>().color = ColorData.GetColor(color);
            }

            bullet.SetActive(true);
            bulletCtrl._rigid.AddForce(direction.normalized * bulletForce);
        }
    }

    internal override void MonsterMove()
    {
        Vector2 velocity = moveDirection.normalized * monster.velocity;
        rb2D.velocity = velocity;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance < obstacleDistance)
        {
            rb2D.velocity = Vector2.zero;
        }

        Flip(rb2D.velocity.x);
    }

    protected override void DecreaseHealth()
    {
        base.DecreaseHealth();
        IngameController.Instance.SetBossHP(monster.health, maxHP);
    }

    internal override void MonsterState()
    {
        
    }

    protected override void UpdateAnimation()
    {

    }

    protected override void TriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(GameConstants.playerBullet))
        {
            if (collision.GetComponent<BulletBase>().bulletColor == BossColor)
            {
                DecreaseHealth();
                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                VFXManager.Instance.PlayVFX("Boom", hitPoint, Quaternion.identity);
            }
            if (IsDead())
            {
                IngameController.Instance.panelBoss.SetActive(false);
                IngameController.Instance.PlayBossDefeatedSound();
                gameObject.SetActive(false);
                GameEvent.OnEnemyKill?.Invoke(BossColor, true);
            }

            collision.gameObject.SetActive(false);
        }
    }
}
