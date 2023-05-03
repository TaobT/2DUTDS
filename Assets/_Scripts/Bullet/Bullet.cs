using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    private CircleCollider2D circleCollider;
    private SpriteRenderer bulletSprite;

    private Team bulletTeam;

    private Vector2 lastPos;
    private Vector2 endPosition;
    private float speed;
    private int damage;
    private float sizeDecrement;
    private float actualSize;

    private LayerMask teamAllyLayer;
    private LayerMask teamEnemyLayer;


    private ContactFilter2D damageableFilter;
    private LayerMask blockBulletLayer;

    private List<Collider2D> hittedCols = new List<Collider2D>();

    private void Awake()
    {
        bulletSprite = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    private void FixedUpdate()
    {
        transform.position = Vector2.MoveTowards(transform.position, endPosition, speed * Time.deltaTime);

        CheckBulletCollisions();

        if (transform.position == (Vector3)endPosition) ReturnToPool();
    }

    private void CheckBulletCollisions()
    {

        actualSize = Vector2.Distance(transform.position, endPosition) * sizeDecrement;
        actualSize = Mathf.Clamp(actualSize, 0f, 1f);

        foreach (Vector2 step in GenerateBulletPath(lastPos, transform.position, 0.1f))
        {
            float stepSize = Vector2.Distance(step, endPosition) * sizeDecrement;
            //Check damageable layer
            List<Collider2D> damageableCol = new List<Collider2D>();
            Physics2D.OverlapCircle(step, stepSize, damageableFilter, damageableCol);

            damageableCol.RemoveAll(x => hittedCols.Find(c => c == x));

            foreach(Collider2D col in damageableCol)
            {
                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    hittedCols.Add(col);
                }
            }

            //Check blockbullet layer
            Collider2D blockBulletCol = Physics2D.OverlapCircle(step, stepSize, blockBulletLayer);
            if(blockBulletCol != null)
            {
                ReturnToPool();
            }

        }

        transform.localScale = new Vector3(actualSize, actualSize, 1);
        circleCollider.radius = actualSize;

        lastPos = transform.position;
    }

    public void InitializeBullet(LayerMask teamAllyLayer, LayerMask teamEnemyLayer, LayerMask blockBulletLayer)
    {
        this.teamAllyLayer = teamAllyLayer;
        this.teamEnemyLayer = teamEnemyLayer;
        this.blockBulletLayer = blockBulletLayer;

        damageableFilter.useLayerMask = true;
        damageableFilter.useTriggers = true;
    }

    public void ShootBullet(Vector2 initialPos, Vector2 endPos, float speed, int damage, float sizeDecrement, Team teamOwner, bool isDhBullet)
    {
        transform.position = initialPos;
        endPosition = endPos;
        this.speed = speed;
        this.damage = damage;
        this.sizeDecrement = sizeDecrement;
        lastPos = transform.position;

        switch(teamOwner)
        {
            case Team.TeamA:
                if(!isDhBullet) bulletSprite.color = Color.blue;
                else bulletSprite.color = new Color(0, 0, 0.5f);
                damageableFilter.layerMask = teamEnemyLayer;
                break;
            case Team.TeamB:
                if (!isDhBullet) bulletSprite.color = Color.red;
                else bulletSprite.color = new Color(0.5f, 0, 0);
                damageableFilter.layerMask = teamAllyLayer;
                break;
        }
    }

    private void ReturnToPool()
    {
        BulletPool.Instance.DisableBullet(gameObject);
        gameObject.SetActive(false);
        hittedCols.Clear();
    }

    private List<Vector2> GenerateBulletPath(Vector2 startPos, Vector2 endPos, float step)
    {
        List<Vector2> path = new List<Vector2>();
        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector2.Distance(startPos, endPos);
        int steps = Mathf.FloorToInt(distance / step);

        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / steps;
            Vector2 point = Vector2.Lerp(startPos, endPos, t);
            path.Add(point);
        }

        return path;
    }
}
