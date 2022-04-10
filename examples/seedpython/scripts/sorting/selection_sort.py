# [[ Data ]]
a = [8, 1, 0, 5, 6, 3, 2, 4, 7, 1]

for i in range(len(a)):
    # [[ Index(min) ]]
    min = i
    for j in range(i + 1, len(a)):
        if a[min] > a[j]:
            min = j
    # [[ Swap(i, min) ]]
    a[i], a[min] = a[min], a[i]

print(a)
