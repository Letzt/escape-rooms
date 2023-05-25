using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;

public enum TileType
{
    WALL = 0,
    FLOOR = 1,
    WATER = 2,
    DRUG = 3,
    VIRUS = 4,
	  BLOCK = 5,
}

struct ObjectValues
{
    public Color c;
  	public float degrees;
  	public float amp;
  	public float freq;
  	public Vector3 posOffset;
}

public class Level : MonoBehaviour
{

      public SceneChange sc;
      // fields/variables you may adjust from Unity's interface
      public int width = 16;   // size of level (default 16 x 16 blocks)
      public int length = 16;
      public float storey_height = 2.5f;   // height of walls
      public float virus_speed = 2.0f;     // virus velocity
      //public GameObject fps_prefab;        // these should be set to prefabs as provided in the starter scene

  	  public GameObject claire;
      public GameObject virus_prefab;
      public GameObject water_prefab;
  	  public GameObject water_prefab2;

      public GameObject sword;

      // public GameObject crystal1;
      // public GameObject crystal2;
      // public GameObject crystal3;

      // public GameObject house_prefab;
      // public GameObject text_box;
      public GameObject scroll_bar;
  	  public int recursion_count = 0;


      //Audios
      public AudioSource bgmClip;
  	  public AudioSource greenClip;
	    public AudioSource redClip;
    	public AudioSource zombieClip;
    	public AudioSource ghostClip;
      public AudioSource touchClip;
      public AudioSource poofClip;

    	public int enteredHouse = 0;
    	public int virusLanded = 0;
    	public int playerOnWater = 0;
    	public int blocknum;
    	public System.Random rnd = new System.Random();

    	public static GameObject curr_block_1;
    	public static GameObject curr_block_2;
    	Dictionary < GameObject, ObjectValues > map = new Dictionary < GameObject, ObjectValues > ();
    	double min, max, range, sample, scaled;
      float f;
    	List<GameObject> list_of_objects;
    	List<PrimitiveType> list_of_primitives;
    	System.Random rand = new System.Random();
    	HashSet<GameObject> deleted;
    	public bool revertCalled = false;
    	public bool destroyCalled = false;


    	List<TileType>[,] grid;
    	//static List<TileType>[,] old_grid;
    	public bool drawn = false;

      // fields/variables accessible from other scripts
      //internal GameObject fps_player_obj;   // instance of FPS template
      internal float energy;
      public Text energydisplay;
      // internal float player_health = 1.0f;  // player health in range [0.0, 1.0]
      internal int num_virus_hit_concurrently = 0;            // how many viruses hit the player before washing them off
      internal bool virus_landed_on_player_recently = false;  // has virus hit the player? if yes, a timer of 5sec starts before infection
      internal float timestamp_virus_landed = float.MaxValue; // timestamp to check how many sec passed since the virus landed on player
      internal bool drug_landed_on_player_recently = false;   // has drug collided with player?
      internal bool player_is_on_water = false;               // is player on water block
      internal bool player_entered_house = false;             // has player arrived in house?
  	  internal int old_num_virus_hit_concurrently = 0;

      // fields/variables needed only from this script
      private Bounds bounds;                   // size of ground plane in world space coordinates
      private float timestamp_last_msg = 0.0f; // timestamp used to record when last message on GUI happened (after 7 sec, default msg appears)
      private int function_calls = 0;          // number of function calls during backtracking for solving the CSP
      private int num_viruses = 0;             // number of viruses in the level
      private List<int[]> pos_viruses;         // stores their location in the grid



      //NPC2 object
      // public GameObject ghost;

      //buttons and panels
      public GameObject levelLost;
      public GameObject pauseMenu;
      public GameObject levelUpMenu;
      public GameObject GameOverMenu;

      List<GameObject> list_of_greens;
      public static HashSet<GameObject> visited_greens;

      List<GameObject> list_of_reds;
      public static HashSet<GameObject> visited_reds;


    // feel free to put more fields here, if you need them e.g, add AudioClips that you can also reference them from other scripts
    // for sound, make also sure that you have ONE audio listener active (either the listener in the FPS or the main camera, switch accordingly)

    // a helper function that randomly shuffles the elements of a list (useful to randomize the solution to the CSP)
    private void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

	void EndMenuScene(){
		//EndMenuScript.winClip.Play();
		//SceneManager.LoadScene("EndMenu");
	}

