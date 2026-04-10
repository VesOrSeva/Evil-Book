using UnityEngine;

public class Spinner : MonoBehaviour // SPIIIINNNNN!!!
{
    [SerializeField] Vector3 axis = Vector3.up;
    [SerializeField] float speed = 90f;
    [SerializeField] Space space = Space.Self;

    private void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime, space);
    }
}