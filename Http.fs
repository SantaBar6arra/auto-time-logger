module AutoTimeLogger.Http
open System.Net.Http

let fetch (httpClient: HttpClient) (url:string) =
    async {
        use! response = httpClient.GetAsync url |> Async.AwaitTask
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