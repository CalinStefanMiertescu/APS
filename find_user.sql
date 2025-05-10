SELECT u.Id, u.UserName, u.Email, u.IsAdmin, u.IsModerator
FROM AspNetUsers u
WHERE u.Email = '123@gmail.com';

SELECT r.Name as RoleName, ur.UserId
FROM AspNetRoles r
JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
WHERE ur.UserId = 'your-user-id';  -- We'll replace this after getting your user ID 