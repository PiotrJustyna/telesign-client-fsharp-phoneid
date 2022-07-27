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
    let stringToSign
        (contentType: string)
        (dateRfc2616: string)
        (authorizationMethod: string)
        (nonce: string)
        (fieldsToSign: string)
        (resource: string)
        : string =
        $"POST\n{contentType}\n{dateRfc2616}\nx-ts-auth-method:{authorizationMethod}\nx-ts-nonce:{nonce}\n{fieldsToSign}\n{resource}"

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
        let authorizationMethod = "HMAC-SHA256"
        let fieldsToSign = "{}"
        let resourceUri = $"{restEndpoint}{resource}"

        let stringToSign =
            stringToSign contentType dateRfc2616 authorizationMethod nonce fieldsToSign resource

        let authorization =
            telesignAuthorizationHeader apiKey stringToSign customerId

        let headers =
            telesignHeaders authorization dateRfc2616 authorizationMethod nonce

        let requestMessage =
            new HttpRequestMessage(HttpMethod.Post, resourceUri)

        requestMessage.Content <- new StringContent(fieldsToSign, Encoding.UTF8, contentType)
        requestMessage.Content.Headers.ContentType <- MediaTypeHeaderValue(contentType)

        for entry in headers do
            requestMessage.Headers.Add(entry.Key, entry.Value)

        requestMessage

    let telesignResponse (request: HttpRequestMessage) : Task<string> =
        let client = new HttpClient()

        task {
            let! response = client.SendAsync(request).ConfigureAwait(false)
            let statusCode = response.StatusCode

            return!
                if statusCode = HttpStatusCode.OK then
                    response.Content.ReadAsStringAsync()
                else
                    Task.FromResult String.Empty
        }
