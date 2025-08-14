using UnityEngine;
public class movement : MonoBehaviour
{
    private Vector3 blickrichtung;

    private float speed = 3f;
    public float rotationSpeed = 90f;
    public GameObject winScreen;
    private bool ended = false;
    public static bool aktive = true;

    private void Update()
    {
        if(aktive){
            if (!ended)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    blickrichtung = gameObject.transform.forward;
                    transform.position += blickrichtung * speed * Time.deltaTime;
                }

                if (Input.GetKey(KeyCode.S))
                {
                    blickrichtung = gameObject.transform.forward;
                    transform.position += blickrichtung * speed * Time.deltaTime * -1;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime * -1);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!ended)
        {
            if (collision.gameObject.name == "Sphere")
            {
                winScreen.SetActive(true);
                ended = true;
            }
        }
    }

    public void setEnded(bool value)
    {
        ended = value;
    }
}
