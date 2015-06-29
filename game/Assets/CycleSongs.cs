using UnityEngine;
using System.Collections;

public class CycleSongs : MonoBehaviour {
	public AudioClip [] songs;
	public int nSongs = 3;
	private int currentSong = 0;

	private AudioSource songSource;
	// Use this for initialization
	void Start () {
		songSource = this.GetComponent<AudioSource> ();
		setSong ();

	}

	/// <summary>
	/// Sets the song. Picks the next song from the array of songs
	/// </summary>
	private void setSong(){
		currentSong = (currentSong + 1) % nSongs;
		songSource.clip = songs [currentSong];
		songSource.Play ();
	}

	// Update is called once per frame
	void Update () {
		if (songSource.isPlaying == false) {
			setSong ();
		}
	}
}
