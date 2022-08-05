namespace MarmadileManteater.InvidiousCLI.Environment

open System.IO
open System

module Paths =
    let AppData =
        let environmentAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) 
        if environmentAppData <> "" then
            environmentAppData
        else
            // if app data is not found for this environment, use the .config subdirectory of your home directory
            Path.Join(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), ".config")
    let LocalDataDirectory = Path.Join(AppData, "MarmadileManteater")
    let ProgramLocalData = Path.Join(LocalDataDirectory, "InvidiousCLI-F#")
    let UserDataPath = Path.Join(ProgramLocalData, "user-data.json")
    let Temp = Path.Join(Path.GetTempPath(), "InvidiousCLI-F#")