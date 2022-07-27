namespace Telesign

open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Security.Cryptography
open System.Text
open System.Threading.Tasks

module PhoneID =
    let stringToSign (dateRfc2616: string) (nonce: string) (resource: string) : string =
        $"POST\napplication/json\n{dateRfc2616}\nx-ts-auth-method:HMAC-SHA256\nx-ts-nonce:{nonce}\n{{}}\n{resource}"

    let telesignAuthorizationHeader (apiKey: string) (stringToSign: string) (customerId: string) : string =
        let hasher =
            new HMACSHA256(Convert.FromBase64String(apiKey))

        let bytes = Encoding.UTF8.GetBytes(stringToSign)
        let hash = hasher.ComputeHash(bytes)
        let signature = Convert.ToBase64String(hash)
        $"TSA {customerId}:{signature}"

    let telesignHeaders
        (authorization: string)
        (dateRfc2616: string)
        (authMethod: string)
        (nonce: string)
        : IDictionary<string, string> =
        dict [ "Authorization", authorization
               "Date", dateRfc2616
               "x-ts-auth-method", authMethod
               "x-ts-nonce", nonce ]

    let telesignRequest
        (dateRfc2616: string)
        (nonce: string)
        (restEndpoint: string)
        (resource: string)
        (customerId: string)
        (apiKey: string)
        : HttpRequestMessage =
        let contentType = "application/json"
        let resourceUri = $"{restEndpoint}{resource}"
        let authMethod = "HMAC-SHA256"

        let fieldsToSign = "{}"

        let stringToSign = stringToSign dateRfc2616 nonce resource

        let authorization =
            telesignAuthorizationHeader apiKey stringToSign customerId

        let headers =
            telesignHeaders authorization dateRfc2616 authMethod nonce

        let requestMessage =
            new HttpRequestMessage(HttpMethod.Post, resourceUri)

        requestMessage.Content <- new StringContent(fieldsToSign, Encoding.UTF8, contentType)
        requestMessage.Content.Headers.ContentType <- MediaTypeHeaderValue("application/json")

        for entry in headers do
            requestMessage.Headers.Add(entry.Key, entry.Value)

        requestMessage

    let telesignResponse (request: HttpRequestMessage) : Task<string> =
        let client = new HttpClient()
        task {
            let! response = client.SendAsync(request).ConfigureAwait(false)
            let statusCode = response.StatusCode
            return! if statusCode = HttpStatusCode.OK then response.Content.ReadAsStringAsync() else Task.FromResult String.Empty
        }
