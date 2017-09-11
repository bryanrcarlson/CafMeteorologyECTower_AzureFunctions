#r "Newtonsoft.Json"

using System;
using System.Net;
using Newtonsoft.Json;
using Nsar.Nodes.Models.LoggerNet.Meteorology;
using Nsar.Nodes.Models.DocumentDb.Measurement;
using Nsar.Nodes.CafEcTower.LoggerNet.Transform;
using Nsar.Nodes.CafEcTower.LoggerNet.Extract;
using Nsar.Nodes.CafEcTower.LoggerNet.Mappers;

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
    var fileNewest;
    foreach (var file in data)
    {
        string filename = file.Name;

        // Expect filename similar to: "cafMET000L_01_20090710_00.csv"
        string[] sections = filename.Split('_');
        DateTime dt = DateTime.Parse(section[2]);

        log.Info("dt: " + dt.String());

        if(dt > newestDT)
        {
            newestDT = dt;
            fileNewest = file;
        }
    }

    log.Info("fileNewest: " + fileNewest);
    
    string result = JsonConvert.SerializeObject(fileNewest,
        Newtonsoft.Json.Formatting.None);
    
    log.Info("result: " + result);

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent(results, System.Text.Encoding.UTF8, "application/json");
    return response;
}