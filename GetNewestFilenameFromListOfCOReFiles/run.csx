#r "Newtonsoft.Json"

using System;
using System.Net;
using System.Globalization;
using Newtonsoft.Json;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    string jsonContent = await req.Content.ReadAsStringAsync();
    dynamic data = JsonConvert.DeserializeObject(jsonContent);
    //log.Info("data: " + data.ToString());

    if (data == null)
    {
        log.Error("No data");
        
        return req.CreateResponse(HttpStatusCode.BadRequest, new
        {
            error = "data are null"
        });
    }

    DateTime newestDT = DateTime.MinValue;
    string filePathNewest = "";
    foreach (var file in data)
    {
        string filename = file.Name;

        // Only process csv files
        if(Path.GetExtension(filename) != ".csv") continue;

        // Expect filename similar to: "cafMET000L_01_20090700_00.csv"
        string[] sections = filename.Split('_');
        if(sections.Length < 4) continue;

        string dateString = sections[2];

        log.Info("dateString: " + dateString);
        
        // If day is "00" then remove it
        DateTime dt;

        if(dateString.Substring(dateString.Length-2) == "00")
        {
            dateString = dateString.Remove(dateString.Length-2);
            dt = DateTime.ParseExact(dateString, "yyyyMM", CultureInfo.InvariantCulture);
        }
        else
        {
            dt = DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        log.Info("dt: " + dt.ToString());

        if(dt > newestDT)
        {
            log.Info("Found newer file.  Path = " + file.Path);
            newestDT = dt;
            filePathNewest = file.Path;
        }
    }

    log.Info("filePathNewest: " + filePathNewest);

    var returnObj = new {filepath = filePathNewest};
    
    string result = JsonConvert.SerializeObject(returnObj);

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
    return response;
}