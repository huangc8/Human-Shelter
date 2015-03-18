using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {
	private Texture2D map;
	SpriteRenderer renderer;


	// Use this for initialization
	void Start () {
		map = Resources.Load ("map") as Texture2D;
		renderer = gameObject.GetComponent<SpriteRenderer>();



	}

	void OnGUI (){

		//display background
		Sprite background = Sprite.Create(map, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0, 0), 100.0f);
		renderer.sprite = background;


	}

	// Update is called once per frame
	void Update () {
	
	}
}
