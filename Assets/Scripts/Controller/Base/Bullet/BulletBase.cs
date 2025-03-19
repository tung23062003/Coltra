using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public enum SpriteDirection
    {
        TOP = 90,
        BOTTOM = -90,
        LEFT = 0,
        RIGHT = 180
    }
    public SpriteDirection spriteDirection;

    [SerializeField] float _lifeTime = 5.0f;
    internal Rigidbody2D _rigid;
    internal SpriteRenderer _renderer;
    public CharacterColor bulletColor;
    [HideInInspector] public bool isBulletPlayer = false;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
    }


    protected void HideBullet()
    {
        gameObject.SetActive(false);
        _rigid.velocity = Vector2.zero;
    }

    private void OnEnable()
    {
        StartCoroutine(DeactiveAfterTime());
    }

    IEnumerator DeactiveAfterTime()
    {
        yield return new WaitForSeconds(_lifeTime);
        HideBullet();
    }

    // Xu ly VFX va va cham
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    public void SetBulletColor(CharacterColor color)
    {
        this.bulletColor = color;
    }

    public void ChangeBulletColor()
    {
        _renderer.color = ColorData.GetColor(bulletColor);
    }

}
