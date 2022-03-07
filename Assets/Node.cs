using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{

    public int floor;    // 1st or 2nd
    public bool obstacle;
    public bool transporter;
    public bool NPC_onThis;

    // World position of center of the node
    public Vector3 pos;  

    // index on grid
    public int indexX;   
    public int indexY;

    // values for path-finding
    public float[] g;
    public float[] h;
   
    //Array to help retrace paths for each NPC
    public Node[] parent;  

    public Node(Vector3 p, int fl, int x, int y)
    {
        pos = p;
        floor = fl;
        obstacle = false;
        transporter = false;
        NPC_onThis = false;
        indexX = x;
        indexY = y;
    }

   


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
