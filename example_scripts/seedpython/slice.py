str = "Python"

print(str[3])  # h
print(str[:])  # Python
print(str[-3:])  # hon
print(str[:-3])  # Pyt
print(str[::-2])  # nhy
print(str[-3::-2])  # hy
print(str[:-5:-2])  # nh
print(str[-4:-6:-2])  # t

array = list(range(10))

print(array[2])  # 2
print(array[:])  # [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
print(array[3:])  # [3, 4, 5, 6, 7, 8, 9]
print(array[:3])  # [0, 1, 2]
print(array[4:6])  # [4, 5]
print(array[::2])  # [0, 2, 4, 6, 8]
print(array[3::2])  # [3, 5, 7, 9]
print(array[:3:2])  # [0, 2]
print(array[4:6:2])  # [4]

print(array[15:])  # []
print(array[:-12])  # []

array[2:5] = (1, 2)
print(array)  # [0, 1, 1, 2, 5, 6, 7, 8, 9]
array[-4:-1] = (1, 2)
print(array)  # [0, 1, 1, 2, 5, 1, 2, 9]
array[-1:-4:-1] = (1, 2, 3)
print(array)  # [0, 1, 1, 2, 5, 3, 2, 1]
array[:10:] = (1, 2, 3, 4, 5)
print(array)  # [1, 2, 3, 4, 5]
array[::-1] = (1, 2, 3, 4, 5)
print(array)  # [5, 4, 3, 2, 1]
array[2:10:2] = (1, 2)
print(array)  # [5, 4, 1, 2, 2]

tuple = (1, 2, 3, 4, 5)
print(tuple[2])  # 3
print(tuple[:])  # (1, 2, 3, 4, 5)
print(tuple[3:])  # (4, 5)
print(tuple[:4])  # (1, 2, 3, 4)
print(tuple[1:4])  # (2, 3, 4)
print(tuple[::2])  # (1, 3, 5)
print(tuple[2::2])  # (3, 5)
print(tuple[:4:2])  # (1, 3)
print(tuple[1:4:2])  # (2, 4)

print(tuple[5:])  # ()
print(tuple[:-6])  # ()
