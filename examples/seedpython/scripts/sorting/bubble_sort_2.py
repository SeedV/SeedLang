# [[ Data ]]
a = [8, 1, 0, 5, 6, 3, 2, 4, 7, 1]

for i in range(len(a)):
    for j in range(len(a) - i - 1):
        if a[j] > a[j + 1]:
            # [[ Swap(j, j+1)
            temp = a[j]
            a[j] = a[j + 1]
            a[j + 1] = temp
            # ]]

print(a)
