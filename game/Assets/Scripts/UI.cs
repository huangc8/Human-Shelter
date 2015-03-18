using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {
	SpriteRenderer rend;


	// Use this for initialization
	void Start () {
		rend = gameObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;



	}

	void OnGUI (){



	}

	// Update is called once per frame
	void Update () {
	
	}
}
