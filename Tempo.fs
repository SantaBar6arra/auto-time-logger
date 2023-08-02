module AutoTimeLogger.Tempo
open System
open System.Net.Http
open System.Net.Http.Headers
open Newtonsoft.Json

type TempoManager = {
    Token: string
    ApiUrl: string
    IssueId: string
    AuthorAccountId: string
}

type RequestData = {
    [<JsonProperty("authorAccountId")>] AuthorAccountId: string
    [<JsonProperty("issueId")>] IssueId: string
    [<JsonProperty("startDate")>] Date: string
    [<JsonProperty("timeSpentSeconds")>] TimeSpent: int
    [<JsonProperty("description")>] Description: string
}    

let makeTempoPostRequest tempoManager reqData =
    async {
        use httpClient = new HttpClient()
        let httpRequestMessage = new HttpRequestMessage(
            Method = HttpMethod.Post,
            RequestUri = Uri tempoManager.ApiUrl,
            Content = new StringContent(JsonConvert.SerializeObject reqData, MediaTypeHeaderValue "application/json")
        )

        httpRequestMessage.Headers.Add("Authorization", $"Bearer {tempoManager.Token}")
            
        return! Http.runRequest httpClient httpRequestMessage
    }
    
let createWorklog (tempoManager: TempoManager) description =
    let date = DateTime.Now.ToString("yyyy-MM-dd")
    let timeSpent = 28800 // 8h * 3600s
    
    let reqData = {
        AuthorAccountId = tempoManager.AuthorAccountId
        IssueId = tempoManager.IssueId 
        Date = date
        TimeSpent = timeSpent
        Description = description
    }
    
    makeTempoPostRequest tempoManager reqData
