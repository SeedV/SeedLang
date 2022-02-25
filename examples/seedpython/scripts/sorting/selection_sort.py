# [[ Data, VReverse ]]
a = [8, 1, 0, 5, 6, 3, 2, 4, 7, 1]

for i in range(len(a)):
    min = i
    for j in range(i + 1, len(a)):
        # [[ Compare ]]
        if a[min] > a[j]:
            min = j
    # [[ Swap ]]
    a[i], a[min] = a[min], a[i]

print(a)
