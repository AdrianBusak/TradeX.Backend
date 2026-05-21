declare @caseId uniqueidentifier = '861322DC-78EA-4381-AB16-1358405FDE7D'

select 
	c.Id,
	c.TenantId,
	Tenant = t.[Name],
	-- ********* Case ************
	c.[Key],
	c.AdmittedAt,
	c.InjuredAt,
	c.DischargedAt,
	c.[Description],
	c.CreatedAt,
	c.CreatedByUserId,
	
-- ********* Patient Details ************
	c.PatientId,
	PatientKey = p.[Key],
	FirstName = cpd.FirstName,
	LastName = cpd.LastName,
	FullName = cpd.LastName + ', ' + cpd.FirstName,
	cpd.Age,
	cpd.BMI,
	AsaStatus = asa.[Key] + ' - ' + asa.[Name],
	cpd.HasAlcoholUseDisorder,
	cpd.HasAutoimmuneDisease,
	cpd.HealthInsuranceNumber, 
	cpd.HealthInsuranceCountry, 
	cpd.Sex, 
	cpd.Age, 
	cpd.Height, 
	cpd.[Weight],
	cpd.BMI, 
	cpd.Occupation, 
	cpd.AsaStatusId, 
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
	cpd.EntryDate, 
	cpd.Note,
	-- ********* Case Emergency Details ************
	ceid.IntubationBeforeAdmission, 
	ceid.IntubationAtAdmission, 
	ceid.Resuscitation, 
	ceid.VolumeReplacementMl, 
	ceid.EKUnitsReplacement, 
	ceid.FFPUnitsAcute, 
	ceid.PlateletsUnitsAcute, 
	ceid.FASTUltrasound,
	ceid.ChestXRay, 
	ceid.PelvisXRay, 
	ceid.MSCT, 
	ceid.PelvicBeltHS, 
	ceid.EmergencyExternalFixation, 
	ceid.PelvicPacking, 
	ceid.Embolization, 
	ceid.TransferredToOperationFromAdmission, 
	ceid.TransferredToICUFromAdmission, 
	ceid.TransferredToWardFromAdmission
from 
	[Case] c
	inner join Tenant t on c.TenantId = t.Id
	left join CasePatientDetails cpd on c.Id = cpd.Id
	left join CaseEmergencyInterventionDetails ceid on c.Id = ceid.Id
	left join Patient p on c.PatientId = p.Id
	left join AsaStatus asa on cpd.AsaStatusId = asa.Id
where
	c.Id = @caseId

select 
	-- ********* Case Injury ************
	ci.Id, ci.InjuryTypeId, 
	ci.InjuredAt, 
	ci.Note, 
	ci.EnergyLevelId, 
	ci.AccidentTypeId,
	EnergyLevel = el.[Key] + ' - ' + el.[Name],
	AccidentType = [at].[Key] + ' - ' + [at].[Name],
	
	-- ********* Case Injury Acetabular Details ************
	ciad.LetournelLeftId, 
	LetournelLeft = ltLeft.[Key] + ' - ' + ltLeft.[Name],
	ciad.LetournelRightId, 
	LetournelRight = ltRight.[Key] + ' - ' + ltRight.[Name],
	ciad.LetournelVariantId, 
	LetournelVariant = lv.[Key] + ' - ' + lv.[Name],
	ciad.PipkinLeftId, 
	PipkinLeft = ptLeft.[Key] + ' - ' + ptLeft.[Name],
	ciad.PipkinRightId, 
	PipkinRight = ptRight.[Key] + ' - ' + ptRight.[Name],
	ciad.HipDislocationLeftId, 
	HipDislocationLeft = hdtLeft.[Key] + ' - ' + hdtLeft.[Name],
	ciad.HipDislocationRightId, 
	HipDislocationRight = hdtRight.[Key] + ' - ' + hdtRight.[Name],
	ciad.ImpactionLeftId, 
	ImpactionLeft = isLeft.[Key] + ' - ' + isLeft.[Name],
	ciad.ImpactionRightId, 
	ImpactionRight = isRight.[Key] + ' - ' + isRight.[Name],
	
	ciad.AcetabularComminutionLeftId, 
	AcetabularComminutionLeft = acLeft.[Key] + ' - ' + acLeft.[Name],
	ciad.AcetabularComminutionRightId, 
	AcetabularComminutionRight = acRight.[Key] + ' - ' + acRight.[Name],
	
	ciad.IntraarticularFragmentLeftId, 
	IntraarticularFragmentLeft = ifLeft.[Key] + ' - ' + ifLeft.[Name],
	ciad.IntraarticularFragmentRightId, 
	IntraarticularFragmentRight = ifRight.[Key] + ' - ' + ifRight.[Name],
	
	ciad.BoneDefectLeftId, 
	BoneDefectLeft = bdLeft.[Key] + ' - ' + bdLeft.[Name],
	ciad.BoneDefectRightId, 
	BoneDefectRight = bdRight.[Key] + ' - ' + bdRight.[Name],
	
	ciad.LetournelVariantComment, 
	ciad.AcetabulumComment,

	-- ********* Case Injury Neurogical Details ************
	cind.AISGradeId, 
	AISGrade = ag.[Key] + ' - ' + ag.[Name],
	cind.NeurologicalStatusId, 
	NeurologicalStatus = ns.[Key] + ' - ' + ns.[Name],

	cind.PerinealSensationPresent,
	cind.AnalSphincterIntact, 
	cind.UrinarySphincterIntact, 
	cind.LumbosacralPlexusInjury, 
	cind.PeripheralNerveInjuryDescription,

	-- *********** Case Injury Open Injury Details **********
	cioid.GustiloAndersonId, 
	GustiloAnderson = ga.[Key] + ' - ' + ga.[Name],
	cioid.FaringerLeftId, 
	FaringerLeft = fzLeft.[Key] + ' - ' + fzLeft.[Name],
	cioid.FaringerRightId, 
	FaringerRight = fzRight.[Key] + ' - ' + fzRight.[Name],
	cioid.OpenPelvicFracture, 
	cioid.WoundContamination, 
	cioid.OpenWoundLocation,
	
	-- ************ Case Injury Pelvic Details ************
	cipd.PelvicInjuryMechanismId, 
	PelvicInjuryMechanism = pim.[Key] + ' - ' + pim.[Name],
	cipd.TileLeftId, 
	TileLeft = tLeft.[Key] + ' - ' + tLeft.[Name],
	cipd.TileRightId, 
	TileRight = tRight.[Key] + ' - ' + tRight.[Name],
	cipd.AOClassificationId, 
	AOClassification = aoc.[Key] + ' - ' + aoc.[Name],
	cipd.YoungBurgessId,
	YoungBurgess = yb.[Key] + ' - ' + yb.[Name],
	cipd.DenisLeftId, 
	DenisLeft = dzLeft.[Key] + ' - ' + dzLeft.[Name],
	cipd.DenisRightId, 
	DenisRight = dzRight.[Key] + ' - ' + dzRight.[Name],
	cipd.RoyCamilleId, 
	RoyCamille = rc.[Key] + ' - ' + rc.[Name],
	cipd.FFPClassificationId,
	FFPClassification = ffpc.[Key] + ' - ' + ffpc.[Name],
	cipd.SPDTypeId, 
	SPDType = spdt.[Key] + ' - ' + spdt.[Name],
	cipd.SymphysisDislocation
