namespace MarmadileManteater.InvidiousCLI.Interfaces

open System.Collections.Generic
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Enums
open MarmadileManteater.InvidiousClient.Interfaces
open System

type ICommand =
    interface
    /// <summary>The name of the command</summary>
    abstract Name : string
    /// <summary>
    /// The match type for the command (ex: Equals, StartsWith, EndsWith)
    /// </summary>
    abstract Match : MatchType
    /// <summary>
    /// A single line description of the command
    /// </summary>
    abstract Description : string
    /// <summary>
    /// A multi line doc for getting a little more info about the parameters used in the command
    /// </summary>
    abstract Documentation : IEnumerable<string>
    /// <summary>
    /// The number of arguments to absolutely require
    /// </summary>
    abstract RequiredArgCount : int
    /// <summary>
    /// Execute the command
    /// </summary>
    abstract Execute : args: IList<string> * userData: UserData * client: IInvidiousAPIClient * isInteractive : bool * processCommand: Action<IList<string>, IInvidiousAPIClient, UserData, bool> -> int
end