    // Use this for initialization
    void Start()
    {

      bgmClip.Play();


      //Popups
      levelLost = GameObject.Find("LostMenu");
      levelLost.gameObject.SetActive(false);

      pauseMenu = GameObject.Find("PauseMenu");
      pauseMenu.gameObject.SetActive(false);

      levelUpMenu = GameObject.Find("LevelUpMenu");
      levelUpMenu.gameObject.SetActive(false);

      GameOverMenu = GameObject.Find("GameOverMenu");
      GameOverMenu.gameObject.SetActive(false);

      sword = GameObject.Find("Sword");
      sword.gameObject.SetActive(false);

      // ghost = GameObject.Find("Ghost");
      // ghost.gameObject.SetActive(false);

		//Debug.Log("Start Called !");
		if(SceneChange.start_menu_loaded == false){
			SceneChange.start_menu_loaded = true;
			//SceneManager.LoadScene("StartMenu");
			//Debug.Log("StartMenu Loaded !!!!!!!!!!");
		}
		// Debug.Log("Calling start");

        // initialize internal/private variables

        bounds = GetComponent<Collider>().bounds;
        // Debug.Log("bounds"+ bounds.max);
        // Debug.Log("bounds"+ bounds.size.z);
		//bounds.Expand(10);
        timestamp_last_msg = 0.0f;
        function_calls = 0;
        num_viruses = 0;
        energy = 100;
        num_virus_hit_concurrently = 0;
        virus_landed_on_player_recently = false;
        timestamp_virus_landed = float.MaxValue;
        drug_landed_on_player_recently = false;
        player_is_on_water = false;
        player_entered_house = false;
        enteredHouse = 0;
        virusLanded = 0;
        playerOnWater = 0;
		drawn = false;
		old_num_virus_hit_concurrently = 0;
		// Debug.Log("Level : " + SceneChange.level_number);
		blocknum = 2 * (SceneChange.level_number+1);
		//System.Convert.ToInt32(System.Math.Pow(2, SceneChange.level_number+1));

		curr_block_1 = null;
		curr_block_2 = null;
		deleted = new HashSet<GameObject>();
		revertCalled = false;
		destroyCalled = false;

    //green and red
    visited_greens = new HashSet<GameObject>();
    visited_reds = new HashSet<GameObject>();

		list_of_primitives = new List<PrimitiveType>{PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Capsule, PrimitiveType.Cylinder};


		//SceneChange.start_menu_loaded = false;

        // initialize 2D grid
        //List<TileType>[,] grid = new List<TileType>[width, length];
		grid = new List<TileType>[width, length];
        // useful to keep variables that are unassigned so far
        List<int[]> unassigned = new List<int[]>();

        // will place x viruses in the beginning (at least 1). x depends on the sise of the grid (the bigger, the more viruses)
        num_viruses = width * length / 25 + 1; // at least one virus will be added
        pos_viruses = new List<int[]>();
        // create the wall perimeter of the level, and let the interior as unassigned
        // then try to assign variables to satisfy all constraints
        // *rarely* it might be impossible to satisfy all constraints due to initialization
        // in this case of no success, we'll restart the random initialization and try to re-solve the CSP
        bool success = false;
        while (!success)
        {
            for (int v = 0; v < num_viruses; v++)
            {
                while (true) // try until virus placement is successful (unlikely that there will no places)
                {
                    // try a random location in the grid
                    int wr = Random.Range(1, width - 1);
                    int lr = Random.Range(1, length - 1);

                    // if grid location is empty/free, place it there
                    if (grid[wr, lr] == null)
                    {
                        grid[wr, lr] = new List<TileType> { TileType.VIRUS };
                        pos_viruses.Add(new int[2] { wr, lr });
                        break;
                    }
                }
            }

            for (int w = 0; w < width; w++)
                for (int l = 0; l < length; l++)
                    if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                        grid[w, l] = new List<TileType> { TileType.WALL };
                    else
                    {
                        if (grid[w, l] == null) // does not have virus already or some other assignment from previous run
                        {
                            // CSP will involve assigning variables to one of the following four values (VIRUS is predefined for some tiles)
                            List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR, TileType.WATER, TileType.DRUG, TileType.BLOCK };
                            Shuffle<TileType>(ref candidate_assignments);

                            grid[w, l] = candidate_assignments;
                            unassigned.Add(new int[] { w, l });
                        }
                    }

            // YOU MUST IMPLEMENT this function!!!
			//Debug.Log("BackTrackSearch called !");
			/*
            success = BackTrackingSearch(grid, unassigned);
			Debug.Log("BackTrackSearch done !");
            if (!success)
            {
                Debug.Log("Could not find valid solution - will try again");
                unassigned.Clear();
                grid = new List<TileType>[width, length];
                function_calls = 0;
            }*/
			success = true;
        }

		for (int w = 1; w < width-1; w++){
            for (int l = 1; l < length-1; l++){
				if(grid[w, l][0] == TileType.WALL || grid[w, l][0] == TileType.WATER || grid[w, l][0] == TileType.VIRUS || grid[w, l][0] == TileType.DRUG || grid[w, l][0] == TileType.BLOCK){
					grid[w, l][0] = TileType.FLOOR;
				}
			}
		}

		List<int[]> list_of_coordinates = new List<int[]>();
		for(int w = 1; w < width-1; w++){
			for(int l = 1; l < length-1; l++){
				list_of_coordinates.Add(new int[2] { w, l });
			}
		}
		Shuffle<int[]>(ref list_of_coordinates);

		//WATER
		//DRUG
		//VIRUS

		int num_virus = SceneChange.level_number *10;
		// Debug.Log("Num Virus : " + num_virus);

		for(int i=0; i<30; i++){
			int w = list_of_coordinates[i][0];
			int l = list_of_coordinates[i][1];
			grid[w, l][0] = TileType.WATER;
		}
		for(int i=30; i<60; i++){
			int w = list_of_coordinates[i][0];
			int l = list_of_coordinates[i][1];
			grid[w, l][0] = TileType.DRUG;
		}
		for(int i=60; i<60+num_virus; i++){
			int w = list_of_coordinates[i][0];
			int l = list_of_coordinates[i][1];
			grid[w, l][0] = TileType.VIRUS;
		}
		for(int i=60+num_virus; i<60+num_virus+blocknum; i++){
			int w = list_of_coordinates[i][0];
			int l = list_of_coordinates[i][1];
			grid[w, l][0] = TileType.BLOCK;
		}

    //Final Grid is already

    // if (SceneChange.isRetrying){
		// 	Debug.Log("Assigning old one");
		// 	grid = SceneChange.old_grid;
		// }

		// SceneChange.old_grid = grid;

    DrawDungeon(grid);



		/*
		list_of_objects = new List<GameObject> { GameObject.Find("Block1"), GameObject.Find("Block2"), GameObject.Find("Block3"), GameObject.Find("Block4"), GameObject.Find("Block5"), GameObject.Find("Block6") };
        Shuffle<GameObject>(ref list_of_objects);
		*/

    list_of_greens = new List<GameObject>();
    for(int i = 0; i < 30; i++){
			list_of_greens.Add(GameObject.Find("Green" + i.ToString()));
		}

    list_of_reds = new List<GameObject>();
    for(int i = 0; i < 30; i++){
			list_of_reds.Add(GameObject.Find("Red" + i.ToString()));
		}


		list_of_objects = new List<GameObject>();

