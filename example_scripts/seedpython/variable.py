# Local scope
def func1():
    x = 300
    print(x)  # 300


func1()


# Enclosed scope
def func2():
    x = 300

    def inner_func():
        print(x)  # 300

    inner_func()


func2()


# Global scope
width = 20
height = 5 * 9
print(width * height)  # 900

x = 300


def func3():
    print(x)  # 300


func3()
print(x)  # 300


def func4():
    x = 200  # Hides global variables
    print(x)  # 200


func4()
print(x)  # 300
