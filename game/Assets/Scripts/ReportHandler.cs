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
	public ArrayList _pages;
	public int _currentPage = 0;

	// ===================================================== initialization
	// Use this for initialization
	void Start ()
	{
		showReports = false;
		_pages = new ArrayList ();
		_reports = new List<Report>();

	}

	public List<Report> CurrentReports{
		get{
			if(_pages != null && _pages.Count > 0){
				return (List<Report>)_pages[_currentPage];
			}
			else{
				return null;
			}
		}
	}


	// ===================================================== functions
	/// <summary>
	/// Passes the reports.
	/// </summary>
	/// <param name="reports">Reports.</param>
	public void PassReports(List<Report> reports)
	{
		List<Report> newReports = new List<Report>();
		//I know this is a hacky approach, we can take it out later if you want
		for(int r = 0; r < reports.Count; r++){
			if(reports[r] && reports[r].IsInitialized() == false){
				reports.RemoveAt(r);
			}
		}
		
		//Debug.Log("Passing Reports");
		if(reports.Count > 0){


			newReports = new List<Report>();
			_hasReports = true;

			foreach(Report rept in reports){
				newReports.Add(rept);
			}

			//newReports = reports;//_reports = reports;
			_currentReportIndex = 0;

			/*
			_reportString = reports[_currentReportIndex].GetMessage();
			Debug.Log ("_reportString set to:");*/

			_pages.Add(newReports);

			Report bugTracer = new Report();

			//bugTracer.SetMessage("ERROR CHECK LINE 59 ReportHandler");
			//new List<Report>();

			_currentPage = _pages.Count -1;

			_reports = (List<Report>)_pages[_currentPage];
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

	public void nextPage()
	{
		if(_currentPage < _pages.Count-1){
			//move to next page
				_currentPage++;
		}
		}

	public void lastPage()
	{

		if(_currentPage > 0){
			//move to previous report
				_currentPage--;
				_reports = CurrentReports;
			
		}

		}

	void OnGUI()
	{








		if(CurrentReports == null){
			//Debug.LogError("Reports are null.");
		}

		//Debug.Log ("ReportHandler.OnGUI():");

#if debugmode
		int rIndex = 0;
		foreach(Report rIt in _reports){
			if(rIt == null){
				Debug.LogError("rIt == null " + rIndex);
			}
			else{
				Debug.Log ("Report message: " + rIt.GetMessage());
			}
			rIndex++;
		}

		Debug.Log ("_____________________");
#endif
		if(showReports && CurrentReports != null)
		{
			if(_hasReports)
			{
				//float x = 300;
				//float y = 100;
				//float w = 200;
				//float h = 30;

				/*if(_currentPage > 0){
					//move to previous report
					if(GUI.Button(new Rect(x,y,w,h),"Previous Page")){
						_currentPage--;
						_reports = CurrentReports;
					}
					x += w;
				}*/

#if debugmode
				int  rIt =0;

				foreach(Report r in _reports){
					if(r){
						r.PrintReport(rIt);
					}
					else{
						Debug.LogError("ERROR: r is null");
					}
					rIt++;
				}
#endif

				//all in UI now


				/*x += w;
				w = 200;
				h = 30;
				if(_currentPage < _pages.Count-1){
					//move to next page
					if(GUI.Button(new Rect(x,y,w,h),"Next Page")){
						_currentPage++;
					}
					x += w;
				}
				*/
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
