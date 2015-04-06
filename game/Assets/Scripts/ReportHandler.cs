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
	// ===================================================== data
	List <Report> _reports; 				// the reports of assign task
	int _currentReportIndex = 0; 			// which report we are currently looking at
	bool _hasReports; 						// whether or not we have reports to display
	string _reportString = "";				// tmp string

	// ===================================================== initialization
	// Use this for initialization
	void Start ()
	{

	}

	// ===================================================== functions
	/// <summary>
	/// Passes the reports.
	/// </summary>
	/// <param name="reports">Reports.</param>
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

	// ====================================================== GUI
	/// <summary>
	/// Raises the GU event.
	/// </summary>
	void OnGUI()
	{
		if(_hasReports)
		{
			int x = 100;
			int y = 100;
			int w = 200;
			int h = 30;

			if(_currentReportIndex > 0){
				//move to previous report
				if(GUI.Button(new Rect(x,y,w,h),"Previous Report")){
					_currentReportIndex--;
					if(_currentReportIndex < 0){
						_currentReportIndex = 0;
					}
					_reportString = _reports[_currentReportIndex].GetMessage();
				}
				x += w;
			}

			w = 500;
			h = 50;
			//Print the report text
			GUI.Label(new Rect(x,y,w,h), "Report: " + _reportString);
			x += w;
			w = 200;
			h = 30;

			if(_currentReportIndex < _reports.Count-1){
				//move to next report
				if(GUI.Button(new Rect(x,y,w,h),"Next Report")){
					_currentReportIndex++;
					if(_currentReportIndex >= _reports.Count-1){
						_currentReportIndex = _reports.Count-1;
					}
					_reportString = _reports[_currentReportIndex].GetMessage();
				}
				x += w;
			}

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

			if(_reports.Count > 1){
				//dismiss all reports
				//check for buttons allowing to delete report,
				if(GUI.Button(new Rect(x,y,w,h),"Dismiss All")){
					_reports = new List<Report>();
					_hasReports = false;
	            }
			}
        }
    }
}
