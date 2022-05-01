seq = list(range(10))

print(seq[2])  # 2
print(seq[:])  # [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
print(seq[3:])  # [3, 4, 5, 6, 7, 8, 9]
print(seq[:3])  # [0, 1, 2]
print(seq[4:6])  # [4, 5]
print(seq[::2])  # [0, 2, 4, 6, 8]
print(seq[3::2])  # [3, 5, 7, 9]
print(seq[:3:2])  # [0, 2]
print(seq[4:6:2])  # [4]

print(seq[15:])  # []
print(seq[:-12])  # []

str = "Python"

print(str[3])  # h
print(str[:])  # Python
print(str[-3:])  # hon
print(str[:-3])  # Pyt
print(str[::-2])  # nhy
print(str[-3::-2])  # hy
print(str[:-5:-2])  # nh
print(str[-4:-6:-2])  # t
