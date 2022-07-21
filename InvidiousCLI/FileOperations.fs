module MarmadileManteater.InvidiousCLI.FileOperations

open System
open System.IO
open MarmadileManteater.InvidiousCLI.Objects
open Newtonsoft.Json
open Newtonsoft.Json.Linq

let saveUserData(userData : UserData) =
    let localDataDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MarmadileManteater")
    let programLocalData = Path.Join(localDataDirectory, "InvidiousCLI-F#")
    let userDataPath = Path.Join(programLocalData, "user-data.json")
    File.WriteAllText(userDataPath, JsonConvert.SerializeObject(userData.GetData()))

let getExistingUserData(firstTimeSetupCallback : Action<UserData>) =
    let mutable isFirstTimeSetup = false
    let localDataDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MarmadileManteater")
    let programLocalData = Path.Join(localDataDirectory, "InvidiousCLI-F#")
    let userDataPath = Path.Join(programLocalData, "user-data.json")
    if Directory.Exists(localDataDirectory) <> true then
        Directory.CreateDirectory(localDataDirectory) |> ignore
    if Directory.Exists(programLocalData) <> true then
        Directory.CreateDirectory(programLocalData) |> ignore
    if File.Exists(userDataPath) <> true then
        // first time setup
        let jsonFile = File.Create(userDataPath)
        jsonFile.Close()
        File.WriteAllText(userDataPath, "{}")
        isFirstTimeSetup <- true
    let userDataJObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(userDataPath))
    let userData = new UserData(userDataJObject)
    if isFirstTimeSetup then
        firstTimeSetupCallback.Invoke(userData)
        userData
    else
        userData