from 
	CaseInjury ci
	left join EnergyLevel el on ci.EnergyLevelId = el.Id
	left join AccidentType at on ci.AccidentTypeId = at.Id
	left join CaseInjuryAcetabularDetails ciad on ci.Id = ciad.Id
	left join LetournelType ltLeft on ciad.LetournelLeftId = ltLeft.id
	left join LetournelType ltRight on ciad.LetournelRightId = ltRight.id
	left join LetournelVariant lv on ciad.LetournelVariantId = lv.Id
	left join PipkinType ptLeft on ciad.PipkinLeftId = ptLeft.Id
	left join PipkinType ptRight on ciad.PipkinRightId= ptRight.Id
	left join HipDislocationType hdtLeft on ciad.HipDislocationLeftId = hdtLeft.Id
	left join HipDislocationType hdtRight on ciad.HipDislocationRightId = hdtRight.Id
	left join ImpactionSeverity isLeft on ciad.ImpactionLeftId = isLeft.Id
	left join ImpactionSeverity isRight on ciad.ImpactionRightId = isRight.Id
	left join AcetabularComminution acLeft on ciad.AcetabularComminutionLeftId = acLeft.Id
	left join AcetabularComminution acRight on ciad.AcetabularComminutionRightId = acRight.Id
	left join IntraarticularFragment ifLeft on ciad.IntraarticularFragmentLeftId = ifLeft.Id
	left join IntraarticularFragment ifRight on ciad.IntraarticularFragmentRightId = ifRight.Id
	left join BoneDefectVolume bdLeft on ciad.BoneDefectLeftId = bdLeft.Id
	left join BoneDefectVolume bdRight on ciad.BoneDefectRightId = bdRight.Id

	left join CaseInjuryNeurologicalDetails cind on ci.Id = cind.Id
	left join AISGrade ag on cind.AISGradeId = ag.Id
	left join NeurologicalStatus ns on cind.NeurologicalStatusId = ns.Id
	
	left join CaseInjuryOpenInjuryDetails cioid on ci.Id = cioid.Id
	left join GustiloAnderson ga on cioid.GustiloAndersonId = ga.Id
	left join FaringerZone fzLeft on cioid.FaringerLeftId = fzLeft.Id
	left join FaringerZone fzRight on cioid.FaringerRightId = fzRight.Id
	
	left join CaseInjuryPelvicDetails cipd on ci.Id = cipd.Id
	left join PelvicInjuryMechanism pim on cipd.PelvicInjuryMechanismId = pim.Id
	left join TileClassification tLeft on cipd.TileLeftId = tLeft.Id
	left join TileClassification tRight on cipd.TileRightId = tRight.Id
	left join AOClassification aoc on cipd.AOClassificationId = aoc.Id
	left join YoungBurgess yb on cipd.YoungBurgessId = yb.Id
	left join DenisZone dzLeft on cipd.DenisLeftId = dzLeft.Id
	left join DenisZone dzRight on cipd.DenisRightId = dzRight.Id
	left join RoyCamille rc on cipd.RoyCamilleId = rc.Id
	left join FFPClassification ffpc on cipd.FFPClassificationId = ffpc.Id
	left join SPDType spdt on cipd.SPDTypeId = spdt.Id
where
	ci.CaseId = @caseId	and ci.IsDeleted = 0