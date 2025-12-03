namespace StoreSimulatorGUI.Views

open Avalonia.Controls
open Avalonia.Markup.Xaml
open StoreSimulatorGUI.ViewModels
open System

type MainWindow() as this =
    inherit Window()

    do
        Console.WriteLine("=== MainWindow constructor called ===")
        try
            Console.WriteLine("=== Loading XAML ===")
            AvaloniaXamlLoader.Load(this)
            Console.WriteLine("=== XAML loaded ===")
            
            Console.WriteLine("=== Creating MainViewModel ===")
            this.DataContext <- MainViewModel()
            Console.WriteLine("=== MainViewModel created ===")
            
            Console.WriteLine("=== MainWindow initialized successfully ===")
        with ex ->
            Console.WriteLine($"ERROR in MainWindow: {ex.Message}")
            Console.WriteLine($"Stack: {ex.StackTrace}")
            if ex.InnerException <> null then
                Console.WriteLine($"Inner: {ex.InnerException.Message}")
            reraise()