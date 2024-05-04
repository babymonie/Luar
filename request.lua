

-- Perform a GET request
local getResponse = request:Get("https://jsonplaceholder.typicode.com/posts/1")
print("GET response:", getResponse)

-- Perform a POST request
local postResponse = request:Post("https://jsonplaceholder.typicode.com/posts", '{"title": "foo","body": "bar","userId": 1}')
print("POST response:", postResponse)

-- Perform a PUT request
local putResponse = request:Put("https://jsonplaceholder.typicode.com/posts/1", '{"id": 1,"title": "foo","body": "bar","userId": 1}')
print("PUT response:", putResponse)

-- Perform a DELETE request
local deleteResponse = request:Delete("https://jsonplaceholder.typicode.com/posts/1")
print("DELETE response:", deleteResponse)
