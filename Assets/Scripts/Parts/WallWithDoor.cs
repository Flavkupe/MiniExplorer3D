using UnityEngine;
using System.Collections;

public class WallWithDoor : ClosingWall
{
    public GameObject ClosedSet;
    public GameObject OpenSet;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void SwitchStance(bool modified)
    {
        if (modified)
        {
            this.ClosedSet.SetActive(true);
            this.OpenSet.SetActive(false);
        }
        else
        {
            this.ClosedSet.SetActive(false);
            this.OpenSet.SetActive(true);
        }
    } 
}
