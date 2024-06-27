using UnityEngine;

public class Portal : MonoBehaviour
{
    public string room;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.transform.position = Vector3.zero;

            FindObjectOfType<WebSocketManager>().LoadScene(room);
        }
    }
}