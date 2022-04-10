dict = {'Name': 'Zara', 'Age': 7, 'Class': 'First'}

print("dict['Name']:", dict['Name'])  # dict['Name']: Zara
print("dict['Age']:", dict['Age'])  # "dict['Age']: 7

print('Age' in dict)  # True
print('School' in dict)  # False

dict['Age'] = 8
dict['School'] = "DPS School"

print('Age' in dict)  # True
print('School' in dict)  # False

print("dict['Age']:", dict['Age'])  # dict['Age']: 8
print("dict['School']:", dict['School'])  # dict['School']: DPS School
