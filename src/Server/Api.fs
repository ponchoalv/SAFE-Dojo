module Api

open DataAccess
open FSharp.Data.UnitSystems.SI.UnitNames
open Giraffe
open Microsoft.AspNetCore.Http
open Saturn
open Shared


let private london = { Latitude = 51.5074; Longitude = 0.1278 }
let invalidPostcode next (ctx:HttpContext) =
    ctx.SetStatusCode 400
    text "Invalid postcode" next ctx

let getDistanceFromLondon next (ctx:HttpContext) = task {
    let! { Postcode = postcode } = ctx.BindModelAsync<PostcodeRequest>()
    if Validation.validatePostcode postcode then
        let! location = getLocation postcode
        let distanceToLondon = getDistanceBetweenPositions location.LatLong london
        return! json { Postcode = postcode; Location = location; DistanceToLondon = (distanceToLondon / 1000.<meter>) } next ctx
    else return! invalidPostcode next ctx }

let getCrimeReport next (ctx:HttpContext) = task {
        let! {LatLong = latlong } = ctx.BindModelAsync<Location>()
        let! reports = Crime.getCrimesNearPosition latlong
        let crimes =
            reports
            |> Array.countBy(fun r -> r.Category)
            |> Array.sortByDescending snd
            |> Array.map(fun (k, c) -> { Crime = k; Incidents = c })
        return! json crimes next ctx }

let private asWeatherResponse (weather:DataAccess.Weather.MetaWeatherLocation.Root) =
    { WeatherType =
        weather.ConsolidatedWeather
        |> Array.countBy(fun w -> w.WeatherStateName)
        |> Array.maxBy snd
        |> fst
        |> WeatherType.Parse;
      AverageTemperature = weather.ConsolidatedWeather |> Array.averageBy(fun r -> float r.TheTemp) }

let getWeather next (ctx:HttpContext) = task {  
    (* Task 4.1 WEATHER: Implement a function that retrieves the weather for
       the given postcode. Use the GeoLocation.getLocation, Weather.getWeatherForPosition and
       asWeatherResponse functions to create and return a WeatherResponse instead of the stub. *)

    let! {LatLong = latlong } = ctx.BindModelAsync<Location>()
    let! weather = getWeatherForPosition latlong
    return! json (weather |> asWeatherResponse) next ctx }

let apiRouter = scope {
    pipe_through (pipeline { set_header "x-pipeline-type" "Api" })
    
    post "/distance" getDistanceFromLondon
    
    post "/crime" getCrimeReport

    post "/weather" getWeather
    
    (* Task 1.0 CRIME: Add a new /crime/{postcode} endpoint to return crime data
       using the getCrimeReport web part function. Use the above distance
       route as an example of how to add a new route. *)    
        
    (* Task 4.2 WEATHER: Hook up the weather endpoint to the getWeather function. *)
    
    }
