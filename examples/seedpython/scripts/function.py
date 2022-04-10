def fib(n):
    a, b = 0, 1
    while a < n:
        print(a)
        a, b = b, a + b


fib(2000)  # 0 1 1 2 3 5 8 13 21 34 55 89 144 233 377 610 987 1597
print(fib)  # Func <fib>
f = fib
f(100)  # 0 1 1 2 3 5 8 13 21 34 55 89


def fib_list(n):
    result = []
    a, b = 0, 1
    while a < n:
        result.append(a)
        a, b = b, a + b
    return result


print(fib_list(100))  # [0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89]


def fib_recursive(n):
    if n == 1 or n == 2:
        return 1
    else:
        return fib_recursive(n - 1) + fib_recursive(n - 2)


print(fib_recursive(10))  # 55


def my_func():
    pass
