/* ============================================================================
   Roles-ConsolidateToThree.sql
   Renames the built-in admin roles and removes the sub-admin roles so the
   system uses exactly three roles:
       1  Student
       2  Administrator          (was "Admin")
       3  System Administrator   (was "SuperAdmin")
   Users previously holding "Content Manager" (4) or "Auditor" (5) are moved
   to "Administrator" (2) to preserve referential integrity, then roles 4/5
   and their permission mappings are deleted.
   ========================================================================= */
SET NOCOUNT ON;
PRINT N'=== Roles consolidation start ===';

/* 1) Rename built-ins */
UPDATE dbo.Roles SET RoleName = N'Administrator'        WHERE RoleId = 2 AND RoleName <> N'Administrator';
UPDATE dbo.Roles SET RoleName = N'System Administrator' WHERE RoleId = 3 AND RoleName <> N'System Administrator';

/* 2) Move any users off the sub-admin roles onto Administrator */
UPDATE dbo.UserRoles SET RoleId = 2 WHERE RoleId IN (4, 5);

/* 3) Delete sub-admin role permission mappings then the roles themselves */
DELETE FROM dbo.RolePermissions WHERE RoleId IN (4, 5);
DELETE FROM dbo.Roles           WHERE RoleId IN (4, 5);

/* 4) Report final state */
SELECT r.RoleId,
       r.RoleName,
       COUNT(ur.UserRoleId) AS AssignedUsers
FROM dbo.Roles r
LEFT JOIN dbo.UserRoles ur ON ur.RoleId = r.RoleId
GROUP BY r.RoleId, r.RoleName
ORDER BY r.RoleId;

SELECT RolePermissionId FROM dbo.RolePermissions WHERE RoleId IN (4, 5);
PRINT N'=== Roles consolidation done ===';
