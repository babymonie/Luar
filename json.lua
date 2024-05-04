-- Example usage of the JSON class methods

-- Stringify an object
local obj = {name = "John", age = 30, city = "New York"}
local jsonString = json:Stringify(obj)
print("Stringified JSON:", jsonString)

-- Parse a JSON string
local parsedObj = json:Parse('{"name":"John","age":30,"city":"New York"}')
print("Parsed JSON:", parsedObj)

-- Serialize an object
local serializedJson = json:Serialize({name = "Alice", age = 25, city = "Los Angeles"})
print("Serialized JSON:", serializedJson)

-- Deserialize a JSON string
local deserializedObj = json:Deserialize('{"name":"Alice","age":25,"city":"Los Angeles"}')
print("Deserialized JSON:", deserializedObj)
