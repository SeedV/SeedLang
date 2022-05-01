a, b = 0, 1
while a < 10:
    print(a)
    a, b = b, a + b


print()


for ch in "string":
    if ch == "i":
        break
    print(ch)


for ch in "string":
    if ch == "i":
        continue
    print(ch)
