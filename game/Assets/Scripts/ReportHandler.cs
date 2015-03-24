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
		Debug.Log("Passing Reports");
		if(reports.Count > 0){
			_hasReports = true;
			_reports = reports;
			_currentReportIndex = 0;
			_reportString = reports[_currentReportIndex].GetMessage();
			Debug.Log ("_reportString set to:");
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
			int x = 100;
			int y = 100;
			int w = 500;
			int h = 500;

			//Print the report text
			GUI.Label(new Rect(x,y,w,h), "Report: " + _reportString);
			x += w;
			w = 200;
			h = 100;

			//check for buttons allowing to delete report,
			if(GUI.Button(new Rect(x,y,w,h),"Dismiss Report")){
				_reports.RemoveAt(_currentReportIndex);
				if(_reports.Count == 0){
					_hasReports = false;
				}
				if(_currentReportIndex >= _reports.Count-1){
					_currentReportIndex = _reports.Count-1;
				}
				_reportString = _reports[_currentReportIndex].GetMessage();
            }
            x += w;

			//move to next report
			if(GUI.Button(new Rect(x,y,w,h),"Next Report")){
				_currentReportIndex++;
				if(_currentReportIndex >= _reports.Count-1){
					_currentReportIndex = _reports.Count-1;
				}
				_reportString = _reports[_currentReportIndex].GetMessage();
			}
			x += w;

			//move to previous report
			if(GUI.Button(new Rect(x,y,w,h),"Previous Report")){
				_currentReportIndex--;
				if(_currentReportIndex < 0){
					_currentReportIndex = 0;
				}
				_reportString = _reports[_currentReportIndex].GetMessage();
            }
            x += w;

			//dismiss all reports
			//check for buttons allowing to delete report,
			if(GUI.Button(new Rect(x,y,w,h),"Dismiss All")){
				_reports = new List<Report>();
				_hasReports = false;
            }
        }
    }
}
