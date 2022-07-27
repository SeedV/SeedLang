sum = 0
i = 1
while i <= 10000000:
    sum = sum + i
    i = i + 1

print(sum)


def f():
    sum = 0
    i = 1
    while i <= 10000000:
        sum = sum + i
        i = i + 1
    return sum


print(f())
