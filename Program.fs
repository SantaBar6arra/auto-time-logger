module AutoTimeLogger.Program

open System
open System.Text
open System.Globalization
open FSharp.CommandLine
open FSharp.Configuration

let dateOption =
    commandOption {
        names [ "d"; "date" ]
        description "date for taking commitData and logging it, format: yyyy-MM-dd"
        takes (format "%s")
        defaultValue (DateTime.Now.ToString())
        suggests (fun _ -> [ Values [ DateTime.Now.ToString() ] ])
    }

let commandOption =
    commandOption {
        names [ "c"; "command" ]
        description "command to fulfill worklog message, goes in the end"
        takes (format "%s")
        defaultValue ""
    }

let logCommand =
    command {
        name "log"
        description "log time for the date specified appending a comment to worklog"

        opt date in dateOption
                    |> CommandOption.zeroOrExactlyOne
                    |> CommandOption.whenMissingUse (DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))

        opt command in commandOption
                       |> CommandOption.zeroOrExactlyOne
                       |> CommandOption.whenMissingUse ""

        do
            let d = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            printfn $"%A{d}"

        return 0
    }

type Config = AppSettings<"app.config">

[<EntryPoint>]
let main argv =
    // printfn $"%A{argv}"
    // logCommand |> Command.runAsEntryPoint argv

    let azureManager: Azure.AzureManager =
        { Username = Config.AzureUser
          Token = Config.AzureToken
          ApiUri = Config.AzureBaseUri
          DateFormat = Config.AzureDateFormat }

    let tempoManager: Tempo.TempoManager =
        { Token = Config.TempoToken
          ApiUri = Config.TempoUri
          IssueId = Config.TempoIssueId
          AuthorAccountId = Config.TempoUserId
          DateFormat = Config.TempoDateFormat }

    let date =
        DateTime.ParseExact(argv[0], Config.AppDateFormat, CultureInfo.InvariantCulture)

    let tempoWorklogDescriptionBuilder = StringBuilder()

    let azureCommitData =
        Azure.getAllCommits azureManager date |> Async.RunSynchronously

    Array.iter
        (fun (commit: Azure.CommitData) ->
            printfn $"%s{commit.Comment}"
            tempoWorklogDescriptionBuilder.Append(commit.Comment + "\n") |> ignore)
        azureCommitData

    tempoWorklogDescriptionBuilder.Append argv[1] |> ignore

    tempoWorklogDescriptionBuilder.ToString()
    |> Tempo.createWorklog tempoManager date
    |> Async.RunSynchronously
    |> printfn "%s"

    // in order to get which issue and useraccountid you're working in, you should run
    // https://api.tempo.io/4/worklogs?from=<yyyy-MM-dd>&to=<yyyy-MM-dd> and try to filter your worklog

    0
