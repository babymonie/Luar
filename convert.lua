
-- Example usage of the convert class methods

-- Convert to string
local str = converter:ToString(123)
print("String:", str)

-- Convert to integer
local num = converter:ToInt("456")
print("Integer:", num)

-- Convert to double
local dbl = converter:ToDouble("3.14")
print("Double:", dbl)

-- Convert to float
local flt = converter:ToFloat("2.718")
print("Float:", flt)

-- Convert to boolean
local bool = converter:ToBool("true")
print("Boolean:", bool)

-- Convert to char
local ch = converter:ToChar("A")
print("Char:", ch)

-- Convert to byte
local byt = converter:ToByte("65")
print("Byte:", byt)

-- Convert to short
local shrt = converter:ToShort("123")
print("Short:", shrt)
