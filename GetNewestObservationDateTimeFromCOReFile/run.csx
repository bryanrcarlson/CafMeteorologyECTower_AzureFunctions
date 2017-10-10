#r "Newtonsoft.Json"

using System;
using System.Net;
using System.Globalization;
using Newtonsoft.Json;
using System.IO;

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

    string fileContent = data.fileContent;
    DateTime newestDTUtc = DateTime.MinValue;
    
    using (StringReader reader = new StringReader(fileContent))
    {
        // Skip first line (the header)
        reader.ReadLine();

        string line;
        while((line = reader.ReadLine()) != null)
        {
            //CAF,000,2010-07-10T00:00-05:00,L,18.4,0.6,127,21,0,102.4,0,,,12.93,20.40
            string[] measurements = line.Split(',');

            // Get datetime
            log.Info("Date: " + measurements[2]);
            DateTimeOffset dtOffset = DateTimeOffset.Parse(measurements[2], CultureInfo.InvariantCulture);
            log.Info("dto: " + dtOffset.ToString("s"));

            DateTime dtUtc = dtOffset.UtcDateTime;

            log.Info("DateUtc: " + dtUtc.ToString("s"));

            if(dtUtc > newestDTUtc) newestDTUtc = dtUtc;
        }
    }


    var returnObj = new {
        formattedDateTime = newestDTUtc.ToString("s")+"Z", 
        year = newestDTUtc.Year,
        month = newestDTUtc.Month,
        day = newestDTUtc.Day,
        hour = newestDTUtc.Hour,
        minute = newestDTUtc.Minute,
        second = newestDTUtc.Second,
        millisecond = newestDTUtc.Millisecond};
    
    string result = JsonConvert.SerializeObject(returnObj);

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
    return response;
}