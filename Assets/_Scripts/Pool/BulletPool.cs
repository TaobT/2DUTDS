using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;

    [SerializeField] private GameObject bulletPf;
    [SerializeField][Range(1, 100)] private int initialBullets;

    private List<GameObject> allBullets = new List<GameObject>();
    private static Stack<GameObject> inactiveBullets = new Stack<GameObject>();
    private static List<GameObject> activeBullets = new List<GameObject>();

    //Bullet configuration
    [SerializeField] private LayerMask teamAllyLayer;
    [SerializeField] private LayerMask teamEnemyLayer;
    [SerializeField] private LayerMask blockBulletLayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        for (int i = 0; i < initialBullets; i++)
        {
            GameObject bullet = Instantiate(bulletPf, transform);
            bullet.SetActive(false);
            bullet.GetComponent<Bullet>().InitializeBullet(teamAllyLayer, teamEnemyLayer, blockBulletLayer);
            allBullets.Add(bullet);
            inactiveBullets = new Stack<GameObject>(allBullets);
        }
    }

    public GameObject GetBullet()
    {
        if (inactiveBullets.Count > 0)
        {
            GameObject bullet = inactiveBullets.Pop();
            activeBullets.Add(bullet);
            return bullet;
        }

        GameObject newBullet = Instantiate(bulletPf, transform);
        newBullet.SetActive(false);
        newBullet.GetComponent<Bullet>().InitializeBullet(teamAllyLayer, teamEnemyLayer, blockBulletLayer);

        allBullets.Add(newBullet);
        activeBullets.Add(newBullet);
        return newBullet;
    }

    public void DisableBullet(GameObject bullet)
    {
        activeBullets.Remove(bullet);
        inactiveBullets.Push(bullet);
    }
}