		for(int i = 0; i < blocknum; i++){
			list_of_objects.Add(GameObject.Find("Block" + i.ToString()));
		}

		Shuffle<GameObject>(ref list_of_objects);

		//List<Color> list_of_colors = new List<Color> { Color.black, Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.white, Color.yellow, new Color(1.0f, 0.64f, 0.0f)};
		/*
		List<Color> list_of_colors = new List<Color> { Color.black, Color.blue, Color.cyan};
		*/

		List<Color> list_of_colors = new List<Color>();

		for(int i = 0; i < blocknum/2; i++){
			Color randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			list_of_colors.Add(randomColor);
		}

        Shuffle<Color>(ref list_of_colors);

		min = 0;
        max = 90;
        range = max - min;
        List<float> degreesPerSecondArr = new List<float>(6);
        for (int i = 0; i < blocknum; i++)
        {
			      list_of_objects[i].GetComponent<Renderer>().material.color = Color.grey;
            sample = rand.NextDouble();
            scaled = (sample * range) + min;
            degreesPerSecondArr.Add((float)scaled);
        }

        min = 1;
        max = 5;
        range = max - min;
        List<float> amplitudeArr = new List<float>(6);
        for (int i = 0; i < blocknum; i++)
        {
            sample = rand.NextDouble();
            scaled = (sample * range) + min;
            amplitudeArr.Add((float)scaled);
        }

        min = 1;
        max = 4;
        range = max - min;
        List<float> frequencyArr = new List<float>(6);
        for (int i = 0; i < blocknum; i++)
        {
            sample = rand.NextDouble();
            scaled = (sample * range) + min;
            frequencyArr.Add((float)scaled);
        }

