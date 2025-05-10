SET QUOTED_IDENTIFIER ON;
GO

-- First, ensure the Admin role exists
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
END
GO

-- Get the Admin role ID, update user, and add to role in a single batch
DECLARE @AdminRoleId NVARCHAR(450);
SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin';

-- Update the user's IsAdmin flag
UPDATE AspNetUsers
SET IsAdmin = 1
WHERE Email = '123@gmail.com';

-- Add the user to the Admin role
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, @AdminRoleId
FROM AspNetUsers
WHERE Email = '123@gmail.com'
AND NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles 
    WHERE UserId = AspNetUsers.Id 
    AND RoleId = @AdminRoleId
);
GO 