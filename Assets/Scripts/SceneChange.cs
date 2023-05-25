using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChange : MonoBehaviour
{
		public static bool start_menu_loaded = false;
	  public static bool level_loaded = false;
		public static List<TileType>[,] old_grid;
		public static int old_startx;
		public static int old_starty;
		public static int old_endx;
		public static int old_endy;
		public Text levelname;
		public static int level_number = 1;
		public GameObject pauseMenu;
		public Scene scene;

		void Start()
		{
				pauseMenu = GameObject.Find("PauseMenu");
				Time.timeScale = 1;
				scene = SceneManager.GetActiveScene();
		}

		void Update()
		{
				if(scene.name == "level")
				{
					levelname.text = "Level " + level_number;
				}
		}

	  public void LoadScene(string sceneName){
				SceneManager.LoadScene(sceneName);
	}

		public void QuitGame()
		{
				UnityEditor.EditorApplication.isPlaying = false;
				Application.Quit();
		}

		public void pause()
		{
				pauseMenu.gameObject.SetActive(true);
				Time.timeScale = 0;
		}

		public void resume()
		{
				pauseMenu.gameObject.SetActive(false);
				Time.timeScale = 1;
		}

		public void nextLevel()
		{
				SceneManager.LoadScene(scene.name);
				level_number++;
				if(level_number > 5){
					level_number = 5;
				}
		}

		public void playAgain()
		{
				level_number = 1;
				SceneManager.LoadScene("level");
		}

		public void retryLevel()
		{
				Scene scene = SceneManager.GetActiveScene();
				SceneManager.LoadScene(scene.name);
		}
}
