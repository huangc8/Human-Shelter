/// <summary>
/// Report handler. Runs UI for the reports displays daily reports at the bottom of the screen
/// Lets the player move forwards and backwards through the reports. Can dismay daily reports as
/// a unit, or individual reports.
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic; //needed for List

public class ReportHandler : MonoBehaviour
{
	List <Report> _reports; // the reports of assign task
	int _currentReportIndex = 0; // which report we are currently looking at

	bool _hasReports; //whether or not we have reports to display

	string _reportString = "";

	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{

	}

	public void PassReports(List<Report> reports)
	{
		if(reports.Count > 0){
			_hasReports = true;
			_reports = reports;
			_currentReportIndex = 0;
			_reportString = reports[_currentReportIndex].GetMessage();
		}
		else
		{
			_hasReports = false;
		}
	}

	/// <summary>
	/// Raises the GU event.
	/// </summary>
	void OnGUI()
	{
		if(_hasReports)
		{
			//Print the report text
			GUI.Label(new Rect(100,100,500,500), "Report: " + _reportString);


			//check for buttons allowing to delete report,

			//move to next report

			//move to previous report

			//dismiss all reports
		}
	}
}
