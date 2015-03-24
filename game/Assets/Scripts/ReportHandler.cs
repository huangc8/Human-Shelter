/// <summary>
/// Report handler. Runs UI for the reports displays daily reports at the bottom of the screen
/// Lets the player move forwards and backwards through the reports. Can dismay daily reports as
/// a unit, or individual reports.
/// </summary>
using UnityEngine;
using System.Collections;

public class ReportHandler : MonoBehaviour
{
	public TextMesh ReportText;
	//List <Report> _reports; // the reports of assign task

	// Use this for initialization
	void Start ()
	{
		ReportText.text = "report text goes here";
	}

	// Update is called once per frame
	void Update ()
	{

	}
}
