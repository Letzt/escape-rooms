using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Claire2 : MonoBehaviour {

    private Animator animation_controller;
    private CharacterController character_controller;
    public Vector3 movement_direction;
    public float walking_velocity;
    public static float velocity;
    public bool threshold_reached;
	  public bool is_jumping;
    public static int num_lives;
    public static bool has_won;
    public static bool player_on_green;
    public Text text;
    float f;

	  void Start ()
    {
        animation_controller = GetComponent<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        walking_velocity = 10f;
        velocity = 0.0f;
        num_lives = 5;
        has_won = false;
		    is_jumping = false;
        player_on_green = false;
        threshold_reached = false;
    }



	  private void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

	void ChangeScene(){
		if(has_won){
			SceneManager.LoadScene("WonMenu");
		}else{
			SceneManager.LoadScene("LostMenu");
		}

	}

	void ChangeJumpVar(){
		is_jumping = false;
	}

  void RecentGreen(){
    Drug.entered_green_recently = false;
  }

  void RecentRed(){
    Water.entered_red_recently = false;
  }

  void EnergyChange()
  {
    Virus.energychanged = false;
  }

    void Update()
    {

    if(Drug.entered_green_recently){
      text.text = "+10";
      Invoke("RecentGreen", 2);
    }else if(Water.entered_red_recently){
      text.text = "-10";
      Invoke("RecentRed", 2);
    }
    else if(Virus.energychanged)
    {
      text.text = "-" + Virus.energydiff;
      Invoke("EnergyChange", 2);
    }
    else{
      text.text = "";
    }

		bool isUpArrow = Input.GetKey(KeyCode.UpArrow);
		bool isDownArrow = Input.GetKey(KeyCode.DownArrow);
		bool isLeftArrow = Input.GetKey(KeyCode.LeftArrow);
		bool isRightArrow = Input.GetKey(KeyCode.RightArrow);
		bool isCtrl = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);
		bool isShift = Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift);
		bool isSpace = Input.GetKey(KeyCode.Space);
		float desired_velocity = 0.0f;

		if(has_won){
			animation_controller.SetBool("WalkForward", false);
			animation_controller.SetBool("WalkBackward", false);
			animation_controller.SetBool("CrouchForward", false);
			animation_controller.SetBool("CrouchBackward", false);
			animation_controller.SetBool("Run", false);
			animation_controller.SetBool("Jump", false);
			animation_controller.SetBool("Dead", false);
			velocity = 0.0f;
			Invoke("ChangeScene", 2);
			return;
		}

		if(num_lives <= 0){
			animation_controller.SetBool("WalkForward", false);
			animation_controller.SetBool("WalkBackward", false);
			animation_controller.SetBool("CrouchForward", false);
			animation_controller.SetBool("CrouchBackward", false);
			animation_controller.SetBool("Run", false);
			animation_controller.SetBool("Jump", false);
			animation_controller.SetBool("Dead", true);
			velocity = 0.0f;
			Invoke("ChangeScene", Convert.ToSingle(4.3));
			return;
		}


		animation_controller.SetBool("WalkForward", isUpArrow && !isCtrl && !isShift && !threshold_reached);
		animation_controller.SetBool("WalkBackward", isDownArrow && !isCtrl);
		animation_controller.SetBool("CrouchForward", isUpArrow && isCtrl);
		animation_controller.SetBool("CrouchBackward", isDownArrow && isCtrl);
		animation_controller.SetBool("Run", isUpArrow && threshold_reached);
		animation_controller.SetBool("Jump", isSpace);

		int flag = 0;

    if(!isUpArrow){
      threshold_reached = false;
    }

		if(animation_controller.GetBool("WalkForward")){
			flag = 1;
			desired_velocity = walking_velocity;
			if(velocity <= desired_velocity){
				velocity += 0.01f;
			}

      if(velocity >= 5f){
          threshold_reached = true;
      }
		}

    if(animation_controller.GetBool("Run")){
      flag = 1;
      desired_velocity = walking_velocity;
      if(velocity <= desired_velocity){
        velocity += 0.01f;
      }
      if(velocity > desired_velocity){
        velocity -= 0.01f;
      }
    }

		if(animation_controller.GetBool("WalkBackward")){
			flag = 1;
			desired_velocity = -1*Convert.ToSingle(walking_velocity/1.5);
			if(velocity >= desired_velocity){
				velocity -= 0.5f;
			}
			if(velocity < desired_velocity){
				velocity += 0.5f;
			}
		}

		if(animation_controller.GetBool("CrouchForward")){
			flag = 1;
			desired_velocity = Convert.ToSingle(walking_velocity/2.0);
			if(velocity <= desired_velocity){
				velocity += 0.5f;
			}
			if(velocity > desired_velocity){
				velocity -= 0.5f;
			}
		}

		if(animation_controller.GetBool("CrouchBackward")){
			flag = 1;
			desired_velocity = -1*Convert.ToSingle(walking_velocity/2.0);
			if(velocity >= desired_velocity){
				velocity -= 0.5f;
			}
			if(velocity < desired_velocity){
				velocity += 0.5f;
			}
		}

		if(animation_controller.GetBool("Jump")){
			flag = 1;
			desired_velocity = Convert.ToSingle(walking_velocity*3.0);
			if(velocity <= desired_velocity){
				velocity += 25f;
			}
			if(velocity > desired_velocity){
				velocity -= 25f;
			}
			is_jumping = true;
			Invoke("ChangeJumpVar", Convert.ToSingle(2.2));
		}

		if((isLeftArrow || isRightArrow) && !is_jumping){
			transform.Rotate(0f, Input.GetAxis("Horizontal")*3 ,0f);
		}

		if(flag == 0) velocity = 0.0f;

        bool is_crouching = false;
        if ( (animation_controller.GetCurrentAnimatorStateInfo(0).IsName("CrouchForward"))
         ||  (animation_controller.GetCurrentAnimatorStateInfo(0).IsName("CrouchBackward")) )
        {
            is_crouching = true;
        }

        if (is_crouching)
        {
            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, 0.0f, GetComponent<CapsuleCollider>().center.z);
        }
        else
        {
            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, 0.9f, GetComponent<CapsuleCollider>().center.z);
        }

        float xdirection = Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
        float zdirection = Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
        movement_direction = new Vector3(xdirection, 0.0f, zdirection);

        if (transform.position.y > 0.0f)
        {
            Vector3 lower_character = movement_direction * velocity * Time.deltaTime;
            lower_character.y = -100.0f;
            character_controller.Move(lower_character);
        }
        else
        {
            character_controller.Move(movement_direction * velocity * Time.deltaTime);
        }



    }
}
