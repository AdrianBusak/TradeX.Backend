INSERT INTO Claim (id, name) 
SELECT
    Id = newid(),
    t.*
FROM
(
    SELECT TOP 0 Name = ''
    UNION ALL SELECT Name = '*'
    UNION ALL SELECT Name = 'entity.Tenant.read'
    UNION ALL SELECT Name = 'entity.Tenant.write'
    UNION ALL SELECT Name = 'entity.Tenant.delete'

	UNION ALL SELECT Name = 'entity.TenantUser.read'
    UNION ALL SELECT Name = 'entity.TenantUser.write'
    UNION ALL SELECT Name = 'entity.TenantUser.delete'
    
    UNION ALL SELECT Name = 'entity.keyname.read'
    UNION ALL SELECT Name = 'entity.keyname.write'
    UNION ALL SELECT Name = 'entity.keyname.delete'

    UNION ALL SELECT Name = 'entity.Patient.read'
    UNION ALL SELECT Name = 'entity.Patient.write'
    UNION ALL SELECT Name = 'entity.Patient.delete'
    
    UNION ALL SELECT Name = 'entity.injurytype.read'
    UNION ALL SELECT Name = 'entity.injurytype.write'
    UNION ALL SELECT Name = 'entity.injurytype.delete'

    UNION ALL SELECT Name = 'entity.Encounter.read'
    UNION ALL SELECT Name = 'entity.Encounter.write'
    UNION ALL SELECT Name = 'entity.Encounter.delete'

    UNION ALL SELECT Name = 'entity.EncounterPatientDetails.read'
    UNION ALL SELECT Name = 'entity.EncounterPatientDetails.write'
    UNION ALL SELECT Name = 'entity.EncounterPatientDetails.delete'

	UNION ALL SELECT Name = 'entity.EncounterInjury.read'
    UNION ALL SELECT Name = 'entity.EncounterInjury.write'
    UNION ALL SELECT Name = 'entity.EncounterInjury.delete'
	
	UNION ALL SELECT Name = 'entity.EncounterInjuryPelvicDetails.read'
    UNION ALL SELECT Name = 'entity.EncounterInjuryPelvicDetails.write'
    UNION ALL SELECT Name = 'entity.EncounterInjuryPelvicDetails.delete'
	

    UNION ALL SELECT Name = 'entity.EncounterEmergencyInterventionDetails.read'
    UNION ALL SELECT Name = 'entity.EncounterEmergencyInterventionDetails.write'
    UNION ALL SELECT Name = 'entity.EncounterEmergencyInterventionDetails.delete'

    UNION ALL SELECT Name = 'entity.EncounterEmergencyIntervention.read'
    UNION ALL SELECT Name = 'entity.EncounterEmergencyIntervention.write'
    UNION ALL SELECT Name = 'entity.EncounterEmergencyIntervention.delete'
    
    UNION ALL SELECT Name = 'entity.Encounterdocument.read'
    UNION ALL SELECT Name = 'entity.Encounterdocument.write'
    UNION ALL SELECT Name = 'entity.Encounterdocument.delete'
    
    UNION ALL SELECT Name = 'entity.user.invite'
    
    UNION ALL SELECT Name = 'entity.EmergencyInterventionType.write'
    UNION ALL SELECT Name = 'entity.EmergencyInterventionType.read'
    UNION ALL SELECT Name = 'entity.EmergencyInterventionType.delete'

    UNION ALL SELECT Name = 'entity.EmergencyInterventionSubType.write'
    UNION ALL SELECT Name = 'entity.EmergencyInterventionSubType.read'
    UNION ALL SELECT Name = 'entity.EmergencyInterventionSubType.delete'
) t
LEFT JOIN Claim old on t.name = old.name
WHERE
    old.id is null

DECLARE 
    @userRoleId as uniqueidentifier = '72D3E8AA-E2B4-4ECE-BFB4-75546F2A68FC',
    @administratorRoleId uniqueidentifier = 'BB1628EC-4537-42AF-86EC-6B49D366B4F9'

