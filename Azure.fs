module AutoTimeLogger.Azure

open System
open System.Globalization
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Web
open Newtonsoft.Json

type AzureManager =
    { Username: string
      Token: string
      ApiUri: Uri
      DateFormat: string }

type CommitId =
    { [<JsonProperty("commitId")>]
      CommitId: string }

type GetAllCommitsResponse =
    { [<JsonProperty("count")>]
      Count: int
      [<JsonProperty("value")>]
      Value: CommitId[] }

type CommitData =
    { [<JsonProperty("comment")>]
      Comment: string }


let makeAzureGetRequest azureManager uri =
    async {
        use httpClient = new HttpClient()

        httpClient.DefaultRequestHeaders.Accept.Add
        <| MediaTypeWithQualityHeaderValue "application/json"

        // ":" is needed for proper auth
        let base64token =
            (":" + azureManager.Token)
            |> ASCIIEncoding.ASCII.GetBytes
            |> Convert.ToBase64String

        httpClient.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Basic", base64token)

        return! Http.fetch httpClient uri
    }


let formAllCommitsUri azureManager (date: DateTime) =
    let uriBuilder = UriBuilder(azureManager.ApiUri)

    uriBuilder.Path <- uriBuilder.Path + "/commits?api-version=7.1-preview.1"

    let query = HttpUtility.ParseQueryString uriBuilder.Query

    let fromDate = date.ToString(azureManager.DateFormat, CultureInfo.InvariantCulture) // todo: add proper handling in order to fulfill fp

    let toDate =
        (date.AddDays 1).ToString(azureManager.DateFormat, CultureInfo.InvariantCulture) // next day

    query["searchCriteria.fromDate"] <- fromDate
    query["searchCriteria.toDate"] <- toDate
    query["searchCriteria.user"] <- azureManager.Username

    uriBuilder.Query <- query.ToString()
    uriBuilder.Uri


let getCommitComment azureManager (commitId: string) =
    async {
        let commitUri =
            Uri(
                azureManager.ApiUri.ToString()
                + $"/commits/{commitId}?api-version=7.1-preview.1"
            )

        let! commitResponse = makeAzureGetRequest azureManager commitUri

        return JsonConvert.DeserializeObject<CommitData> commitResponse
    }


let getAllCommits azureManager date =
    let allCommitsUri = formAllCommitsUri azureManager date

    let allCommitsResponse =
        makeAzureGetRequest azureManager allCommitsUri |> Async.RunSynchronously

    let data = JsonConvert.DeserializeObject<GetAllCommitsResponse> allCommitsResponse

    Array.map (fun commitId -> getCommitComment azureManager commitId.CommitId) data.Value
    |> Async.Parallel
