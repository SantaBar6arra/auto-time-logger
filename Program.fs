module AutoTimeLogger.Program

open System
open System.Text
open System.Globalization
open FSharp.CommandLine
open FSharp.Configuration

type Config = YamlConfig<"config.yaml">

let dateOption =
    commandOption {
        names [ "d"; "date" ]
        description "date for taking commitData and logging it, format: yyyy-MM-dd"
        takes (format "%s")
        defaultValue (DateTime.Now.ToString())
        suggests (fun _ -> [ Values [ DateTime.Now.ToString() ] ])
    }

let commentOption =
    commandOption {
        names [ "c"; "command" ]
        description "command to fulfill worklog message, goes in the end"
        takes (format "%s")
        defaultValue ""
    }

let log (cfg: Config) date (comment: string) = 
    let azureManager: Azure.AzureManager =
        { Username = cfg.Azure.User
          Token = cfg.Azure.Token
          ApiUri = cfg.Azure.BaseUri
          DateFormat = cfg.Azure.DateFormat }

    let tempoManager: Tempo.TempoManager =
        { Token = cfg.Tempo.Token
          ApiUri = cfg.Tempo.Uri
          IssueId = cfg.Tempo.IssueId
          AuthorAccountId = cfg.Tempo.UserId
          DateFormat = cfg.Tempo.DateFormat }

    let tempoWorklogDescriptionBuilder = StringBuilder()

    let azureCommitData =
        Azure.getAllCommits azureManager date |> Async.RunSynchronously

    Array.iter
        (fun (commit: Azure.CommitData) ->
            printfn $"%s{commit.Comment}"
            tempoWorklogDescriptionBuilder.Append(commit.Comment + "\n") |> ignore)
        azureCommitData

    tempoWorklogDescriptionBuilder.Append comment |> ignore

    tempoWorklogDescriptionBuilder.ToString()
    |> Tempo.createWorklog tempoManager date
    |> Async.RunSynchronously
    |> printfn "%s"

let logCommand =
    command {
        name "log"
        description "log time for the date specified appending a comment to worklog"

        opt date in dateOption
                    |> CommandOption.zeroOrExactlyOne
                    |> CommandOption.whenMissingUse (DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))

        opt comment in commentOption
                       |> CommandOption.zeroOrExactlyOne
                       |> CommandOption.whenMissingUse ""

        do
            let config = Config()
            let date = DateTime.ParseExact(date, config.App.DateFormat, CultureInfo.InvariantCulture)

            printfn $"loggin' time for date %A{date}, comment: %s{comment}"

            log config date comment

        return 0
    }


[<EntryPoint>]
let main argv =
    printfn $"%A{argv}"
    logCommand |> Command.runAsEntryPoint argv

    // in order to get which issue and useraccountid you're working in, you should run
    // https://api.tempo.io/4/worklogs?from=<yyyy-MM-dd>&to=<yyyy-MM-dd> and try to filter your worklog
