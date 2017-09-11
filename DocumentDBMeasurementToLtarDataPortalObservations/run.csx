#r "Newtonsoft.Json"

using System;
using System.Net;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent("Foo", "application/json");
    return response;
}