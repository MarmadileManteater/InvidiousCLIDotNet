namespace MarmadileManteater.InvidiousCLI

open System.Runtime.Loader
open System.Reflection
open System
open System.IO
open System.Collections.Generic
open MarmadileManteater.InvidiousCLI.Interfaces
open System.Linq
open MarmadileManteater.InvidiousCLI.Functions

module Plugins =
    type PluginLoadContext(pluginPath : string) =
        inherit AssemblyLoadContext()
        let _resolver : AssemblyDependencyResolver = new AssemblyDependencyResolver(pluginPath)
        override self.Load(assemblyName : AssemblyName) : Assembly =
            let assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName)
            if assemblyPath <> null then
                self.LoadFromAssemblyPath(assemblyPath)
            else
                null
        override self.LoadUnmanagedDll(unmanagedDllName : string) : IntPtr =
            let libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName)
            if libraryPath <> null then
                self.LoadUnmanagedDllFromPath(libraryPath)
            else
                IntPtr.Zero

    let LoadPlugin (relativePath : string) : Assembly =
        let root = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)))
        let pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)))
        let loadContext = new PluginLoadContext(pluginLocation)
        loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)))
        

    let CreateGenericType<'T> (assembly : Assembly): IList<'T>  =
        let results = new List<'T>()
        let mutable count = 0
        for assemblyType : Type in assembly.GetTypes() do
            if typeof<'T>.IsAssignableFrom(assemblyType) then
                let result = Activator.CreateInstance(assemblyType)
                if result <> null then
                    count <- count + 1
                    results.Add(result :?> 'T)
        if count = 0 then
            let availableTypes = assembly.GetTypes().Select(fun t -> t.FullName) |> String.concat ","
            raise(new ApplicationException($"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                                           $"Available types: {availableTypes}"))
        results

    let CreateCommands (assembly : Assembly): IList<ICommand>  =
        CreateGenericType(assembly) 