module AutoTimeLogger.Program
open System.Text

[<EntryPoint>]
let main _ =
    // todo: create + parse cfg
    let azureToken = ""
    let azureUser = "" 
    let azureBaseUrl = ""
    
    let tempoToken = "" 
    let tempoUrl = ""
    let tempoIssueId = ""
    let tempoUserId = ""
    
    let azureManager: Azure.AzureManager = { 
        Username = azureUser
        Token = azureToken
        ApiUrl = azureBaseUrl
    }

    let tempoManager: Tempo.TempoManager = {
        Token = tempoToken
        ApiUrl = tempoUrl
        IssueId = tempoIssueId
        AuthorAccountId = tempoUserId 
    }
    
    let tempoWorklogDescriptionBuilder = StringBuilder() 
    let azureCommitData = Azure.getAllCommits azureManager |> Async.RunSynchronously
    
    Array.iter (fun (commit: Azure.CommitData) ->
        printfn $"%s{commit.Comment}"
        tempoWorklogDescriptionBuilder.Append (commit.Comment + "\n") |> ignore
        ) azureCommitData
    
    tempoWorklogDescriptionBuilder.ToString()
    |> Tempo.createWorklog tempoManager
    |> Async.RunSynchronously
    |> printfn "%s"
    
    // in order to get which issue/useraccountid you're working in, you should run
    // https://api.tempo.io/4/worklogs?from=<yyyy-MM-dd>&to=<yyyy-MM-dd> and try to filter your worklog   
    
    0
