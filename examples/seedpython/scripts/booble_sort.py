array = [64, 34, 25, 12, 22, 11, 90]
n = len(array)
i = 0
while i < n:
    j = 0
    while j < n - i - 1:
        if array[j] > array[j + 1]:
            temp = array[j]
            array[j] = array[j + 1]
            array[j + 1] = temp
        j = j + 1
    i = i + 1
array
