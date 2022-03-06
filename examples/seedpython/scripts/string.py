print('TODO: Fix the parsing error that comments cannot be the first line of scripts.')

# The language doesn't support raw string like r"raw string".
# The language doesn't support unicode string like u"unicode string".
# The language doesn't support string templates like "...%()...", "...{}..." or
# f"...{}...".

print("\\n")  # \n
print("spam eggs")  # spam eggs
print('\tdoesn\'t')  # [\t]doesn't
print("doesn't")  # doesn't
print('"Yes," they said.')  # "Yes," they said.
print('"Isn\'t, "\nThey said.')  # "Isn't, "[\n]They said.

print(3 * "un" + "ium")  # unununium
print("Py" "thon")  # Python

prefix = "Py"
print(prefix + "thon")  # Python

word = "Python"
print(word[0])  # P
print(word[5])  # n
print(word[-1])  # n
print(word[-2])  # o
print(word[-6])  # P

s = "supercalifragilisticexpialidocious"
len(s)  # 34
