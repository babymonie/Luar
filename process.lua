process:Start("notepad.exe")

-- Start a process by path with arguments
process:Start("notepad.exe", "example.txt")

-- Kill a process by ID
process:Kill(1234)

-- Kill a process by name
process:Kill("notepad")

-- Kill all processes
process:KillAll()

-- Check if a process is running by ID
local isRunning = process:IsRunning(1234)
print("Process is running:", isRunning)

-- Find a process by name and return its ID
local id = process:FindByName("notepad")
print("Process ID:", id)

-- Find a process by ID and return its name
local name = process:FindByInt(1234)
print("Process Name:", name)

-- Wait for a process to exit by ID
process:WaitForExit(1234)

-- Wait for a process to exit by name
process:WaitForExit("notepad")

-- Wait for all processes to exit
process:WaitForExitAll()

-- Close the main window of a process by ID
process:CloseMainWindow(1234)

-- Close the main window of a process by name
process:CloseMainWindow("notepad")

-- Close the main window of all processes
process:CloseMainWindowAll()

-- Close a process by ID
process:Close(1234)

-- Close a process by name
process:Close("notepad")

-- Close all processes
process:CloseAll()

-- Check if a process exists by ID
local exists = process:Exists(1234)
print("Process exists:", exists)

-- Check if a process exists by name
local exists = process:Exists("notepad")
print("Process exists:", exists)

-- Check if any processes exist
local existsAll = process:ExistsAll()
print("Processes exist:", existsAll)

-- Suspend a process by ID
process:Suspend(1234)

-- Suspend a process by name
process:Suspend("notepad")

-- Suspend all processes
process:SuspendAll()

-- Resume a process by ID
process:Resume(1234)

-- Resume a process by name
process:Resume("notepad")

-- Resume all processes
process:ResumeAll()
