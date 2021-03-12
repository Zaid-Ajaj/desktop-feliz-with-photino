module App

open Feliz
open Feliz.UseDeferred 

[<ReactComponent>] 
let SystemInfo() = 
    let data = React.useDeferred(Server.Api.SystemInfo(), [| |])
    match data with
    | Deferred.HasNotStartedYet -> Html.none
    | Deferred.InProgress -> Html.h1 "Loading system info"
    | Deferred.Failed error -> Html.span error.Message
    | Deferred.Resolved system -> 
        Html.div [
            Html.h1 $"Platform: {system.Platform}"
            Html.h3 [
                prop.text  $"Version: {system.Version}"
                prop.style [ style.color.mediumAquamarine ]
            ]
        ]