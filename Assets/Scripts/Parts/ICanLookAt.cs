using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface ICanLookAt
{
    void LookAt(GameObject source);    
}

public interface ICanLookAtAndInteract : ICanLookAt
{
    bool InteractWith(GameObject source, KeyCode key);
}



