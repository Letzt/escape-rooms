using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private Level level;
    public static bool playtouchclip = false;

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
		if(other.name == "Claire"){
      playtouchclip = true;
			if(Level.curr_block_1 == null){
				Level.curr_block_1 = this.gameObject;
			}else if(Level.curr_block_2 == null && this.gameObject != Level.curr_block_1){
				Level.curr_block_2 = this.gameObject;
			}
		}
    }
}
