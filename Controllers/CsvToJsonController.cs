using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExcelToJson.Controllers
{
    /// <summary>
    /// This Controller is for CSV to JSON Convertor.
    /// The file extension must only be .csv 
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CsvToJsonController : Controller
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        private readonly ILogger<CsvToJsonController> _logger;

        public CsvToJsonController(ILogger<CsvToJsonController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "CSVToJSON")]
        public async Task<Object> Get(string Path)
        {
            // Browse the given path for files and folders. Drill down into each folder and get all the csv files. 
            List<string> allFilesToProcesss = GetAllCSVFilesFromPath(Path, "*.csv");

            foreach (var filepath in allFilesToProcesss)
            {
                ConvertCSVToJSON(Path, filepath);
            }

            return JsonConvert.SerializeObject(result);

        }

        private List<string> GetAllCSVFilesFromPath(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, new EnumerationOptions() { RecurseSubdirectories = true }).ToList();
        }

        private void ConvertCSVToJSON(string rootpath, string excelPath)
        {
            String[] csvContetnt = System.IO.File.ReadAllLines(excelPath);

            excelPath = excelPath.Replace(rootpath, "");
            short objIdx = 1; // To keep track of more that one table in csv
            
            string filepath = excelPath.Replace("\\", "_");
            string filename = filepath.Split('.')[0];
            string objectname = filename;
            if (csvContetnt.Length > 1) // Ensure that there is Headers as well as atleast one record. 
            {
                // Create the headers that will be used as objects properties. 

                var recordProperties = csvContetnt[0].Split(',');

                bool recordsPropertiesChanged = false;
                for (int i = 1; i < csvContetnt.Length; i++)
                {
                    var record = csvContetnt[i].Split(',');

                    if (record.Length > 1 && !recordsPropertiesChanged)
                    {
                        Dictionary<string, string> csv = new Dictionary<string, string>();

                        for (int j = 0; j < record.Length; j++)
                        {
                            csv.Add(recordProperties[j], record[j]);
                        }
                        if (!result.ContainsKey(objectname))
                        {
                            result.Add(objectname, new List<Dictionary<string, string>>() { csv });
                        }
                        else
                        {
                            ((List<Dictionary<string, string>>)result[objectname]).Add(csv);
                        }
                    }
                    else
                    {
                        // Reinitialize the header for new type of object...
                        if (recordsPropertiesChanged)
                        {
                            recordProperties = csvContetnt[i].Split(',');
                            objectname = $"{filename}_{objIdx++}"; // set the new object name for the new data. 
                            recordsPropertiesChanged = false;
                        }
                        else
                            recordsPropertiesChanged = true;
                    }
                }

            }

        }    
    }
}
