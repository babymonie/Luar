HttpServer:AddGetRoute("/", function(req, res)
    local response = "<html><body><h1>Welcome to the home page!</h1></body></html>"
    res.OutputStream:Write(response, 0, #response)
    res:Close()
end)

HttpServer:AddGetRoute("/hello", function(req, res)
    local response = "<html><body><h1>Hello from the server!</h1></body></html>"
    res.OutputStream:Write(response, 0, #response)
    res:Close()
end)

HttpServer:AddPostRoute("/", function(req, res)
    --api test
    local body = HttpServer:GetRequestBody(req)
    local json = JSON:Parse(body)

    local response = JSON:Stringify(json)
    res.OutputStream:Write(response, 0, #response)
    res:Close()


end)

-- Start the HTTP server
HttpServer:Start("http://localhost:8080/")


-- Wait for the user to press a key
print("Press any key to stop the server...")
io.read()
