open System
open Expecto

let add x y = x + y

let appTests = testList "Application tests" [
    testCase "add works" <| fun _ ->
        let result = add 2 3
        Expect.equal result 5 "Result must be 5"
]

let tests = testList "All" [
    appTests
]

[<EntryPoint>]
let main argv = runTests defaultConfig tests