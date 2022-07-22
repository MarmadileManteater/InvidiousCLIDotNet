module  MarmadileManteater.InvidiousCLI.Paths

open System.IO
open System

let LocalDataDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MarmadileManteater")
let ProgramLocalData = Path.Join(LocalDataDirectory, "InvidiousCLI-F#")
let UserDataPath = Path.Join(ProgramLocalData, "user-data.json")