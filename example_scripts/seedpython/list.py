squares = [1, 4, 9, 16, 25]
print(squares)  # [1, 4, 9, 16, 25]

print(squares[0])  # 1
print(squares[-1])  # 25

cubes = [1, 8, 27, 65, 125]
cubes[3] = 64
print(cubes)  # [1, 8, 27, 64, 125]

cubes.append(216)
cubes.append(7 ** 3)
print(cubes)  # [1, 8, 27, 64, 125, 216, 343]

print(8 in cubes)  # True
print(None in cubes)  # False

a = [1, 2, 3]
print(a)  # [1, 2, 3]

a[1] = [1, 2, 3]
print(a)  # [1, [1, 2, 3], 3]

a[1][1] = [1, 2, 3]
print(a)  # [1, [1, [1, 2, 3], 3], 3]

a[1][1][1] = 5
print(a)  # [1, [1, [1, 5, 3], 3], 3]
