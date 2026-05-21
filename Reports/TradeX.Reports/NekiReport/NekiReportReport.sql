-- declare @EncounterId uniqueidentifier = '861322DC-78EA-4381-AB16-1358405FDE7D'

-- PRVI SELECT: Encounter & Patient Details
select 
    c.Id,
    c.TenantId,
    Tenant = t.[Name],
    c.[Key],
    -- Kastanje u DATETIME rješava problem s DateTimeOffset mapiranjem
    AdmittedAt = CAST(c.AdmittedAt AS DATETIME),
    InjuredAt = CAST(c.InjuredAt AS DATETIME),
    DischargedAt = CAST(c.DischargedAt AS DATETIME),
    c.[Description],
    CreatedAt = CAST(c.CreatedAt AS DATETIME),
    c.CreatedByUserId,
    
    c.PatientId,
    PatientKey = p.[Key],
    FirstName = cpd.FirstName,
    LastName = cpd.LastName,
    FullName = cpd.LastName + ', ' + cpd.FirstName,
    cpd.Age,
    cpd.BMI,
    AsaStatus = asa.[Key] + ' - ' + asa.[Name],
    cpd.HealthInsuranceNumber, 
    cpd.HealthInsuranceCountry, 
    cpd.Sex, 
    cpd.Height, 
    cpd.[Weight],
    cpd.Occupation, 
    cpd.IsSmoker, 
    cpd.HasAlcoholUseDisorder, 
    cpd.HasOtherAddictions, 
    cpd.HasDiabetes, 
    cpd.HasCOPD, 
    cpd.HasChronicKidneyDisease, 
    cpd.HasCardiovascularDisease, 
    cpd.HasCerebrovascularDisease, 
    cpd.HasDementia, 
    cpd.HasMalignantDisease, 
    cpd.HasHepaticInsufficiency, 
    cpd.HasAutoimmuneDisease, 
    cpd.HasImmunodeficiency, 
    cpd.HasParkinsonsDisease, 
    cpd.HasHistoryOfPelvicRadiation, 
    cpd.HasHistoryOfAbdominalOrPelvicSurgery, 
    cpd.HasOtherComorbidities, 
    cpd.Note,

    ceid.IntubationBeforeAdmission, 
    ceid.IntubationAtAdmission, 
    ceid.Resuscitation, 
    ceid.VolumeReplacementMl, 
    ceid.EKUnitsReplacement, 
    ceid.FFPUnitsAcute, 
    ceid.PlateletsUnitsAcute, 
    ceid.FASTUltrasound,
    ceid.MSCT, 
    ceid.PelvicBeltHS, 
    ceid.EmergencyExternalFixation, 
    ceid.PelvicPacking, 
    ceid.Embolization
from 
    [Encounter] c
    inner join Tenant t on c.TenantId = t.Id
    left join EncounterPatientDetails cpd on c.Id = cpd.Id
    left join EncounterEmergencyInterventionDetails ceid on c.Id = ceid.Id
    left join Patient p on c.PatientId = p.Id
    left join AsaStatus asa on cpd.AsaStatusId = asa.Id
where
    c.Id = @EncounterId;

-- DRUGI SELECT: Injuries
select 
    ci.Id, 
    InjuredAt = CAST(ci.InjuredAt AS DATETIME), 
    ci.Note, 
    EnergyLevel = el.[Key] + ' - ' + el.[Name],
    AccidentType = [at].[Key] + ' - ' + [at].[Name],
    
    LetournelLeft = ltLeft.[Key] + ' - ' + ltLeft.[Name],
    LetournelRight = ltRight.[Key] + ' - ' + ltRight.[Name],
    LetournelVariant = lv.[Key] + ' - ' + lv.[Name],
    PipkinLeft = ptLeft.[Key] + ' - ' + ptLeft.[Name],
    PipkinRight = ptRight.[Key] + ' - ' + ptRight.[Name],
    HipDislocationLeft = hdtLeft.[Key] + ' - ' + hdtLeft.[Name],
    HipDislocationRight = hdtRight.[Key] + ' - ' + hdtRight.[Name],
    ciad.AcetabulumComment,

    AISGrade = ag.[Key] + ' - ' + ag.[Name],
    NeurologicalStatus = ns.[Key] + ' - ' + ns.[Name],
    cind.PerinealSensationPresent,
    cind.AnalSphincterIntact, 
    cind.PeripheralNerveInjuryDescription,

    GustiloAnderson = ga.[Key] + ' - ' + ga.[Name],
    cioid.OpenPelvicFracture, 
    cioid.OpenWoundLocation,
    
    PelvicInjuryMechanism = pim.[Key] + ' - ' + pim.[Name],
    TileLeft = tLeft.[Key] + ' - ' + tLeft.[Name],
    TileRight = tRight.[Key] + ' - ' + tRight.[Name],
    AOClassification = aoc.[Key] + ' - ' + aoc.[Name],
    YoungBurgess = yb.[Key] + ' - ' + yb.[Name],
    cipd.SymphysisDislocation
from 
    EncounterInjury ci
    left join EnergyLevel el on ci.EnergyLevelId = el.Id
    left join AccidentType at on ci.AccidentTypeId = at.Id
    left join EncounterInjuryAcetabularDetails ciad on ci.Id = ciad.Id
    left join LetournelType ltLeft on ciad.LetournelLeftId = ltLeft.id
    left join LetournelType ltRight on ciad.LetournelRightId = ltRight.id
    left join LetournelVariant lv on ciad.LetournelVariantId = lv.Id
    left join PipkinType ptLeft on ciad.PipkinLeftId = ptLeft.Id
    left join PipkinType ptRight on ciad.PipkinRightId= ptRight.Id
    left join HipDislocationType hdtLeft on ciad.HipDislocationLeftId = hdtLeft.Id
    left join HipDislocationType hdtRight on ciad.HipDislocationRightId = hdtRight.Id
    left join EncounterInjuryNeurologicalDetails cind on ci.Id = cind.Id
    left join AISGrade ag on cind.AISGradeId = ag.Id
    left join NeurologicalStatus ns on cind.NeurologicalStatusId = ns.Id
    left join EncounterInjuryOpenInjuryDetails cioid on ci.Id = cioid.Id
    left join GustiloAnderson ga on cioid.GustiloAndersonId = ga.Id
    left join EncounterInjuryPelvicDetails cipd on ci.Id = cipd.Id
    left join PelvicInjuryMechanism pim on cipd.PelvicInjuryMechanismId = pim.Id
    left join TileClassification tLeft on cipd.TileLeftId = tLeft.Id
    left join TileClassification tRight on cipd.TileRightId = tRight.Id
    left join AOClassification aoc on cipd.AOClassificationId = aoc.Id
    left join YoungBurgess yb on cipd.YoungBurgessId = yb.Id
where
    ci.EncounterId = @EncounterId and ci.IsDeleted = 0;