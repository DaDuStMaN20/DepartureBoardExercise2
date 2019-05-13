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

            Dictionary<string, DataTable> UntiedAirlines = getAllData("C:\\Users\\dw102\\Documents\\ETL_QA_ExerciseFiles\\ExerciseFiles\\UntiedAirlines");
            Dictionary<string, DataTable> AmonricaAirlines = getAllData("C:\\Users\\dw102\\Documents\\ETL_QA_ExerciseFiles\\ExerciseFiles\\AmonricaAirlines");

            var result = UntiedAirlines["FLG_RT"].AsEnumerable()
                    .Join(
                        UntiedAirlines["AIRPT_DPT"].AsEnumerable(),
                        flights => flights.Field<string>("SerialNum"),
                        departure => departure.Field<string>("SerialNum"),
                        (flights, departure) => new { flights, departure }
                    )
                    .Join(
                        UntiedAirlines["FLG_ST"].AsEnumerable(),
                        flights => flights.flights.Field<string>("SerialNum"),
                        status => status.Field<string>("SerialNum"),
                        (flights, status) => new { flights, status }
                    )
                    .Join(
                        UntiedAirlines["MST_ST"].AsEnumerable(),
                        flights => flights.status.Field<string>("Status"),
                        mst => mst.Field<string>("Status"),
                        (flights, mst) => new { flights, mst }
                    );


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


            List<Flight> model = untiedAirlinesFlights;
            model.AddRange(amonricaAirlinesFlights);
            return View(model);
        }

        public ActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        ///     Gets a list of objects containing the contents of any csv file in a specified folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Dictionary<string, DataTable> getAllData(string path)
        {
            Dictionary<string, DataTable> data = new Dictionary<string, DataTable>();

            string[] filePaths = Directory.GetFiles(path, "*.csv");

            foreach(var file in filePaths)
            {
                var table = getData(file);

                data.Add(table.TableName, table);
            }


            return data;
        }

        public DataTable getData(string path)
        {
            string fileName = path.Split('\\').LastOrDefault();

            //Remove the .csv file extension
            fileName = fileName.Substring(0, fileName.Length - 4);

            DataTable dt = new DataTable();

            dt.TableName = fileName;

            using (StreamReader sr = new StreamReader(path))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }

            }

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
