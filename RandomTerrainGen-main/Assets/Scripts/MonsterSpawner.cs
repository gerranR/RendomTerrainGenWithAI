using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private float minDistFromPlayer;
    [SerializeField] private float maxDistFromPlayer;

    [SerializeField] private GameObject monster;
    [SerializeField] private float timeBetweenSpawns;
    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer >= timeBetweenSpawns)
        {
            float x = Random.Range(transform.position.x - maxDistFromPlayer, transform.position.x + maxDistFromPlayer);
            float z = Random.Range(transform.position.y - maxDistFromPlayer, transform.position.y + maxDistFromPlayer);

            if (x > transform.position.x && x < transform.position.x + minDistFromPlayer)
            {
                x = transform.position.x + minDistFromPlayer;
            }
            else if (x < transform.position.x && x > transform.position.x - minDistFromPlayer)
            {
                x = transform.position.x - minDistFromPlayer;
            }

            if (z > transform.position.z && z < transform.position.z + minDistFromPlayer)
            {
                z = transform.position.z + minDistFromPlayer;
            }
            else if (z < transform.position.z && z > transform.position.z - minDistFromPlayer)
            {
                z = transform.position.z - minDistFromPlayer;
            }

            Vector3 spawnPos = new Vector3(x, 20f, z);

            Instantiate(monster, spawnPos, Quaternion.identity);
            timer = 0;
        }
        timer += Time.deltaTime;
    }
}
