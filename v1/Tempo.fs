module AutoTimeLogger.Tempo

open System
open System.Net.Http
open System.Net.Http.Headers
open Newtonsoft.Json

type TempoManager =
    { Token: string
      ApiUri: Uri
      IssueId: int
      AuthorAccountId: string
      DateFormat: string }

type RequestData =
    { [<JsonProperty("authorAccountId")>]
      AuthorAccountId: string
      [<JsonProperty("issueId")>]
      IssueId: int
      [<JsonProperty("startDate")>]
      Date: string
      [<JsonProperty("timeSpentSeconds")>]
      TimeSpentSeconds: int
      [<JsonProperty("description")>]
      Description: string }


let makeTempoPostRequest tempoManager reqData =
    async {
        use httpClient = new HttpClient()

        let httpRequestMessage =
            new HttpRequestMessage(
                Method = HttpMethod.Post,
                RequestUri = tempoManager.ApiUri,
                Content =
                    new StringContent(JsonConvert.SerializeObject reqData, MediaTypeHeaderValue "application/json")
            )

        httpRequestMessage.Headers.Add("Authorization", $"Bearer {tempoManager.Token}")

        return! Http.runRequest httpClient httpRequestMessage
    }


let createWorklog (tempoManager: TempoManager) (date: DateTime) description =
    let date = date.ToString tempoManager.DateFormat
    let timeSpentSeconds = 28800 // 8h * 3600s

    let reqData =
        { AuthorAccountId = tempoManager.AuthorAccountId
          IssueId = tempoManager.IssueId
          Date = date
          TimeSpentSeconds = timeSpentSeconds
          Description = description }

    makeTempoPostRequest tempoManager reqData
