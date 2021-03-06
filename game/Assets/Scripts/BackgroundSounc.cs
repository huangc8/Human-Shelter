﻿using UnityEngine;
using System.Collections;

public class BackgroundSounc : MonoBehaviour
{

		public AudioSource _Day5BGM;
		public AudioSource _Day6BGM;
		public AudioSource _Day7BGM;
		public AudioSource _GateNoise;
		public GameObject _g;
		private GameTime _gt;
		private Visitor _vt;
		private Dialogue _diag;
		private UI _ui;

		// initialize
		void Start ()
		{
				_gt = _g.GetComponent<GameTime> ();
				_vt = _g.GetComponent<Visitor> ();
				_ui = _g.GetComponent<UI> ();
				_diag = _g.GetComponent<Dialogue> ();
		}

		// Update is called once per frame
		void Update ()
		{
				switch (_gt._currentDay) {
				case 5:
						_Day6BGM.Stop ();
						_Day7BGM.Stop ();
						if (!_Day5BGM.isPlaying) {
								_Day5BGM.Play ();
						}
						break;
				case 6:
						_Day5BGM.Stop ();
						_Day7BGM.Stop ();
						if (!_Day6BGM.isPlaying) {
								_Day6BGM.Play ();
						}
						break;
				case 7:
						_Day5BGM.Stop ();
						_Day6BGM.Stop ();
						if (!_Day7BGM.isPlaying) {
								_Day7BGM.Play ();
						}
						break;
				}// end of switch

				if (!_ui.tutorial && _ui.fading == 0 && !_diag._DiagCon.IsConversationActive && _vt._personList [_gt._currentDay] != null) {
						if (!_GateNoise.isPlaying) {
								_GateNoise.PlayDelayed (10);
						}
				} else {
						if (_GateNoise.isPlaying) {
								_GateNoise.Stop ();
						}
				}
		}
}
