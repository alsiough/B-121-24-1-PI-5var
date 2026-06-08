INSERT INTO Roles (Name) VALUES ('Admin'), ('Employer'), ('Employee')

INSERT INTO Users (Name, Email, RoleId)
VALUES ('Адміністратор', 'admin@test.com', 1)

INSERT INTO Users (Name, Email, RoleId)
VALUES ('Роботодавець', 'employer@test.com', 2)

INSERT INTO Users (Name, Email, RoleId)
VALUES ('Здобувач', 'worker@test.com', 3)