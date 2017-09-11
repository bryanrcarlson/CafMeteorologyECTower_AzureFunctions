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

    log.Info("result: " + result);
    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
    return response;
}
