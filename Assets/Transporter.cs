using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transporter : MonoBehaviour
{
    bool rest;
    float restTime;
    int dir;                          //Direction of moving
    float speed = 18f;

    //NPCs waiting to go up
    public List<GameObject> NPC_Up = new List<GameObject>();
    //NPCs waiting to go down
    public List<GameObject> NPC_Down = new List<GameObject>();
    //NPCs currently using the transporter
    public List<GameObject> NPC_Moving = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        rest = true;
        restTime = 0;
        dir = 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (rest)
        {
            restTime += Time.deltaTime;
            if(restTime >= 2)
            {
                rest = false;
                restTime = 0;
                dir *= -1;
                SelectNPC();
            }
        }
        else
        {
            //Moving transporter and NPCS
            transform.Translate(dir * Vector2.up * speed * Time.deltaTime);
            foreach (GameObject npc in NPC_Moving)
            {
                npc.transform.Translate(dir * Vector2.up * speed * Time.deltaTime);
            }

            //Check if arrived
            if (transform.position.y >= 29.7f || transform.position.y <= 0f)
            {
                rest = true;
                foreach (GameObject npc in NPC_Moving)
                {
                    npc.GetComponent<NPC>().waiting = false;
                }
                NPC_Moving.Clear();
            }
        }                       
    }

    public void AddWaitingNPC(GameObject npc, int floor)
    {
        if(floor == 1)
        {
            NPC_Up.Add(npc);
        }
        else
        {
            NPC_Down.Add(npc);
        }
    }


    //Select 3 waiting NPCs to move
    void SelectNPC()
    {
        if (dir == 1)
        {
            int i = 0;
            while(NPC_Up.Count>0 && i < 3)
            {
                GameObject npc = NPC_Up[0];
                NPC_Moving.Add(npc);
                NPC_Up.Remove(npc);
                i++;
            }
        }
        else
        {
            int i = 0;
            while (NPC_Down.Count > 0 && i < 3)
            {
                GameObject npc = NPC_Down[0];
                NPC_Moving.Add(npc);
                NPC_Down.Remove(npc);
                i++;
            }
        }
    }
}

    

