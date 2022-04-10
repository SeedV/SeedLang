array = [64, 34, 25, 12, 22, 11, 90]
n = len(array)
for i in range(n):
    for j in range(n - i - 1):
        if array[j] > array[j + 1]:
            # [[ Swap(array, j, j + 1) ]]
            array[j], array[j + 1] = array[j + 1], array[j]


print(array)


def bubble_sort(array):
    n = len(array)
    for i in range(n):
        for j in range(n - i - 1):
            if array[j] > array[j + 1]:
                # [[ Swap(array, j, j + 1)
                temp = array[j]
                array[j] = array[j + 1]
                array[j + 1] = temp
                # ]]
    return array


print(bubble_sort([64, 34, 25, 12, 22, 11, 90]))
