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
	public bool showReports;
	ArrayList _pages;
	int _currentPage = 0;

	// ===================================================== initialization
	// Use this for initialization
	void Start ()
	{
		showReports = false;
		_pages = new ArrayList ();

	}

	// ===================================================== functions
	/// <summary>
	/// Passes the reports.
	/// </summary>
	/// <param name="reports">Reports.</param>
	public void PassReports(List<Report> reports)
	{
		//I know this is a hacky approach, we can take it out later if you want
		for(int r = 0; r < reports.Count; r++){
			if(reports[r] && reports[r].IsInitialized() == false){
				reports.RemoveAt(r);
			}
		}


		Debug.Log("Passing Reports");
		if(reports.Count > 0){
			_hasReports = true;
			_reports = reports;
			_currentReportIndex = 0;

			/*
			_reportString = reports[_currentReportIndex].GetMessage();
			Debug.Log ("_reportString set to:");
*/
			_pages.Add(_reports);
			_currentPage = _pages.Count -1;
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
		if(showReports)
		{
			if(_hasReports)
			{
				int x = 100;
				int y = 100;
				int w = 200;
				int h = 30;

				if(_currentPage > 0){
					//move to previous report
					if(GUI.Button(new Rect(x,y,w,h),"Previous Page")){
						_currentPage--;
						_reports = (List <Report>)_pages[_currentPage];
					}
					x += w;
				}
				int  rIt =0;

				foreach(Report r in _reports){
					if(r){
						r.PrintReport(rIt);
					}
					else{
						Debug.Log ("ERROR: r is null");
					}
					rIt++;
				}

				w = 500;
				h = 50;
				//Print the report text
				for(int i = 0; i < _reports.Count; i++)
				{
					try{
						GUI.Label(new Rect(x,y,w,h), "Report: " + _reports[i].GetMessage()); //error thrown is deifinitely not an indexing erro
						y+=50;
					}
					catch{
						Debug.LogError("ReportHandler (109) ERROR: i=" + i + " _reports.Count:" + _reports.Count);
					}
				}


				x += w;
				w = 200;
				h = 30;
				if(_currentPage < _pages.Count-1){
					//move to next page
					if(GUI.Button(new Rect(x,y,w,h),"Next Page")){
						_currentPage++;
						_reports = (List <Report>)_pages[_currentPage];
					}
					x += w;
				}
				
				/*
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
				*/
			}
        }
    }
}
