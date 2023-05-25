using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public GameObject claire;
    public GameObject ghost;
    public GameObject crystal1;
    public GameObject crystal2;
    public GameObject crystal3;
    public GameObject levelLost;
    public static bool playghostclip = false;

    public int ghost_count;
    public bool flag = true;
    public Vector3 origin;

    void Start()
    {
        ghost = GameObject.Find("Ghost");
        claire = GameObject.Find("Claire");
        crystal1 = GameObject.Find("Crystal1");
        crystal2 = GameObject.Find("Crystal2");
        crystal3 = GameObject.Find("Crystal3");
        levelLost = GameObject.Find("LostMenu");

        origin = new Vector3(0, -1.5f, 0);
        ghost.transform.position = origin;

        if(SceneChange.level_number >= 3)
        {
            ghost_count = SceneChange.level_number;
        }
        else
        {
            crystal1.SetActive(false);
            crystal2.SetActive(false);
            crystal3.SetActive(false);
            ghost_count = 0;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        ghost.transform.position = origin;
        if (collision.name == "Claire" && !LifeSaver.lifesaved)
        {
            playghostclip = true;
            if(crystal3 != null)
            {
                Destroy(crystal3);
                crystal3 = null;
            }
            else if(crystal2 != null)
            {
                Destroy(crystal2);
                crystal2 = null;
            }
            else if(crystal1 != null)
            {
                Destroy(crystal1);
                crystal1 = null;
            }
            else
            {
                levelLost.gameObject.SetActive(true);
            }
        }
    }


    void Appear()
    {
        Debug.Log("ghost count" + ghost_count);
        if(ghost_count > 0)
        {
          flag = true;
          ghost_count--;
        }
    }

    void Update()
    {
        if(ghost_count > 0)
        {
            while(flag)
            {
                Vector3 rad = new Vector3(Random.Range(-20.0F, 20.0F), Random.Range(-20.0F, 20.0F), Random.Range(-20.0F, 20.0F));
                ghost.transform.position = claire.transform.position + rad;
                flag = false;
                Invoke("Appear", 7);
            }

            Vector3 movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
            movement_direction = claire.transform.position - transform.position;
            movement_direction = movement_direction / Vector3.Magnitude(movement_direction);
            transform.Translate(movement_direction * 3 * Time.deltaTime);
        }
    }
}
