using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DepartureBoardExercise2.Models;
using System.IO;
using System.Data;

namespace DepartureBoardExercise.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Get the current directory (as that is where the data files are stored)
            var currentDirectory = HttpRuntime.AppDomainAppPath;

            //Retrieve the data tables for each airline
            Dictionary<string, DataTable> UntiedAirlines = getAllData(currentDirectory +  "\\FlightData\\UntiedAirlines");
            Dictionary<string, DataTable> AmonricaAirlines = getAllData(currentDirectory + "\\FlightData\\AmonricaAirlines");

            //Extract the flight information from the data tables and put them in a flight object
            var untiedAirlinesFlights = (from flights in UntiedAirlines["FLG_RT"].AsEnumerable()
                                        join departure in UntiedAirlines["AIRPT_DPT"].AsEnumerable()
                                        on flights.Field<string>("SerialNum") equals departure.Field<string>("SerialNum")
                                        join status in UntiedAirlines["FLG_ST"].AsEnumerable()
                                        on flights.Field<string>("SerialNum") equals status.Field<string>("SerialNum")
                                        join destinations in UntiedAirlines["FLG_RT"].AsEnumerable()
                                        on flights.Field<string>("SerialNum") equals destinations.Field<string>("SerialNum")
                                        join statusText in UntiedAirlines["MST_ST"].AsEnumerable()
                                        on status.Field<string>("Status") equals statusText.Field<string>("Status")
                                        select new Flight
                                        {
                                            Airline = "Untied Airlines",
                                            Destination = destinations.Field<string>("To"),
                                            FlightNum = status.Field<string>("Flight"),
                                            Gate = departure.Field<string>("Gate"),
                                            Remarks = statusText.Field<string>("Desc"),
                                            Time = departure.Field<string>("Time")
                                        }).ToList();


            var amonricaAirlinesFlights = (from flights in AmonricaAirlines["FlightDeparture"].AsEnumerable()
                                          join passengers in AmonricaAirlines["FlightPassengers"].AsEnumerable()
                                          on flights.Field<string>("Flight Number") equals passengers.Field<string>("Flight Number")
                                          join status in AmonricaAirlines["FlightStatus"].AsEnumerable()
                                          on flights.Field<string>("Flight Number") equals status.Field<string>("Flight Number")
                                          select new Flight
                                          {
                                              Airline = "Amonrica Airlines",
                                              Destination = passengers.Field<string>("Destination"),
                                              FlightNum = flights.Field<string>("Flight Number"),
                                              Gate = flights.Field<string>("Gate"),
                                              Remarks = status.Field<string>("Status"),
                                              Time = flights.Field<string>("Time")
                                          }).ToList().Distinct(new FlightComparer());


            //combine the lists into one list
            List<Flight> model = untiedAirlinesFlights;
            model.AddRange(amonricaAirlinesFlights);

            //return the view with the flight list
            return View(model);
        }


        /// <summary>
        ///     Gets the data from all .csv files in a specific location
        /// </summary>
        /// <param name="path">The location of the csv files</param>
        /// <returns>Dictionary of DataTables</returns>
        public Dictionary<string, DataTable> getAllData(string path)
        {
            
            //create a base data dictionary
            Dictionary<string, DataTable> data = new Dictionary<string, DataTable>();

            //get the file paths of all files with a .csv extension
            string[] filePaths = Directory.GetFiles(path, "*.csv");

            //for each file
            foreach(var file in filePaths)
            {
                //get the data table
                var table = getData(file);

                //add the table to the dictionary with the table name as the key
                data.Add(table.TableName, table);
            }

            //return the tables
            return data;
        }

        /// <summary>
        ///     Gets the data from each csv file
        /// </summary>
        /// <param name="path">The path to the CSV file</param>
        /// <returns></returns>
        public DataTable getData(string path)
        {
            //get the filename
            string fileName = path.Split('\\').LastOrDefault();

            //Remove the .csv file extension
            fileName = fileName.Substring(0, fileName.Length - 4);

            //create the data table
            DataTable dt = new DataTable();

            //set the table name to the filename
            dt.TableName = fileName;

            //initialize a streamreader
            using (StreamReader sr = new StreamReader(path))
            {
                //read the headers (always on the first line)
                string[] headers = sr.ReadLine().Split(',');

                //Add the column names
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }

                //for every subsequent line
                while (!sr.EndOfStream)
                {

                    //get all of the values from the row
                    string[] columnValues = sr.ReadLine().Split(',');

                    //create a new datarow
                    DataRow dr = dt.NewRow();

                    //for each item in headers
                    for (int i = 0; i < headers.Length; i++)
                    {
                        //add the column value
                        dr[i] = columnValues[i];
                    }

                    //add the datarow to the datatable
                    dt.Rows.Add(dr);
                }

            }

            //return the data table
            return dt;

        }


    }

    public class FlightComparer : IEqualityComparer<Flight>
    {
        public bool Equals(Flight x, Flight y)
        {
            if (string.Equals(x.FlightNum, y.FlightNum))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(Flight obj)
        {
            return obj.FlightNum.GetHashCode();
        }
    }
}
