"spam eggs"  # spam eggs
"doesn't"  # doesn\'t
"doesn't"  # doesn't
'"Yes," they said.'  # "Yes," they said.
'"Yes," they said.'  # "Yes," they said.
'"Isn\'t," they said.'  # "Isn\'t," they said.

"""
Usage: thingy [OPTIONS]
     -h                        Display this usage message
     -H hostname               Hostname to connect to
"""

3 * "un" + "ium"  # unununium
"Py" "thon"  # Python

prefix = "Py"
prefix + "thon"  # Python

word = "Python"
word[0]  # P
word[5]  # n
word[-1]  # n
word[-2]  # o
word[-6]  # P

s = "supercalifragilisticexpialidocious"
len(s)  # 34

word[42]  # IndexError: string index out of range
word[0] = "J"  # TypeError: 'str' object does not support item assignment
