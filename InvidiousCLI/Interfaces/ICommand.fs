namespace MarmadileManteater.InvidiousCLI.Interfaces

open System.Collections.Generic
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Enums
open MarmadileManteater.InvidiousClient.Interfaces
open System

type ICommand =
    interface
    // the name of the command
    abstract Name : string
    abstract Match : MatchType
    // a single line description of the command
    abstract Description : string
    // a multi line doc for getting a little more info about the parameters used in the command
    abstract Documentation : IEnumerable<string>
    // the number of arguments to absolutely require
    abstract RequiredArgCount : int
    // execute the command
    // @param {IList<string>} arguments
    // @param {UserData} userData - the user data object used to interact with the data stored in user-data within plugins
    // @param {IInvidiousAPIClient} client - the client used to talk to invidious
    // @param {Action} processCommand - is a method for calling other commands recursively
    // @returns an int which will be interpreted as an error if it is -1 and a success if it is 0
    abstract Execute : IList<string> * UserData * IInvidiousAPIClient * bool * Action<IList<string>, IInvidiousAPIClient, UserData, bool> -> int
end
