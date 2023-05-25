using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virus : MonoBehaviour
{
    private Level level;
    private float radius_of_search_for_player;
    public float virus_speed;
	  public GameObject claire;
    public bool entered;
    public static bool zombieclipplay = false;
    public static bool energychanged = false;
    public static float energydiff;
	System.Random rand = new System.Random();


	void Start ()
    {
        entered = false;
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        Bounds bounds = level.GetComponent<Collider>().bounds;
        radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 15.0f;
		claire = GameObject.Find("Claire");
		int min = 1;
        int max = 4;
        int range = max - min;

		float sample = (float)rand.NextDouble();
        float scaled = (sample * range) + min;
		virus_speed = scaled;
    }

	public void SetSpeed(float s){
		virus_speed = s;
	}

	public float GetSpeed(){
		return virus_speed;
	}

    void Update()
    {
        if (level.player_entered_house)
            return;
        Color redness = new Color
        {
            r = Mathf.Max(1.0f, 0.25f + Mathf.Abs(Mathf.Sin(2.0f * Time.time)))
        };
        transform.localScale = new Vector3(
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)),
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)),
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time))
                               );
		float playerDistance = Vector3.Distance(claire.transform.position, transform.position);
		Vector3 movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
		if(playerDistance < radius_of_search_for_player){
				movement_direction = claire.transform.position - transform.position;
				movement_direction = movement_direction / Vector3.Magnitude(movement_direction);
				float x = movement_direction.x;
				float z = movement_direction.z;
				movement_direction = new Vector3(x, 0.0f, z);

		}else{

		}
		transform.Translate(movement_direction * virus_speed * Time.deltaTime);


		if(!level.player_is_on_water){
		}else{
		}

		if(transform.position.y > 1.5f){
			transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
		}

    }

     void setSpeedBack()
     {
         virus_speed = 1;
         Debug.Log("Set Back");
     }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Claire" && !Claire2.player_on_green)
        {
            zombieclipplay = true;
            energychanged = true;
            float old_energy = level.energy;
            if(Claire2.velocity >= 5f)
            {
              Debug.Log("hell no");

              if(level.energy <= 30)
                level.energy -= 7;
              else if(level.energy <= 60)
                level.energy -= 15;
              else
                level.energy -= 25;
              if (!level.virus_landed_on_player_recently)
                  level.timestamp_virus_landed = Time.time;
              level.num_virus_hit_concurrently++;
              level.virus_landed_on_player_recently = true;
              Destroy(gameObject);
            }
            else{
                Debug.Log("hell yea");

                if(level.energy <= 30)
                  level.energy -= 5;
                else if(level.energy <= 60)
                  level.energy -= 10;
                else
                  level.energy -= 15;

                  virus_speed = -1;
                  Invoke("setSpeedBack", 3);
              }
              energydiff = old_energy - level.energy;
            }
    }

}
