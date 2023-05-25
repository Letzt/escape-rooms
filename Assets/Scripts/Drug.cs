using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drug : MonoBehaviour
{
    private Level level;
	  private Virus virus;
    public static bool playgreenclip = false;
    public static bool entered_green_recently = false;

    void Start()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
		    virus = null;
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
    }

    void setSpeedBack()
    {
        virus.SetSpeed(1);
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Claire")
        {
            playgreenclip = true;
            Claire2.player_on_green = true;
            if(!Level.visited_greens.Contains(gameObject))
            {
              level.energy += 10;
              entered_green_recently = true;
              Level.visited_greens.Add(gameObject);
            }
            Debug.Log(level.energy);
        }

        if (other.gameObject.name == "COVID")
        {
			       virus = other.gameObject.GetComponent<Virus>();
			       virus.SetSpeed(-1);
             Invoke("setSpeedBack", 3);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Claire")
        {
            Claire2.player_on_green = false;
        }
        if (other.gameObject.name == "COVID")
        {
          
        }
    }
}
