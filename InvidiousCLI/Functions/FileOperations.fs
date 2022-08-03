﻿namespace MarmadileManteater.InvidiousCLI.Functions

open System
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Environment

module FileOperations =
    let SaveUserData(userData : UserData) =
        try
            Directory.CreateDirectory(Paths.AppData) |> ignore
        with
            ex -> ()
        try
            Directory.CreateDirectory(Paths.LocalDataDirectory) |> ignore
        with
            ex -> ()
        try
            Directory.CreateDirectory(Paths.ProgramLocalData) |> ignore
        with
            ex -> ()
        File.WriteAllText(Paths.UserDataPath, JsonConvert.SerializeObject(userData.GetData()))

    let GetExistingUserData(firstTimeSetupCallback : Action<UserData>) =
        let mutable isFirstTimeSetup = false
        if Directory.Exists(Paths.LocalDataDirectory) <> true then
            Directory.CreateDirectory(Paths.LocalDataDirectory) |> ignore
        if Directory.Exists(Paths.ProgramLocalData) <> true then
            Directory.CreateDirectory(Paths.ProgramLocalData) |> ignore
        if File.Exists(Paths.UserDataPath) <> true then
            // first time setup
            let jsonFile = File.Create(Paths.UserDataPath)
            jsonFile.Close()
            File.WriteAllText(Paths.UserDataPath, "{}")
            isFirstTimeSetup <- true
        let userDataJObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Paths.UserDataPath))
        let userData = new UserData(userDataJObject)
        if isFirstTimeSetup then
            firstTimeSetupCallback.Invoke(userData)
            userData
        else
            userData