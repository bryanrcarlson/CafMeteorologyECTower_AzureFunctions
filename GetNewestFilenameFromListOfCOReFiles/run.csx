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

        // Expect filename similar to: "cafMET000L_01_20090710_00.csv"
        string[] sections = filename.Split('_');
        if(sections.Length < 4) break;

        log.Info("sections[2]: " + sections[2]);
        
        DateTime dt = DateTime.ParseExact(sections[2], "yyyyMMdd", CultureInfo.InvariantCulture);

        log.Info("dt: " + dt.ToString());

        if(dt > newestDT)
        {
            log.Info("Found newer file.  Path = " + file.Path);
            newestDT = dt;
            filePathNewest = file.Path;
        }
    }

    log.Info("filePathNewest: " + filePathNewest);
    
    //string result = JsonConvert.SerializeObject(filenameNewest,
    //    Newtonsoft.Json.Formatting.None);

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent(filePathNewest, System.Text.Encoding.UTF8, "application/json");
    return response;
}