module Client

open App
open Elmish
open Elmish.Debug
#if DEBUG
open Elmish.HMR
open Elmish.React
#endif


Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif

|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif

|> Program.run
