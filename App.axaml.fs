namespace StoreSimulatorGUI

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Markup.Xaml
open StoreSimulatorGUI.Views
open System

type App() =
    inherit Application()

    override this.Initialize() =
        Console.WriteLine("=== App.Initialize() called ===")
        try
            AvaloniaXamlLoader.Load(this)
            Console.WriteLine("=== XAML loaded successfully ===")
        with ex ->
            Console.WriteLine($"ERROR in Initialize: {ex.Message}")
            Console.WriteLine($"Stack: {ex.StackTrace}")
            reraise()

    override this.OnFrameworkInitializationCompleted() =
        Console.WriteLine("=== OnFrameworkInitializationCompleted called ===")
        try
            match this.ApplicationLifetime with
            | :? IClassicDesktopStyleApplicationLifetime as desktop ->
                Console.WriteLine("=== Creating MainWindow ===")
                desktop.MainWindow <- MainWindow()
                Console.WriteLine("=== MainWindow created successfully ===")
            | _ -> 
                Console.WriteLine("=== Not desktop lifetime ===")
        with ex ->
            Console.WriteLine($"ERROR: {ex.Message}")
            Console.WriteLine($"Stack: {ex.StackTrace}")
            if ex.InnerException <> null then
                Console.WriteLine($"Inner: {ex.InnerException.Message}")

        base.OnFrameworkInitializationCompleted()