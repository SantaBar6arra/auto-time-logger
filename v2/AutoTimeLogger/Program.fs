module AutoTimeLogger

open LibGit2Sharp
open FSharp.Configuration
open System
open System.Text.RegularExpressions
open AutoTimeLogger.Types

type Config = YamlConfig<"config.yaml">

let taskNumberRegex = Regex(@"[a-zA-Z]+\-\d+")

let filterByAuthorAndDate (author: string, date: DateTime) (commit: Commit) =
    commit.Author.Email = author && commit.Author.When.Date = date

let listCommits path author date =
    let repo = new Repository(path)
    let commitLog = repo.Commits.QueryBy(CommitFilter())
    let isMineTodayCommit = filterByAuthorAndDate (author, date)
    let mineTodayCommits = commitLog |> Seq.filter isMineTodayCommit

    mineTodayCommits
    |> Seq.map (fun commit ->
        let taskNumber = taskNumberRegex.Match commit.Message

        {
            TaskNumber = taskNumber.Groups[0].Value
            CommitMessage = commit.Message
        }
    )

let config = Config()
let dates = config.Git.Dates |> Seq.map DateTime.Parse

config.Git.RepoPaths
|> Seq.iter (fun path ->
    dates
    |> Seq.iter (fun date ->
        let commitList = listCommits path config.Git.Author date

        commitList
        |> Seq.iter (fun commit -> printfn $"%s{commit.TaskNumber}: %s{commit.CommitMessage}")
    )
)
