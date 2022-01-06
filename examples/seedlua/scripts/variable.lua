x = 10                  -- Global
local i = 1             -- Local to the chunk

while i <= x do
    local x = i * 2     -- Local to the while body
    x                   --> 2, 4, 6, 8, ...
    i = i + 1
end

if i > 20 then
    local x             -- Local to the "then" body
    x = 20
    x + 2
else
    x                   --> 10  (the global one)
end

x                       --> 10  (the global one)
