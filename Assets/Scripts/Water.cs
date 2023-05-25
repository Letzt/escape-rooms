using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    private Level level;
    public static bool playredclip = false;
    public static bool entered_red_recently = false;


    void Start()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Claire")
        {
            playredclip = true;
            level.player_is_on_water = true;
            if(!Level.visited_reds.Contains(gameObject))
            {
              level.energy -= 10;
              entered_red_recently = true;
              Level.visited_reds.Add(gameObject);
            }
            Debug.Log(level.energy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Claire")
        {
            level.player_is_on_water = false;
        }
    }
}
