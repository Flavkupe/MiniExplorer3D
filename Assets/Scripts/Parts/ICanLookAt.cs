using UnityEngine;

public interface ICanLookAt : IHasName
{
    void LookAt(GameObject source);    
}

public interface ICanLookAtAndInteract : ICanLookAt
{
    bool InteractWith(GameObject source, KeyCode key);
}



