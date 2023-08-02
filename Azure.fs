module AutoTimeLogger.Azure
open System
open System.Globalization
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Web
open Newtonsoft.Json

type AzureManager = {
    Username: string
    Token: string
    ApiUrl: string
}

type CommitId = {
    [<JsonProperty("commitId")>] CommitId: string
}

type GetAllCommitsResponse = {
    [<JsonProperty("count")>] Count: int
    [<JsonProperty("value")>] Value: CommitId[]
}

type CommitData = {
    [<JsonProperty("comment")>] Comment: string
}

let makeAzureGetRequest azureManager url =
    async {
        use httpClient = new HttpClient()
        httpClient.DefaultRequestHeaders.Accept.Add <| MediaTypeWithQualityHeaderValue "application/json"
        
        // ":" is needed for proper auth
        let base64token = (":" + azureManager.Token) |> ASCIIEncoding.ASCII.GetBytes |> Convert.ToBase64String
        httpClient.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Basic", base64token)
        
        return! Http.fetch httpClient url
    }

let formAllCommitsUrl azureManager =
    let uriBuilder = UriBuilder(azureManager.ApiUrl + "/commits?api-version=7.1-preview.1")
    let query = HttpUtility.ParseQueryString uriBuilder.Query
    
    let fromDate = DateTime.Today.ToString("G", CultureInfo.InvariantCulture) // .AddDays(-1) ??
    query["searchCriteria.fromDate"] <- fromDate
    query["searchCriteria.user"] <- azureManager.Username
    
    uriBuilder.Query <- query.ToString()
    uriBuilder.ToString()

let getCommitComment azureManager (commitId: string)  =
    async {
        let commitUrl = azureManager.ApiUrl + $"/commits/{commitId}?api-version=7.1-preview.1"
        let! commitResponse = makeAzureGetRequest azureManager commitUrl
        
        return JsonConvert.DeserializeObject<CommitData> commitResponse    
    }
    
let getAllCommits azureManager =
    let allCommitsUrl = formAllCommitsUrl azureManager
    let allCommitsResponse = makeAzureGetRequest azureManager allCommitsUrl |> Async.RunSynchronously
    
    let data = JsonConvert.DeserializeObject<GetAllCommitsResponse> allCommitsResponse
    
    Array.map (fun commitId -> getCommitComment azureManager commitId.CommitId) data.Value |> Async.Parallel
