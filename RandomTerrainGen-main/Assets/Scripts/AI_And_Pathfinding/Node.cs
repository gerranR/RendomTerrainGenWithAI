using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool containsPlayer;
    public GameObject playerPrefab;
    public GameObject Player;

    public Node cameFrom;
    public List<Node> conactions;

    public float g;
    public float h;

    public float f()
    {
        return g + h;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if(conactions.Count > 0)
        {
            for(int i = 0; i < conactions.Count; i++)
            {
                Gizmos.DrawLine(transform.position, conactions[i].transform.position);
            }
        }
    }

    private void Update()
    {
        
        if(containsPlayer && Player == null)
        {
            Player = Instantiate(playerPrefab, transform.position, Quaternion.identity, transform);
        }
        else if(!containsPlayer && Player != null)
        {
            Destroy(Player);
        }
    }
}
