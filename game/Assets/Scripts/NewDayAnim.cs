using UnityEngine;
using System.Collections;

//When you hit next day, an animation happens for each character
public class NewDayAnim : MonoBehaviour {

	Sprite heal, rest, shield;
	public bool running;

	// Use this for initialization
	void Start () {
		heal = (Sprite)Resources.Load ("Icon Heal", typeof(Sprite));
		rest = (Sprite)Resources.Load ("Icon Rest", typeof(Sprite));
		shield = (Sprite)Resources.Load ("Icon Shield", typeof(Sprite));

		running = false;
	}

	public IEnumerator run(Survivor[] s, int size)
	{
		running = true;

		for(int i = 0; i < size; i++)
		{						
			//hide ! above head
			foreach(Transform child in s[i].image.transform)
			{
				child.renderer.enabled=false;
			}
		}

		yield return new WaitForSeconds(.5f);


		for(int i = 0; i < size; i++)
		{
			Sprite image = null;

			if(s[i]._task == Survivor.task.Heal)
			{
				image = heal;
			}
			else if(s[i]._task == Survivor.task.Defend)
			{
				image = shield;
			}
			else if(s[i]._task == Survivor.task.Resting)
			{
				image = rest;
			}
			else
			{
				s[i].image.renderer.enabled=false;
			}


			if(image != null)
			{
				StartCoroutine(charAnim(s[i], image));
			}

			yield return new WaitForSeconds(1f);

		}

		running = false;
	}

	IEnumerator charAnim(Survivor s, Sprite image)
	{
		GameObject icon = new GameObject("Icon");
		SpriteRenderer render = icon.AddComponent<SpriteRenderer> ();
		render.sprite = image;
		icon.transform.position = new Vector3 (s.image.transform.position.x, s.image.transform.position.y+1f, s.image.transform.position.z);
		icon.transform.localScale = new Vector3 (.5f, .5f);

		for(int i = 0; i< 50; i++)
		{
			icon.transform.position = new Vector3(icon.transform.position.x, icon.transform.position.y+.02f, s.image.transform.position.z);
			if(i>25)
			{
				render.color = new Color(1,1,1,render.color.a-.08f);
			}
			yield return null;
		}
		Destroy (icon);
	}

	// Update is called once per frame
	void Update () {

	}
}
