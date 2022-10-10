# Tower of Hanoi problem.
def move(n, source, target, auxiliary):
    if n <= 0:
        return
    move(n - 1, source, auxiliary, target)
    print('Tower ' + source + ' -> Tower ' + target)
    move(n - 1, auxiliary, target, source)


num = 3
move(num, 'A', 'C', 'B')
