using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeSaver : MonoBehaviour
{
    public Vector3 origin;


    public static bool lifesaved;

    void Start()
    {
        origin = new Vector3(0, 0, 0);
        if(SceneChange.level_number >= 3)
        {
            Vector3 rad = new Vector3(Random.Range(-25.0F, 25.0F), 2.0F, Random.Range(-25.0F, 25.0F));
            transform.position = origin + rad;
        }
        else
        {
            transform.position = new Vector3(0, -5.0f, 0);
        }
        lifesaved = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.name == "Claire")
        {
          lifesaved = true;
          Destroy(gameObject);
        }
    }
}
