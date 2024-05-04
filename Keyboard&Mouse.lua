--Mouse

-- Move the mouse to absolute coordinates
mouse:MoveMouse(100, 100)

-- Move the mouse to relative coordinates
mouse:MoveMouseRelative(50, 50)

-- Perform a left click
mouse:LeftClick()

-- Perform a right click
mouse:RightClick()

-- Perform a middle click
mouse:MiddleClick()

-- Scroll the mouse wheel up
mouse:ScrollUp()

-- Scroll the mouse wheel down
mouse:ScrollDown()

--Keyboard 
keyboard:SendKey(lu.Keyboard.DirectXKeyStrokes.DIK_A, false, lu.Keyboard.InputType.Keyboard)

-- Send a key release
keyboard:SendKey(lu.Keyboard.DirectXKeyStrokes.DIK_A, true, lu.Keyboard.InputType.Keyboard)

-- Send a key press with custom scan code
keyboard:SendKey(0x1E, false, lu.Keyboard.InputType.Keyboard)

-- Send a key release with custom scan code
keyboard:SendKey(0x1E, true, lu.Keyboard.InputType.Keyboard)
