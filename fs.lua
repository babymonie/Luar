local content = fs:ReadFile("example.txt")
print("File content:", content)

-- Write to a file
fs:WriteFile("new_file.txt", "Hello, Luar!")

-- Append to a file
fs:AppendFile("new_file.txt", "\nThis is a new line.")

-- Delete a file
fs:DeleteFile("new_file.txt")

-- Create a directory
fs:CreateDirectory("new_directory")

-- Delete a directory
fs:DeleteDirectory("new_directory")

-- Copy a file
fs:CopyFile("source.txt", "destination.txt")

-- Move a file
fs:MoveFile("source.txt", "new_location.txt")

-- Move a directory
fs:MoveDirectory("source_directory", "new_location_directory")

-- Get a list of files in a directory
local files = fs:GetFiles("directory_path")
for _, file in ipairs(files) do
    print("File:", file)
end

-- Get a list of directories in a directory
local directories = fs:GetDirectories("directory_path")
for _, directory in ipairs(directories) do
    print("Directory:", directory)
end

-- Get the file name from a path
local fileName = fs:GetFileName("directory_path/file.txt")
print("File name:", fileName)

-- Get the directory name from a path
local directoryName = fs:GetDirectoryName("directory_path/subdirectory/file.txt")
print("Directory name:", directoryName)

-- Get the extension from a path
local extension = fs:GetExtension("directory_path/file.txt")
print("Extension:", extension)

-- Get the file name without extension from a path
local fileNameWithoutExtension = fs:GetFileNameWithoutExtension("directory_path/file.txt")
print("File name without extension:", fileNameWithoutExtension)

-- Get the full path
local fullPath = fs:GetFullPath("file.txt")
print("Full path:", fullPath)

-- Check if a file exists
local exists = fs:FileExists("file.txt")
print("File exists:", exists)

-- Check if a directory exists
local exists = fs:DirectoryExists("directory_path")
print("Directory exists:", exists)

-- Set the current directory
fs:SetCurrentDirectory("new_directory")

-- Get the current directory
local currentDirectory = fs:GetCurrentDirectory()
print("Current directory:", currentDirectory)

-- Get the parent directory
local parentDirectory = fs:GetParent("directory_path/subdirectory/file.txt")
print("Parent directory:", parentDirectory)

-- Get the root directory
local rootDirectory = fs:GetRoot("C:/directory_path/subdirectory/file.txt")
print("Root directory:", rootDirectory)

-- Get the logical drives
local logicalDrives = fs:GetLogicalDrives()
print("Logical drives:", logicalDrives)

-- Get the creation time of a file
local creationTime = fs:GetCreationTime("file.txt")
print("Creation time:", creationTime)

-- Create a file
fs:CreateFile("new_file.txt")

-- Create a file with specified buffer size
fs:CreateFile("new_file.txt", 1024)

-- Create a file with specified buffer size and options
fs:CreateFile("new_file.txt", 1024, lu.FileOptions.None)
