using UnityEngine;

public class OscillateScale : MonoBehaviour
{
    public float Magnitude = 1.0f;

    public float Speed = 1.0f;

    public float ScaleFactor = 0.5f;

    private float period = 0.0f;

    

    private Vector3 _localScale;
    private void Start()
    {
        _localScale = transform.localScale;

    }

    private void Update()
    {
        this.period += Time.deltaTime * Speed;
        this.period = this.period % (2 * Mathf.PI);
        float t = Mathf.Abs(Mathf.Cos(period)); 
        float scaleFactor = 0.5f + ScaleFactor * t; // Remaps [0,1] to [0.5,1.0]
        this.transform.localScale = _localScale * scaleFactor * Magnitude;
    }
}