INSERT INTO Role (id, Name)
SELECT
    t.id,
    t.Name
FROM
(
    SELECT Id = @userRoleId, Name = 'User'
    UNION ALL SELECT Id = @administratorRoleId, Name = 'Administrator'
) t
LEFT JOIN Role old on t.name = old.name
WHERE
    old.id is null

DELETE FROM RolePermission

;WITH 
UserClaimsCTE AS
(
    SELECT Id FROM Claim 
    WHERE
        Name IN
        (
            'entity.Tenant.read',
            'entity.Tenant.write',
            
            'entity.keyname.read',

            'entity.EmergencyInterventionType.read',
            'entity.EmergencyInterventionSubType.read',

            'entity.injurytype.read',
                       
            'entity.Patient.read',
            'entity.patient.write',
            'entity.patient.delete',

            'entity.Encounter.read',
            'entity.Encounter.write',
            'entity.Encounter.delete',

            'entity.EncounterPatientDetails.read',
            'entity.EncounterPatientDetails.write',
            'entity.EncounterPatientDetails.delete',

            'entity.EncounterEmergencyInterventionDetails.read',
            'entity.EncounterEmergencyInterventionDetails.write',
            'entity.EncounterEmergencyInterventionDetails.delete',

			'entity.EncounterInjury.read',
            'entity.EncounterInjury.write',
            'entity.EncounterInjury.delete',

			'entity.EncounterInjuryPelvicDetails.read',
            'entity.EncounterInjuryPelvicDetails.write',
            'entity.EncounterInjuryPelvicDetails.delete',			
			
            'entity.EncounterEmergencyIntervention.read',
            'entity.EncounterEmergencyIntervention.write',
            'entity.EncounterEmergencyIntervention.delete',

            'entity.Encounterdocument.read',
            'entity.Encounterdocument.write',
            'entity.Encounterdocument.delete'
        )
),
AdministratorClaimsCTE AS
(
    SELECT Id FROM UserClaimsCTE
    UNION ALL
    SELECT Id FROM Claim 
    WHERE
        Name IN
        (
            'entity.Tenant.delete',
            
			'entity.TenantUser.read',
            'entity.TenantUser.write',
            'entity.TenantUser.delete',

            'entity.keyname.write',
            'entity.keyname.delete',

            'entity.injurytype.write',
            'entity.injurytype.delete',

            'entity.user.invite',
            
            'entity.EmergencyInterventionType.write',
            'entity.EmergencyInterventionType.delete',
            
            'entity.EmergencyInterventionSubType.write',
            'entity.EmergencyInterventionSubType.delete'
        )
)
INSERT INTO RolePermission(id, RoleId, ClaimId, CanExecute)
SELECT
    Id = newid(),
    t.*
FROM
    (
        SELECT RoleId = @userRoleId, ClaimId = t.Id, CanExecute = 1 FROM UserClaimsCTE t
        UNION ALL SELECT @administratorRoleId, ClaimId = t.Id, CanExecute = 1 FROM AdministratorClaimsCTE t
    ) t
    LEFT JOIN RolePermission old on t.RoleId = old.RoleId AND t.ClaimId = old.ClaimId
WHERE
    old.id is null
/*

delete from EncounterEmergencyIntervention
delete from EncounterEmergencyInterventionDetails
delete from EmergencyInterventionType
delete from [EmergencyInterventionSubType]
delete from [UserActiveTenant]
delete from [TenantUser]
delete from [EncounterPatientDetails]
delete from [EncounterEmergencyInterventionDetails]
delete from [EncounterDocument]
delete from [Encounter]
delete from Patient
delete from Tenant
delete from asastatus
*/
select * from TenantUser
--update TenantUser 
--set roleid = 'BB1628EC-4537-42AF-86EC-6B49D366B4F9'

