#r "Newtonsoft.Json"

using System;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

using CsvHelper;

using Nsar.Nodes.CafEcTower.DocumentDb.Extract;
using Nsar.Nodes.CafEcTower.DocumentDb.Transform;
using Nsar.Nodes.CafEcTower.LtarDataPortal.Load;
using Nsar.Nodes.CafEcTower.LtarDataPortal.Extract;
using Nsar.Nodes.Models.DocumentDb.Measurement;
using Nsar.Nodes.Models.LtarDataPortal.CORe;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    int utcOffset = -8;

    string jsonContent = await req.Content.ReadAsStringAsync();

    dynamic data = JsonConvert.DeserializeObject(jsonContent);

    //log.Info("data: " + data.ToString());

    if (data == null | 
        data.measurements[0].physicalQuantities[0] == null |
        data.recentFilePath == null |
        data.recentFileContent == null)
    {
        log.Error("No data");
        
        return req.CreateResponse(HttpStatusCode.BadRequest, new
        {
            error = "data are null"
        });
    }

    // Convert Measurements to CORe.Observation
    MeasurementJsonExtractor extractor = new MeasurementJsonExtractor();
    LtarDataPortalCOReTransformer transformer = new LtarDataPortalCOReTransformer();
    COReCsvStringWriter loader = new COReCsvStringWriter();

    List<Measurement> measurements = extractor.ToMeasurements(data.measurements.ToString());
    List<Observation> observations = transformer.ToCOReObservations("CAF", "000", 'L', utcOffset, measurements);
    log.Info("count: " + observations.Count);
    // Check if we're writing a new file
    string filename = loader.GetFilenamePstDateTime(observations[0]);
    //string filename = "cafMET001L_01_20170900_00.csv";
    string oldFileYYYYMM = data.recentFilePath.ToString().Substring(data.recentFilePath.ToString().Length-15, 6);
    string newFileYYYYMM = filename.Substring(filename.Length-15, 6);
    
    log.Info("old: "+oldFileYYYYMM);
    log.Info("new: "+newFileYYYYMM);
    
    // If files match then we need to append the data
    if(oldFileYYYYMM == newFileYYYYMM)
    {
        log.Info("Files match, appending old data");
        COReCsvExtractor e = new COReCsvExtractor();
        //log.Info("Recent content: " + data.recentFileContent.ToString());
        List<Observation> oldObs = e.GetObservations(data.recentFileContent.ToString(), utcOffset);
        if(oldObs.Count > 0)
        {
            oldObs.AddRange(observations);
            observations = oldObs;
        }
    }

    // Now write the data to string and return
    string fileContent = loader.GetContentString(observations);
    //string fileContent = "foo";
    
    var returnObj = new {filename = filename, fileContent = fileContent};
    
    string result = JsonConvert.SerializeObject(returnObj);

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
    return response; 
}