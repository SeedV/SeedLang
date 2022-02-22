# [[ Data ]]
a = [8, 1, 0, 5, 6, 3, 2, 4, 7, 1]


def merge_sort(a):
    if len(a) > 1:
        # [[ Partition ]]
        mid = len(a) // 2
        # [[ Temp Data ]]
        left = a[:mid]
        # [[ Temp Data ]]
        right = a[mid:]
        merge_sort(left)
        merge_sort(right)

        i = j = k = 0
        while i < len(left) and j < len(right):
            # [[ Compare ]]
            if left[i] < right[j]:
                # [[ Copy ]]
                a[k] = left[i]
                i += 1
            else:
                # [[ Copy ]]
                a[k] = right[j]
                j += 1
            k += 1

        while i < len(left):
            # [[ Copy ]]
            a[k] = left[i]
            i += 1
            k += 1

        while j < len(right):
            # [[ Copy ]]
            a[k] = right[j]
            j += 1
            k += 1


merge_sort(a)
print(a)
