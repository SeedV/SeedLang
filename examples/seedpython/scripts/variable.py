# Local scope
def func1():
    x = 300
    x  # 300


func1()


# Enclosed scope
def func2():
    x = 300

    def inner_func():
        x  # 300

    inner_func()


func2()


# Global scope
width = 20
height = 5 * 9
width * height  # 900

x = 300


def func3():
    x  # 300


func3()
x  # 300


def func4():
    x = 200  # Hides global variables
    x  # 200


func4()
x  # 300
