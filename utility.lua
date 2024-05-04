-- Example usage of the utility functions

-- Sleep for 1 second (1000 milliseconds)
sleep(1000)

-- Set an interval to execute a function every 500 milliseconds
local intervalAction = function()
    print("Interval action executed!")
end
SetInterval(intervalAction, 500)

-- Set a timeout to execute a function after 2 seconds (2000 milliseconds)
local timeoutAction = function()
    print("Timeout action executed!")
end
SetTimeout(timeoutAction, 2000)

-- Write a message and wait for user input
WriteAndInput("Please enter something: ")

-- Wait for user input
Input()

-- Clear the console screen
Clear()

-- Exit the program
Exit()
