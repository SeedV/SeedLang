function max(num1, num2)
    if (num1 > num2) then
       result = num1;
    else
       result = num2;
    end
    return result;
end

max(10, 4)      -- 10
max(5, 6)       -- 6

function func3()
    local num3 = 44
    function func4()
        return num3
    end
    return func4
end

local func = func3()
func()          -- 44
