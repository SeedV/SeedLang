def fib(n):
    """Print a Fibonacci series up to n."""
    a, b = 0, 1
    while a < n:
        a
        a, b = b, a + b


fib(2000)  # 0 1 1 2 3 5 8 13 21 34 55 89 144 233 377 610 987 1597
fib  # <function fib at 10042ed0>
f = fib
f(100)  # 0 1 1 2 3 5 8 13 21 34 55 89


def fib_list(n):
    """Return a list containing the Fibonacci series up to n."""
    result = []
    a, b = 0, 1
    while a < n:
        result.append(a)
        a, b = b, a + b
    return result


fib_list(100)  # [0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89]


def fib_recursive(n):
    if n == 1 or n == 2:
        return 1
    else:
        return fib_recursive(n - 1) + fib_recursive(n - 2)


fib_recursive(10)  # 55


def my_func():
    """Do nothing, but document it.

    No, really, it doesn't do anything.
    """
    pass
