module AutoTimeLogger.Git

open System
open System.Text.RegularExpressions
open LibGit2Sharp
open AutoTimeLogger.Types

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
            TimeSpent = 0
        }
    )
