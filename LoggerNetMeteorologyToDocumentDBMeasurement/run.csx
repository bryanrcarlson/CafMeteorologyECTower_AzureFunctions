#r "Newtonsoft.Json"

using System;
using System.Net;
using Newtonsoft.Json;
using Nsar.Nodes.Models.LoggerNet.Meteorology;
using Nsar.Nodes.Models.DocumentDb.Measurement;
using Nsar.Nodes.CafEcTower.LoggerNet.Transform;
using Nsar.Nodes.CafEcTower.LoggerNet.Extract;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    string jsonContent = await req.Content.ReadAsStringAsync();
    dynamic data = JsonConvert.DeserializeObject(jsonContent);

    if (data.filename == null || data.filecontent == null)
    {
        log.Error("filename and filecontents are null");
        
        return req.CreateResponse(HttpStatusCode.BadRequest, new
        {
            error = "filename and filecontents are null"
        });
    }

    MeteorologyCsvTableExtractor extractor = new MeteorologyCsvTableExtractor(data.filename.ToString(), data.filecontent.ToString());
    Meteorology met = extractor.GetMeteorology();
    
    Mappers.MapFromMeteorologyToCAFStandards map = new Mappers.MapFromMeteorologyToCAFStandards();
    DocumentDbMeasurementTransformer transformer = 
        new DocumentDbMeasurementTransformer(map, "1.0.0");
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
