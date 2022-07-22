module MarmadileManteater.InvidiousCLI.FileOperations

open System
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Paths

let saveUserData(userData : UserData) =
    let localDataDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MarmadileManteater")
    let programLocalData = Path.Join(localDataDirectory, "InvidiousCLI-F#")
    let userDataPath = Path.Join(programLocalData, "user-data.json")
    File.WriteAllText(userDataPath, JsonConvert.SerializeObject(userData.GetData()))

let getExistingUserData(firstTimeSetupCallback : Action<UserData>) =
    let mutable isFirstTimeSetup = false
    if Directory.Exists(LocalDataDirectory) <> true then
        Directory.CreateDirectory(LocalDataDirectory) |> ignore
    if Directory.Exists(ProgramLocalData) <> true then
        Directory.CreateDirectory(ProgramLocalData) |> ignore
    if File.Exists(UserDataPath) <> true then
        // first time setup
        let jsonFile = File.Create(UserDataPath)
        jsonFile.Close()
        File.WriteAllText(UserDataPath, "{}")
        isFirstTimeSetup <- true
    let userDataJObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(UserDataPath))
    let userData = new UserData(userDataJObject)
    if isFirstTimeSetup then
        firstTimeSetupCallback.Invoke(userData)
        userData
    else
        userData