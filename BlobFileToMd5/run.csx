#r "Newtonsoft.Json"

using System;
using System.Net;
using Newtonsoft.Json;
using System.Security;
using System.Text;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    string jsonContent = await req.Content.ReadAsStringAsync();
    dynamic data = JsonConvert.DeserializeObject(jsonContent);

    // Use input string to calculate MD5 hash
    string input = data.fileContent;
    string output = "";
    using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        // Convert the byte array to hexadecimal string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("X2"));
        }
        output = sb.ToString().ToLower();
    }

    string newFilename = data.filename.ToString().Replace("csv", "md5");

    var returnObj = new {hash = output, filename = newFilename};
    string result = JsonConvert.SerializeObject(returnObj);

    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
    return response;
}
