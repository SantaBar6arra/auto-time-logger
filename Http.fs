module AutoTimeLogger.Http

open System.Net.Http
open System

let fetch (httpClient: HttpClient) (uri: Uri) =
    async {
        use! response = httpClient.GetAsync uri |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }


let runRequest (httpClient: HttpClient) httpRequestMessage =
    async {
        use! response = httpClient.SendAsync httpRequestMessage |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }
