squares = [1, 4, 9, 16, 25]
squares  # [1, 4, 9, 16, 25]

squares[0]  # 1
squares[-1]  # 25

cubes = [1, 8, 27, 65, 125]
cubes[3] = 64
cubes  # [1, 8, 27, 64, 125]

cubes.append(216)
cubes.append(7 ** 3)
cubes  # [1, 8, 27, 64, 125, 216, 343]
