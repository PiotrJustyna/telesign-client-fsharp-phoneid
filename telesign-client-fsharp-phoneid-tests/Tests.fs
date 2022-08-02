module Tests

open System
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open Xunit
open Telesign.PhoneID

let phoneNumber = "sensitive"
let customerId = "sensitive"
let apiKey = "sensitive"

[<Fact>]
let ``Test 1`` () =
    let dateRfc2616 = DateTime.UtcNow.ToString("r")
    let nonce = Guid.NewGuid().ToString()
    let restEndpoint = "https://rest-us.telesign.com"
    let resource = $"/v1/phoneid/{phoneNumber}"
    let client = new HttpClient()
    let ct = CancellationToken.None

    let request =
        telesignRequest dateRfc2616 nonce restEndpoint resource customerId apiKey

    task {
        let! response = telesignResponse request client ct
        Assert.True(response.Length > 0)
    }

[<Fact>]
let ``Test 2`` () =
    let dateRfc2616 = DateTime.UtcNow.ToString("r")
    let nonce = Guid.NewGuid().ToString()
    let restEndpoint = "https://rest-us.telesign.com"
    let resource = $"/v1/phoneid/{phoneNumber}"
    let client = new HttpClient()
    let ct = CancellationToken(canceled = true)

    let request =
        telesignRequest dateRfc2616 nonce restEndpoint resource customerId apiKey

    task {
        try
            let! _ = telesignResponse request client ct
            Assert.True(false, "client is expected to throw")
        with
        | :? TaskCanceledException -> Assert.True true
        | _ -> Assert.True(false, "client is expected to throw")
    }