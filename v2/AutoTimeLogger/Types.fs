module AutoTimeLogger.Types

open System

type CommitLogData = {
    Date: DateTime
    TaskNumber: string
    CommitMessage: string
    TimeSpent: double
}

type CommitLog = { Commits: CommitLogData array }
