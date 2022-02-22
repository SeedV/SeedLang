# [[ Data ]]
a = [8, 1, 0, 5, 6, 3, 2, 4, 7, 1]

for i in range(1, len(a)):
    # [[ Store ]]
    key = a[i]
    j = i - 1
    while j >= 0 and key < a[j]:
        # [[ Shift ]]
        a[j + 1] = a[j]
        j -= 1
    # [[ Insert ]]
    a[j + 1] = key

print(a)
