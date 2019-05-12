using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DepartureBoardExercise.Models;
using System.IO;
using System.Data;

namespace DepartureBoardExercise.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {

            Dictionary<string, DataTable> UntiedAirlines = getAllData("C:\\Users\\dw102\\Documents\\ETL_QA_ExerciseFiles\\ExerciseFiles\\UntiedAirlines");
            Dictionary<string, DataTable> AmonricaAirlines = getAllData("C:\\Users\\dw102\\Documents\\ETL_QA_ExerciseFiles\\ExerciseFiles\\AmonricaAirlines");

            var result = UntiedAirlines["FLG-RT"];


            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

            foreach (var file in filePaths)
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
}
