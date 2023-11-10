open System

let transitions = Map.empty |> Map.add "CLOSED" [["APP_PASSIVE_OPEN";"LISTEN"];
                                                 ["APP_ACTIVE_OPEN";"SYN_SENT"]]
                 |> Map.add "LISTEN" [["RCV_SYN";"SYN_RCVD"];
                                      ["APP_SEND";"SYN_SENT"];
                                      ["APP_CLOSE";"CLOSED"]]
                 |> Map.add "SYN_RCVD" [["APP_CLOSE";"FIN_WAIT_1"];
                                        ["RCV_ACK";"ESTABLISHED"]]
                 |> Map.add "SYN_SENT" [["RCV_SYN";"SYN_RCVD"];
                                        ["RCV_SYN_ACK";"ESTABLISHED"];
                                        ["APP_CLOSE";"CLOSED"]]
                 |> Map.add "ESTABLISHED" [["APP_CLOSE";"FIN_WAIT_1"];
                                           ["RCV_FIN";"CLOSE_WAIT"]]
                 |> Map.add "FIN_WAIT_1" [["RCV_FIN";"CLOSING"];
                                          ["RCV_FIN_ACK";"TIME_WAIT"]
                                          ["RCV_ACK";"FIN_WAIT_2"]]
                 |> Map.add "CLOSING" [["RCV_ACK";"TIME_WAIT"]]
                 |> Map.add "FIN_WAIT_2" [["RCV_FIN";"TIME_WAIT"]]
                 |> Map.add "TIME_WAIT" [["APP_TIMEOUT";"CLOSED"]]
                 |> Map.add "CLOSE_WAIT" [["APP_CLOSE";"LAST_ACK"]]
                 |> Map.add "LAST_ACK" [["RCV_ACK";"CLOSED"]]

let mutable currentState = "CLOSED"

let getNextState (event: string) =
    let mutable nextState = currentState
    let eventState = transitions.[currentState]

    for t in eventState do
        if t[0] = event then nextState <- t[1]
        
    if currentState = nextState then nextState <- "ERROR"
    currentState <- nextState

let processEvents (events: string[]) =
    currentState <- "CLOSED"
    try
        for event in events do
            getNextState event
    with KeyNotFoundException ->
        currentState <- "ERROR"
        
    currentState
    

[<EntryPoint>]
let main argv =
    let mutable input = ""
    while input = "" do
        let message = "Доступные события: APP_PASSIVE_OPEN, APP_ACTIVE_OPEN, APP_SEND, APP_CLOSE, APP_TIMEOUT, RCV_SYN, RCV_ACK, RCV_SYN_ACK, RCV_FIN, RCV_FIN_ACK\n" +
                      "Введите набор TCP событий через запятую: "
        printf $"{message}"
        input <- Console.ReadLine()
        
        let array = [| for event in input.Split(",") do
                           if event <> String.Empty then
                               event.Trim() |]
        currentState <- processEvents array
        
        printfn $"Состояние: {currentState}"
        
        printfn "------------------------------------"
        input <- ""
    1