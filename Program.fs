namespace StoreSimulatorGUI

open System
open Avalonia

module Program =

    [<CompiledName "BuildAvaloniaApp">]
    let buildAvaloniaApp () =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .WithInterFont()

    [<EntryPoint; STAThread>]
    let main argv =
        Console.WriteLine("=== Application starting ===")
        try
            let result = 
                buildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(argv)
            Console.WriteLine($"=== Application exited with code: {result} ===")
            result
        with ex -> 
            Console.WriteLine($"FATAL ERROR: {ex.Message}")
            Console.WriteLine($"Stack trace: {ex.StackTrace}")
            if ex.InnerException <> null then
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}")
            Console.ReadLine() |> ignore
            1


            