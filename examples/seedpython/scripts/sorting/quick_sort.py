# [[ Data ]]
a = [8, 1, 0, 5, 6, 3, 2, 4, 7, 1]

# [[ Index(start, end) ]]
def partition(start, end, a):
    # [[ Index ]]
    pivot_index = start
    # [[ Save ]]
    pivot = a[pivot_index]
    while start < end:
        while start < len(a) and a[start] <= pivot:
            start += 1
        while a[end] > pivot:
            end -= 1
        if (start < end):
            # [[ Swap(start, end) ]]
            a[start], a[end] = a[end], a[start]
        # [[ Swap(end, pivot_index) ]]
        a[end], a[pivot_index] = a[pivot_index], a[end]
    return end


# [[ Bounds(start, end) ]]
def quick_sort(start, end, a):
    if start < end:
        mid = partition(start, end, a)
        quick_sort(start, mid - 1, a)
        quick_sort(mid + 1, end, a)


quick_sort(0, len(a) - 1, a)
print(a)
