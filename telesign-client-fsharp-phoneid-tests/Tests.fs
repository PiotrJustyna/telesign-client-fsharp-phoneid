module Tests

open System
open Xunit
open Telesign.PhoneID

[<Fact>]
let ``Test 1`` () =
    let dateRfc2616 = DateTime.UtcNow.ToString("r")
    let nonce = Guid.NewGuid().ToString()
    let restEndpoint = "https://rest-us.telesign.com"
    let phoneNumber = "sensitive"
    let customerId = "sensitive"
    let apiKey = "sensitive"
    let resource = $"/v1/phoneid/{phoneNumber}"

    task {
        let request = telesignRequest dateRfc2616 nonce restEndpoint resource customerId apiKey
        let! response = telesignResponse request
        Assert.True(response.Length > 0)
    }