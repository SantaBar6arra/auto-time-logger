module AutoTimeLogger.Program

open LibGit2Sharp
open FSharp.Configuration
open System
open System.Text.RegularExpressions
open AutoTimeLogger.Types

type Config = YamlConfig<"config.yaml">

let taskNumberRegex = Regex(@"[a-zA-Z]+\-\d+")

let filterByAuthorAndDate (author: string, date: DateTime) (commit: Commit) =
    commit.Author.Email = author && commit.Author.When.Date = date

let listCommits (repo: Repository) author date =
    let commitLog = repo.Commits.QueryBy(CommitFilter())
    let isMineTodayCommit = filterByAuthorAndDate (author, date)
    let mineTodayCommits = commitLog |> Seq.filter isMineTodayCommit

    mineTodayCommits
    |> Seq.map (fun commit ->
        let taskNumber = taskNumberRegex.Match commit.Message

        {
            Date = date
            TaskNumber = taskNumber.Groups[0].Value
            CommitMessage = commit.Message
        }
    )

let config = Config()
let dates = config.Git.Dates |> Seq.map DateTime.Parse

config.Git.RepoPaths
|> Seq.iter (fun path ->
    let repo = new Repository(path)

    let commits =
        dates
        |> Seq.map (listCommits repo config.Git.Author)
        |> Seq.collect (fun commitsAtDate -> commitsAtDate)

    commits
    |> Seq.iter (fun commit -> printfn $"%A{commit.Date} %s{commit.TaskNumber}: %s{commit.CommitMessage}")
)