		for(int i=0; i<blocknum; i++){
			ObjectValues ov = new ObjectValues();
			ov.c = list_of_colors[i%(blocknum/2)];
			ov.degrees = degreesPerSecondArr[i];
			ov.amp = amplitudeArr[i];
			ov.freq = frequencyArr[i];
			ov.posOffset = list_of_objects[i].transform.position;
			map[list_of_objects[i]] = ov;
		}
    }

    // one type of constraint already implemented for you
    bool DoWeHaveTooManyInteriorWallsORWaterORDrug(List<TileType>[,] grid)
    {
        int[] number_of_assigned_elements = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    number_of_assigned_elements[(int)grid[w, l][0]]++;
            }

        if ((number_of_assigned_elements[(int)TileType.WALL] > num_viruses * 10) ||
             (number_of_assigned_elements[(int)TileType.WATER] > (width + length) / 2) ||
             (number_of_assigned_elements[(int)TileType.DRUG] >= num_viruses / 2))
            return true;
        else
            return false;
    }

    // another type of constraint already implemented for you
    bool DoWeHaveTooFewWallsORWaterORDrug(List<TileType>[,] grid)
    {
        int[] number_of_potential_assignments = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                for (int i = 0; i < grid[w, l].Count; i++)
                    number_of_potential_assignments[(int)grid[w, l][i]]++;
            }

        if ((number_of_potential_assignments[(int)TileType.WALL] < (width * length) / 4) ||
             (number_of_potential_assignments[(int)TileType.WATER] < num_viruses / 4) ||
             (number_of_potential_assignments[(int)TileType.DRUG] < num_viruses / 4))
            return true;
        else
            return false;
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there are three (or more) interior consecutive wall blocks either horizontally or vertically
    // by interior, we mean walls that do not belong to the perimeter of the grid
    // e.g., a grid configuration: "FLOOR - WALL - WALL - WALL - FLOOR" is not valid
    bool TooLongWall(List<TileType>[,] grid)
    {
        /*** implement the rest ! */
		for (int w = 1; w < width-1; w++){
			for (int l = 1; l < length-3; l++){
				if(grid[w,l].Count == 1 && grid[w,l][0] == TileType.WALL && grid[w,l+1].Count == 1 && grid[w,l+1][0] == TileType.WALL && grid[w,l+2].Count == 1 && grid[w,l+2][0] == TileType.WALL) return true;
			}
		}
		for (int l = 1; l < length-1; l++){
			for (int w = 1; w < width-3; w++){
				if(grid[w,l].Count == 1 && grid[w,l][0] == TileType.WALL && grid[w+1,l].Count == 1 && grid[w+1,l][0] == TileType.WALL && grid[w+2,l].Count == 1 && grid[w+2,l][0] == TileType.WALL) return true;
			}
		}
        return false;
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there is no WALL adjacent to a virus
    // adjacency means left, right, top, bottom, and *diagonal* blocks
    bool NoWallsCloseToVirus(List<TileType>[,] grid)
    {
        /*** implement the rest ! */
		int[] dirx = new int[] {-1, -1, -1, 0, 0, 1, 1, 1};
		int[] diry = new int[] {-1, 0, 1, -1, 1, -1, 0, 1};

		for (int w = 0; w < width; w++){
			for (int l = 0; l < length; l++){
				if(grid[w,l].Count == 1 && grid[w,l][0] == TileType.VIRUS){
					for(int i=0; i<8; i++){
						int newx = w+dirx[i];
						int newy = l+diry[i];
						if(newx >= 0 && newx < width && newy >=0 && newy < length && grid[newx,newy].Count == 1 && grid[newx,newy][0] == TileType.WALL) return false;
					}
				}
			}
		}
        return true;
    }


    // check if attempted assignment is consistent with the constraints or not
    bool CheckConsistency(List<TileType>[,] grid, int[] cell_pos, TileType t)
    {
        int w = cell_pos[0];
        int l = cell_pos[1];

        List<TileType> old_assignment = new List<TileType>();
        old_assignment.AddRange(grid[w, l]);
        grid[w, l] = new List<TileType> { t };

		// note that we negate the functions here i.e., check if we are consistent with the constraints we want
        bool areWeConsistent = !DoWeHaveTooFewWallsORWaterORDrug(grid) && !DoWeHaveTooManyInteriorWallsORWaterORDrug(grid)
                            && !TooLongWall(grid) && !NoWallsCloseToVirus(grid);

        grid[w, l] = new List<TileType>();
        grid[w, l].AddRange(old_assignment);
        return areWeConsistent;
    }


    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // implement backtracking
    bool BackTrackingSearch(List<TileType>[,] grid, List<int[]> unassigned)
    {
        // if there are too many recursive function evaluations, then backtracking has become too slow (or constraints cannot be satisfied)
        // to provide a reasonable amount of time to start the level, we put a limit on the total number of recursive calls
        // if the number of calls exceed the limit, then it's better to try a different initialization
        if (function_calls++ > 100000)
            return false;

        // we are done!
        if (unassigned.Count == 0)
            return true;

        /*** implement the rest ! */
        for(int i=0; i<unassigned.Count; i++){
			int w = unassigned[i][0];
			int l = unassigned[i][1];
			List<TileType> t = grid[w, l];
			int placed = 0;
			for(int j=0; j<4; j++){
				grid[w, l] = new List<TileType>{t[j]};
				int[] cpos = new int[] {w,l};
				if(CheckConsistency(grid, cpos, t[j])){
					placed = 1;
					break;
				}
			}
			if(placed == 0) return false;
		}
		//Debug.Log("BackTrackingSearch Completed !");
        return true;
    }

	void dfs(List<TileType>[,] solution, ref bool[,] visited2, int x, int y, int endX, int endY, ref bool exists){
		if(exists == true) return;
		if(x < 0 || x >= width || y < 0 || y >= length) return;
		if(visited2[x,y] == true) return;
		if(x == endX && y == endY){
			//Debug.Log("Path Exists !");
			exists = true;
			return;
		}
		if(solution[x,y][0] == TileType.WALL) return;
		visited2[x,y] = true;
		dfs(solution, ref visited2, x+1, y, endX, endY, ref exists);
		dfs(solution, ref visited2, x-1, y, endX, endY, ref exists);
		dfs(solution, ref visited2, x, y+1, endX, endY, ref exists);
		dfs(solution, ref visited2, x, y-1, endX, endY, ref exists);
		//dfs(solution, ref visited, x+1, y+1, endX, endY, ref exists);
		//dfs(solution, ref visited, x+1, y-1, endX, endY, ref exists);
		//dfs(solution, ref visited, x-1, y+1, endX, endY, ref exists);
		//dfs(solution, ref visited, x-1, y-1, endX, endY, ref exists);
	}

	bool pathExists(List<TileType>[,] solution, int startX, int startY, int endX, int endY){
		bool exists = false;
		bool[,] visited2 = new bool[width, length];
		for(int i=0; i<width; i++){
			for(int j=0; j<length; j++){
				visited2[i, j] = false;
			}
		}
		dfs(solution, ref visited2, startX, startY, endX, endY, ref exists);
		//if(exists == true) Debug.Log("Path existsss !");
		return exists;
	}

	void findPath(List<TileType>[,] solution, ref bool[,] visited3, int x, int y, int endX, int endY){
		//Debug.Log(recursion_count);
		recursion_count += 1;
		if(x < 0 || x >= width || y < 0 || y >= length) return;
		if(visited3[x,y]) return;
		if(x == endX && y == endY){
			/*if(curr_wall_coordinates.Count < final_wall_coordinates.Count){
				final_wall_coordinates = curr_wall_coordinates;
			}*/
			return;
		}
		visited3[x,y] = true;
		if(solution[x,y][0] == TileType.WALL){
			//curr_wall_coordinates.Add(new int[] { x, y });
		}
		findPath(solution, ref visited3, x+1, y, endX, endY);
		findPath(solution, ref visited3, x-1, y, endX, endY);
		findPath(solution, ref visited3, x, y+1, endX, endY);
		findPath(solution, ref visited3, x, y-1, endX, endY);
		visited3[x,y] = false;
	}

	List<int[]> getNeighbours(int[] curr_coordinates, int[,] print, ref bool[,] visited){
		//bool[,] visited = new bool[width, length];
		List<int[]> res = new List<int[]>{};
		Queue<int[]> q = new Queue<int[]>();
		visited[curr_coordinates[0], curr_coordinates[1]] = true;
		int newX = curr_coordinates[0]+1;
		int newY = curr_coordinates[1];
		if(newX >= 0 && newX < width && newY >=0 && newY < length && !visited[newX, newY]){
			 q.Enqueue(new int[] { newX, newY });
			 visited[newX, newY] = true;
		}
		newX = curr_coordinates[0]-1; newY = curr_coordinates[1];
		if(newX >= 0 && newX < width && newY >=0 && newY < length && !visited[newX, newY]){
			 q.Enqueue(new int[] { newX, newY });
			 visited[newX, newY] = true;
		}
		newX = curr_coordinates[0]; newY = curr_coordinates[1]-1;
		if(newX >= 0 && newX < width && newY >=0 && newY < length && !visited[newX, newY]){
			 q.Enqueue(new int[] { newX, newY });
			 visited[newX, newY] = true;
		}
		newX = curr_coordinates[0]; newY = curr_coordinates[1]+1;
		if(newX >= 0 && newX < width && newY >=0 && newY < length && !visited[newX, newY]){
			 q.Enqueue(new int[] { newX, newY });
			 visited[newX, newY] = true;
		}
		while(q.Count != 0){
			int[] curr = q.Dequeue();
			if(print[curr[0],curr[1]] == 1 || print[curr[0],curr[1]] == 3 || print[curr[0],curr[1]] == 2){
				res.Add(curr);
				continue;
			}
			newX = curr[0]+1;
			newY = curr[1];
			if(newX >= 0 && newX < width && newY >=0 && newY < length && !visited[newX, newY]){
				 q.Enqueue(new int[] { newX, newY });
				 visited[newX, newY] = true;
			}
			newX = curr[0]-1; newY = curr[1];
			if(newX >= 0 && newX < width && newY >=0 && newY < length && !visited[newX, newY]){
				 q.Enqueue(new int[] { newX, newY });
				 visited[newX, newY] = true;
			}
			newX = curr[0]; newY = curr[1]-1;
			if(newX >= 0 && newX < width && newY >=0 && newY < length && !visited[newX, newY]){
				 q.Enqueue(new int[] { newX, newY });
				 visited[newX, newY] = true;
			}
			newX = curr[0]; newY = curr[1]+1;
			if(newX >= 0 && newX < width && newY >=0 && newY < length && !visited[newX, newY]){
				 q.Enqueue(new int[] { newX, newY });
				 visited[newX, newY] = true;
			}
		}
		return res;
	}

	void makePath(ref List<TileType>[,] solution, ref int[,] print, int startX, int startY, int endX, int endY){
		//Debug.Log("start coords : " + startX + " " + startY);
		//Debug.Log("end coords : " + endX + " " + endY);
		bool[,] visited4 = new bool[width, length];
		for(int i=0; i<width; i++){
			for(int j=0; j<length; j++){
				visited4[i, j] = false;
			}
		}
		visited4[startX, startY] = true;
		Queue<int[]> q = new Queue<int[]>();
		q.Enqueue(new int[] { startX, startY });
		List<int>[,] parent = new List<int>[width, length];
		parent[startX, startY] = new List<int> {-1, -1};

		while(q.Count != 0){
			int[] curr_coordinates = q.Dequeue();
			if(curr_coordinates[0] == endX && curr_coordinates[1] == endY){
				break;
			}
			List<int[]> neighbours = getNeighbours(curr_coordinates, print, ref visited4);
			for(int i=0; i<neighbours.Count; i++){
				//if(neighbours[i][0] == endX && neighbours[i][1] == endY) Debug.Log("End found !");
				int w = neighbours[i][0];
				int l = neighbours[i][1];
				if (!(neighbours[i][0] == endX && neighbours[i][1] == endY)&&(w == 0 || l == 0 || w == width - 1 || l == length - 1))
                    continue;
				if(true/*!visited4[neighbours[i][0], neighbours[i][1]]*/){
					//visited4[neighbours[i][0], neighbours[i][1]] = true;
					q.Enqueue(neighbours[i]);
					//if(neighbours[i][0] == endX && neighbours[i][1] == endY) Debug.Log("End found 2 !");
					parent[neighbours[i][0], neighbours[i][1]] = new List<int> {curr_coordinates[0], curr_coordinates[1]};
				}
			}
		}

		List<int[]> final_wall_coordinates = new List<int[]>();
		List<int> curr_cord = parent[endX, endY];
		int currX = curr_cord[0];
		int currY = curr_cord[1];
		//Debug.Log("first :" + currX + " " + currY);
		while(!(currX == startX && currY == startY)){
			final_wall_coordinates.Add(new int[]{currX, currY});
			curr_cord = parent[currX, currY];
			currX = curr_cord[0];
			currY = curr_cord[1];
			//Debug.Log(currX + " " + currY);
		}

		for(int i=0; i<final_wall_coordinates.Count; i++){
			int x = final_wall_coordinates[i][0];
			int y = final_wall_coordinates[i][1];

			solution[x, y][0] = TileType.FLOOR;
			print[x, y] = 0;
		}


		/*List<int[]> curr_wall_coordinates = new List<int[]>();
		List<int[]> final_wall_coordinates = new List<int[]>();
		findPath(solution, ref visited, startX, startY, endX, endY);

		for(int i=0; i<final_wall_coordinates.Count; i++){
			int x = final_wall_coordinates[i][0];
			int y = final_wall_coordinates[i][1];
			solution[x, y][0] = TileType.FLOOR;
		}*/
	}


    // places the primitives/objects according to the grid assignents
    // you will need to edit this function (see below)
    void DrawDungeon(List<TileType>[,] solution)
    {
        GetComponent<Renderer>().material.color = Color.grey; // ground plane will be grey

        // place character at random position (wr, lr) in terms of grid coordinates (integers)
        // make sure that this random position is a FLOOR tile (not wall, drug, or virus)
        int wr = 0;
        int lr = 0;
        while (true) // try until a valid position is sampled
        {
            wr = Random.Range(1, width - 1);
            lr = Random.Range(1, length - 1);

			// if(SceneChange.isRetrying){
			// 	wr = SceneChange.old_startx;
			// 	lr = SceneChange.old_starty;
			// }

            if (solution[wr, lr][0] == TileType.FLOOR)
            {
                float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
                float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
				claire = GameObject.Find("Claire");
				claire.transform.position = new Vector3(x, 0.0f, z);
                //fps_player_obj = Instantiate(fps_prefab);
                //fps_player_obj.name = "PLAYER";
                // character is placed above the level so that in the beginning, he appears to fall down onto the maze
                //fps_player_obj.transform.position = new Vector3(x + 0.5f, 2.0f * storey_height, z + 0.5f);
				SceneChange.old_startx = wr;
				SceneChange.old_starty = lr;
                break;
            }
        }

        // place an exit from the maze at location (wee, lee) in terms of grid coordinates (integers)
        // destroy the wall segment there - the grid will be used to place a house
        // the exist will be placed as far as away from the character (yet, with some randomness, so that it's not always located at the corners)
        int max_dist = -1;
        int wee = -1;
        int lee = -1;
        while (true) // try until a valid position is sampled
        {
            if (wee != -1)
                break;
            for (int we = 0; we < width; we++)
            {
                for (int le = 0; le < length; le++)
                {
                    // skip corners
                    if (we == 0 && le == 0)
                        continue;
                    if (we == 0 && le == length - 1)
                        continue;
                    if (we == width - 1 && le == 0)
                        continue;
                    if (we == width - 1 && le == length - 1)
                        continue;

                    if (we == 0 || le == 0 || wee == length - 1 || lee == length - 1)
                    {
                        // randomize selection
                        if (Random.Range(0.0f, 1.0f) < 0.1f)
                        {
                            int dist = System.Math.Abs(wr - we) + System.Math.Abs(lr - le);
                            if (dist > max_dist) // must be placed far away from the player
                            {
                                wee = we;
                                lee = le;
                                max_dist = dist;
                            }
                        }
                    }
                }
            }
        }

		// if(SceneChange.isRetrying){
		// 	wee = SceneChange.old_endx;
		// 	lee = SceneChange.old_endy;
		// 	// SceneChange.isRetrying = false;
		// }

		SceneChange.old_endx = wee;
		SceneChange.old_endy = lee;


        // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION  ***
        // implement an algorithm that checks whether
        // all paths between the player at (wr,lr) and the exit (wee, lee)
        // are blocked by walls. i.e., there's no way to get to the exit!
        // if this is the case, you must guarantee that there is at least
        // one accessible path (any path) from the initial player position to the exit
        // by removing a few wall blocks (removing all of them is not acceptable!)
        // this is done as a post-processing step after the CSP solution.
        // It might be case that some constraints might be violated by this
        // post-processing step - this is OK.

        /*** implement what is described above ! */

		int startX = wr, startY = lr, endX = wee, endY = lee;

		int[,] print = new int[width, length];

		for(int x=0; x<width; x++){
			for(int y=0; y<length; y++){
				if(solution[x,y][0] == TileType.WALL){
					print[x,y] = 1;
				}else{
					print[x,y] = 0;
				}
			}
		}

		print[startX,startY] = 2;
		print[endX,endY] = 3;

		//Debug.Log(print);

		StringBuilder sb = new StringBuilder();
		for(int i=0; i< width; i++)
		{
			for(int j=0; j<length; j++)
			{
				sb.Append(print [i,j]);
				sb.Append(' ');
			}
			sb.AppendLine();
		}
		//Debug.Log(sb.ToString());

		//Debug.Log("pathExists called !");
		if(pathExists(solution, startX, startY, endX, endY) == false){
			//Debug.Log("makePath called !");
			makePath(ref solution, ref print, startX, startY, endX, endY);
			//makePath(ref solution, ref print, startX, startY, endX, endY);
			//makePath(ref solution, ref print, startX, startY, endX, endY);
		}

		for(int x=0; x<width; x++){
			for(int y=0; y<length; y++){
				if(solution[x,y][0] == TileType.WALL){
					print[x,y] = 1;
				}else{
					print[x,y] = 0;
				}
			}
		}

		print[startX,startY] = 2;
		print[endX,endY] = 3;

		//Debug.Log(print);

		sb = new StringBuilder();
		for(int i=0; i< width; i++)
		{
			for(int j=0; j<length; j++)
			{
				sb.Append(print [i,j]);
				sb.Append(' ');
			}
			sb.AppendLine();
		}
		//Debug.Log(sb.ToString());


        // the rest of the code creates the scenery based on the grid state
        // you don't need to modify this code (unless you want to replace the virus
        // or other prefabs with something else you like)
        int w = 0;
		    int num = 0;
        int green_num = 0;
        int red_num = 0;
        // int count = 0;
        for (float x = bounds.min[0]; x < bounds.max[0]; x += bounds.size[0] / (float)width - 1e-6f, w++)
        {
            int l = 0;
            for (float z = bounds.min[2]; z < bounds.max[2]; z += bounds.size[2] / (float)length - 1e-6f, l++)
            {
                if ((w >= width) || (l >= width))
                    continue;

                float y = bounds.min[1];
                //Debug.Log(w + " " + l + " " + h);
                // if ((w == wee) && (l == lee)) // this is the exit
                // {
                //     GameObject house = Instantiate(house_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                //     house.name = "HOUSE";
                //     house.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                //     if (l == 0)
                //         house.transform.Rotate(0.0f, 270.0f, 0.0f);
                //     else if (w == 0)
                //         house.transform.Rotate(0.0f, 0.0f, 0.0f);
                //     else if (l == length - 1)
                //         house.transform.Rotate(0.0f, 90.0f, 0.0f);
                //     else if (w == width - 1)
                //         house.transform.Rotate(0.0f, 180.0f, 0.0f);
                //
                //     house.AddComponent<BoxCollider>();
                //     house.GetComponent<BoxCollider>().isTrigger = true;
                //     house.GetComponent<BoxCollider>().size = new Vector3(3.0f, 3.0f, 3.0f);
                //     house.AddComponent<House>();
                // }
                if (solution[w, l][0] == TileType.WALL)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WALL";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y + storey_height / 2.0f, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
                }
                else if (solution[w, l][0] == TileType.VIRUS)
                {
                    GameObject virus = Instantiate(virus_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    virus.name = "COVID";
                    virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);

                    //GameObject virus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //virus.GetComponent<Renderer>().material.color = new Color(0.5f, 0.0f, 0.0f);
                    //virus.name = "ENEMY";
                    //virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    //virus.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    //virus.AddComponent<BoxCollider>();
                    //virus.GetComponent<BoxCollider>().size = new Vector3(1.2f, 1.2f, 1.2f);
                    //virus.AddComponent<Rigidbody>();
                    //virus.GetComponent<Rigidbody>().useGravity = false;

                    virus.AddComponent<Virus>();
                    virus.GetComponent<Rigidbody>().mass = 10000;
                }
                else if (solution[w, l][0] == TileType.DRUG)
                {
					/*
                    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    capsule.name = "DRUG";
                    capsule.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    capsule.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    capsule.GetComponent<Renderer>().material.color = Color.green;
                    capsule.AddComponent<Drug>();
					*/

					GameObject water = Instantiate(water_prefab2, new Vector3(0, 0, 0), Quaternion.identity);
                    water.name = "Green" + green_num.ToString();
                    water.transform.localScale = new Vector3(0.5f * bounds.size[0] / (float)width, 1.0f, 0.5f * bounds.size[2] / (float)length);
                    water.transform.position = new Vector3(x + 0.5f, y + 0.1f, z + 0.5f);

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "Green_Box" + green_num.ToString();;
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height / 20.0f, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = Color.grey;
                    cube.GetComponent<BoxCollider>().size = new Vector3(0.8f, 20.0f * storey_height, 0.8f);
                    cube.GetComponent<BoxCollider>().isTrigger = true;
                    cube.AddComponent<Drug>();
                    green_num++;
                }
				else if (solution[w, l][0] == TileType.BLOCK)
                {
					          int idx = UnityEngine.Random.Range(0, 4);

                    // if(!SceneChange.isRetrying)
                    // {
                    //     // Debug.Log(SceneChange.retryPrimitivesIndex);
                    //     // SceneChange.retryPrimitivesIndex.Add(idx); //retry logic
                    // }

                    // else{
                    //     idx = SceneChange.retryPrimitivesIndex[count];
                    //     count++;
                    // }
                    GameObject block = GameObject.CreatePrimitive(list_of_primitives[idx]);
                    block.name = "Block"+num.ToString();
					          num++;
                    block.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    block.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    block.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f);
                    // cube.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
					if(idx == 0){//Cube
						block.GetComponent<BoxCollider>().size = new Vector3(1.5f, 20.0f * storey_height, 1.5f);
						block.GetComponent<BoxCollider>().isTrigger = true;
						block.AddComponent<BoxCollider>();
						block.AddComponent<Block>();
					}else if(idx == 1){//Sphere
						block.GetComponent<SphereCollider>().radius = 1.5f;
						block.GetComponent<SphereCollider>().isTrigger = true;
						block.AddComponent<SphereCollider>();
						block.AddComponent<Block>();
					}else if(idx == 2){//Capsule
						block.GetComponent<CapsuleCollider>().radius = 1.5f;
						block.GetComponent<CapsuleCollider>().height = 2.0f;
						block.GetComponent<CapsuleCollider>().isTrigger = true;
						block.AddComponent<CapsuleCollider>();
						block.AddComponent<Block>();
					}else{//Cylinder
						block.GetComponent<CapsuleCollider>().radius = 1.5f;
						block.GetComponent<CapsuleCollider>().height = 2.0f;
						block.GetComponent<CapsuleCollider>().isTrigger = true;
						block.AddComponent<CapsuleCollider>();
						block.AddComponent<Block>();
					}

				}
                else if (solution[w, l][0] == TileType.WATER)
                {
                    GameObject water = Instantiate(water_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    water.name = "Red" + red_num.ToString();
                    water.transform.localScale = new Vector3(0.5f * bounds.size[0] / (float)width, 1.0f, 0.5f * bounds.size[2] / (float)length);
                    water.transform.position = new Vector3(x + 0.5f, y + 0.1f, z + 0.5f);

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "Red_Box" + red_num.ToString();
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height / 20.0f, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = Color.grey;
                    cube.GetComponent<BoxCollider>().size = new Vector3(1.1f, 20.0f * storey_height, 1.1f);
                    cube.GetComponent<BoxCollider>().isTrigger = true;
                    cube.AddComponent<Water>();
                    red_num++;
                }
            }
        }
    }

	void Destroy(){
		//Destroy
		deleted.Add(curr_block_1);
		deleted.Add(curr_block_2);
		Destroy(curr_block_1);
		Destroy(curr_block_2);
    poofClip.Play();
		curr_block_1 = null;
		curr_block_2 = null;
		destroyCalled = false;
	}

	void RevertColor(){
		curr_block_1.GetComponent<Renderer>().material.color = Color.grey;
		curr_block_2.GetComponent<Renderer>().material.color = Color.grey;
		curr_block_1 = null;
		curr_block_2 = null;
		revertCalled = false;
	}

  // void Appear()
  // {
  //     Debug.Log("invoke inside");
  //     ghost.gameObject.SetActive(true);
  // }


    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION JUST TO ADD SOUNDS ***
    // YOU MAY CHOOSE ANY SHORT SOUNDS (<2 sec) YOU WANT FOR A VIRUS HIT, A VIRUS INFECTION,
    // GETTING INTO THE WATER, AND REACHING THE EXIT
    // note: you may also change other scripts/functions to add sound functionality,
    // along with the functionality for the starting the level, or repeating it
    void Update()
    {
      // if(SceneChange.level_loaded == false){
      //     return;
      // }
      // if(!drawn){
      //     drawn = true;
      //     DrawDungeon(grid);
      // }

      energydisplay.text = ""+energy;

      //sounds
      if(Drug.playgreenclip)
      {
        Drug.playgreenclip = false;
        greenClip.Play();
      }

      if(Water.playredclip)
      {
        Water.playredclip = false;
        redClip.Play();
      }

      if(Virus.zombieclipplay)
      {
        Virus.zombieclipplay = false;
        zombieClip.Play();
      }

      if(Ghost.playghostclip)
      {
        Ghost.playghostclip = false;
        ghostClip.Play();
      }

      if(Block.playtouchclip)
      {
        Block.playtouchclip = false;
        touchClip.Play();
      }

      //sword - lifesaver
      if (LifeSaver.lifesaved)
      {
          sword.gameObject.SetActive(true);
      }





      // Debug.Log(energy);
      if (energy < 1) //end level
      {
          levelLost.gameObject.SetActive(true);
          // Invoke("TryAgainScene", 3);
          return;
      }

		if(deleted.Count == blocknum){
      if (SceneChange.level_number == 5)
      {
        GameOverMenu.gameObject.SetActive(true);
      }
      else{
        levelUpMenu.gameObject.SetActive(true);
      }
		}


    //logic for
    // Invoke("Appear", 2);

		for(int i=0; i<blocknum; i++){
			/*
			if(!deleted.Contains(list_of_objects[i])){
				ObjectValues ov = map[list_of_objects[i]];
				list_of_objects[i].transform.Rotate(new Vector3(0f, Time.deltaTime * ov.degrees, 0f), Space.World);

				// Float up/down with a Sin()
				tempPos = ov.posOffset;
				tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * ov.freq) * ov.amp;

				list_of_objects[i].transform.position = tempPos;
			}*/
		}

        if(curr_block_1 != null){
			ObjectValues ov = map[curr_block_1];
			curr_block_1.GetComponent<Renderer>().material.color = ov.c;
		}

		if(curr_block_2 != null){
			ObjectValues ov = map[curr_block_2];
			curr_block_2.GetComponent<Renderer>().material.color = ov.c;
			if(curr_block_1.GetComponent<Renderer>().material.color == curr_block_2.GetComponent<Renderer>().material.color){
				if(!destroyCalled){
					destroyCalled = true;
					Invoke("Destroy", 1);
				}
			}else{
				if(!revertCalled){
					revertCalled = true;
					Invoke("RevertColor", 1);
				}

			}
		}

        //
        // if (player_health < 0.001f) // the player dies here
        // {
        //     text_box.GetComponent<Text>().text = "Failed!";
        //
        //     /*if (fps_player_obj != null)
        //     {
        //         GameObject grave = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //         grave.name = "GRAVE";
        //         grave.transform.localScale = new Vector3(bounds.size[0] / (float)width, 2.0f * storey_height, bounds.size[2] / (float)length);
        //         grave.transform.position = fps_player_obj.transform.position;
        //         grave.GetComponent<Renderer>().material.color = Color.black;
				// Cursor.lockState = CursorLockMode.None;
        //         Cursor.visible = true;
        //         Object.Destroy(fps_player_obj);
				//
        //     }*/
        //     return;
        // }

        if (player_entered_house) // the player suceeds here, variable manipulated by House.cs
        {
			if(enteredHouse == 0){
				enteredHouse = 1;
				//Debug.Log("Player won !");
				//houseClip.Play();
				//AudioSource.PlayClipAtPoint(hClip, new Vector3(5, 1, 2));
			}
      //       if (virus_landed_on_player_recently){
			// 	text_box.GetComponent<Text>().text = "Washed it off at home! Success!!!";
			// }else{
			// 	text_box.GetComponent<Text>().text = "Success!!!";
			// }
			//yield WaitForSeconds(3.0);

            //Object.Destroy(fps_player_obj);
			Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
			Invoke("EndMenuScene", 3);
            return;
        }

        // if (Time.time - timestamp_last_msg > 7.0f) // renew the msg by restating the initial goal
        // {
        //     text_box.GetComponent<Text>().text = "Find your home!";
        // }

        // virus hits the players (boolean variable is manipulated by Virus.cs)
        if (virus_landed_on_player_recently)
        {
			if(num_virus_hit_concurrently != old_num_virus_hit_concurrently){
				//virusClip.Play();
			}
			old_num_virus_hit_concurrently = num_virus_hit_concurrently;
            float time_since_virus_landed = Time.time - timestamp_virus_landed;
            if (time_since_virus_landed > 5.0f)
            {
				virusLanded = 0;
                // player_health -= Random.Range(0.25f, 0.5f) * (float)num_virus_hit_concurrently;
                // player_health = Mathf.Max(player_health, 0.0f);
                // if (num_virus_hit_concurrently > 1)
                //     text_box.GetComponent<Text>().text = "Ouch! Infected by " + num_virus_hit_concurrently + " viruses";
                // else
                //     text_box.GetComponent<Text>().text = "Ouch! Infected by a virus";
                timestamp_last_msg = Time.time;
                timestamp_virus_landed = float.MaxValue;
                virus_landed_on_player_recently = false;
                num_virus_hit_concurrently = 0;
				old_num_virus_hit_concurrently = 0;
            }
            else
            {
				if(virusLanded == 0){
					virusLanded = 1;
					//virusClip.Play();
				}
                // if (num_virus_hit_concurrently == 1)
                //     text_box.GetComponent<Text>().text = "A virus landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
                // else
                //     text_box.GetComponent<Text>().text = num_virus_hit_concurrently + " viruses landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
            }
        }

        // drug picked by the player  (boolean variable is manipulated by Drug.cs)
        if (drug_landed_on_player_recently)
        {
			//Debug.Log("DrugClip start.....");
			//drugClip.Play();
			//Debug.Log("DrugClip end.....");
            // if (player_health < 0.999f || virus_landed_on_player_recently)
            // if (virus_landed_on_player_recently)
            //     text_box.GetComponent<Text>().text = "Phew! New drug helped!";
            // else
            //     text_box.GetComponent<Text>().text = "No drug was needed!";
            timestamp_last_msg = Time.time;
            // player_health += Random.Range(0.25f, 0.75f);
            // player_health = Mathf.Min(player_health, 1.0f);
            drug_landed_on_player_recently = false;
            timestamp_virus_landed = float.MaxValue;
            virus_landed_on_player_recently = false;
            num_virus_hit_concurrently = 0;
        }

        // splashed on water  (boolean variable is manipulated by Water.cs)
        if (player_is_on_water)
        {
			if(playerOnWater == 0){
				playerOnWater = 1;
				//waterClip.Play();
			}
            // if (virus_landed_on_player_recently)
            //     text_box.GetComponent<Text>().text = "Phew! Washed it off!";
            timestamp_last_msg = Time.time;
            timestamp_virus_landed = float.MaxValue;
            virus_landed_on_player_recently = false;
            num_virus_hit_concurrently = 0;
        }

		if(!player_is_on_water) playerOnWater = 0;

        // update scroll bar (not a very conventional manner to create a health bar, but whatever)
        scroll_bar.GetComponent<Scrollbar>().size = energy/100.0f;
        if (energy < 30.0f)
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(1.0f, 0.0f, 0.0f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }
        else
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(0.0f, 1.0f, 0.25f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }

    }
}
