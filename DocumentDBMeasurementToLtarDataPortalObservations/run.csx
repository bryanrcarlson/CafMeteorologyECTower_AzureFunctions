#r "Newtonsoft.Json"

using System;
using System.Net;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    var returnObj = new {filename = "test.csv", fileContent = "test"};
    string result = JsonConvert.SerializeObject(returnObj);

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
    return response;
}