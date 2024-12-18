module AutoTimeLogger.Program

open LibGit2Sharp
open FSharp.Configuration
open System
open AutoTimeLogger.Types
open AutoTimeLogger.Git
open System.Globalization
open System.Text.Json
open System.IO

type Config = YamlConfig<"config.yaml">

let parseDate str =
    DateTime.ParseExact(str, "dd-MM-yyyy", CultureInfo.InvariantCulture)

let config = Config()

let dates = config.Git.Dates |> Seq.map parseDate

let commits =
    config.Git.RepoPaths
    |> Seq.map (fun path ->
        let repo = new Repository(path)

        let commits =
            dates
            |> Seq.map (listCommits repo config.Git.Author)
            |> Seq.collect (fun commitsAtDate -> commitsAtDate)

        commits
    )
    |> Seq.collect (fun commitsPerRepo -> commitsPerRepo)
    |> Seq.sortBy (fun commit -> commit.Date)
    |> Seq.toArray

let log = { Commits = commits }

let json =
    JsonSerializer.Serialize(log, JsonSerializerOptions(WriteIndented = true))

let logFilePath = sprintf $"{config.Git.LogFilePath}\\log.json"

File.WriteAllText(logFilePath, json)
