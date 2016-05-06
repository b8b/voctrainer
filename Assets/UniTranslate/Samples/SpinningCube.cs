using UnityEngine;

public class SpinningCube : MonoBehaviour
{
    [SerializeField] private Vector3 addedRotation;
    
	// Update is called once per frame
	private void Update ()
	{
	    transform.Rotate(addedRotation * Time.deltaTime);
	}
}
