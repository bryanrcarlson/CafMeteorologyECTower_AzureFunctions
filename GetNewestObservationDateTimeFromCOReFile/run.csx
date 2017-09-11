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

    if (data.filename == null || 
        data.filecontent == null ||
        data.schemaVersion == null)
    {
        log.Error("filename, filecontents, and/or schemaVersion are null");
        
        return req.CreateResponse(HttpStatusCode.BadRequest, new
        {
            error = "filename, filecontents, and/or schemaVersion are null"
        });
    }

    MeteorologyCsvTableExtractor extractor = new MeteorologyCsvTableExtractor(
        data.filename.ToString(), data.filecontent.ToString(), -8);
    Meteorology met = extractor.GetMeteorology();
    
    MapFromMeteorologyToCafStandards map = new MapFromMeteorologyToCafStandards();
    DocumentDbMeasurementTransformer transformer = 
        new DocumentDbMeasurementTransformer(map, data.schemaVersion.ToString());
    var measurements = transformer.ToMeasurements(met);

    // Ignore null values
    string result = JsonConvert.SerializeObject(measurements,
        Newtonsoft.Json.Formatting.None,
        new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore
        });

    log.Info("result: " + result);
    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
    return response;
}
