# [[ Data, VReverse ]]
a = [8, 1, 0, 5, 6, 3, 2, 4, 7, 1]

# [[ MaxNumber ]]
max = 10

# [[ TempData ]]
count = list(range(max + 1))
for i in range(len(count)):
    count[i] = 0

# [[ Result ]]
result = list(range(len(a)))
for i in range(len(result)):
    result[i] = 0

for i in a:
    count[i] += 1

for i in range(1, len(count)):
    count[i] += count[i - 1]

for i in range(len(a)):
    result[count[a[i]] - 1] = a[i]
    count[a[i]] -= 1

print(result)
