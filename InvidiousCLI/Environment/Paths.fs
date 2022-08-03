namespace MarmadileManteater.InvidiousCLI.Environment

open System.IO
open System
module Paths =
    let LocalDataDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MarmadileManteater")
    let ProgramLocalData = Path.Join(LocalDataDirectory, "InvidiousCLI-F#")
    let UserDataPath = Path.Join(ProgramLocalData, "user-data.json")
    let Temp = Path.Join(Path.GetTempPath(), "InvidiousCLI-F#")