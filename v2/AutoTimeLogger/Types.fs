module AutoTimeLogger.Types

open System

type CommitLogData = {
    Date: DateTime
    TaskNumber: string
    CommitMessage: string
}
