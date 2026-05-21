namespace TradeX.Reports.Encounter;

public class EncounterReportDto
{
    // Encounter & Tenant
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Tenant { get; set; }
    public string Key { get; set; }
    public DateTime? AdmittedAt { get; set; }
    public DateTime? InjuredAt { get; set; }
    public DateTime? DischargedAt { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }

    // Patient Details
    public Guid PatientId { get; set; }
    public string PatientKey { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public int? Age { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public double? BMI { get; set; }
    public string Occupation { get; set; }
    public string AsaStatus { get; set; }
    public string HealthInsuranceNumber { get; set; }
    public string HealthInsuranceCountry { get; set; }
    public string Sex { get; set; }

    // Comorbidities
    public bool IsSmoker { get; set; }
    public bool HasAlcoholUseDisorder { get; set; }
    public bool HasOtherAddictions { get; set; }
    public bool HasDiabetes { get; set; }
    public bool HasCOPD { get; set; }
    public bool HasChronicKidneyDisease { get; set; }
    public bool HasCardiovascularDisease { get; set; }
    public bool HasCerebrovascularDisease { get; set; }
    public bool HasDementia { get; set; }
    public bool HasMalignantDisease { get; set; }
    public bool HasHepaticInsufficiency { get; set; }
    public bool HasAutoimmuneDisease { get; set; }
    public bool HasImmunodeficiency { get; set; }
    public bool HasParkinsonsDisease { get; set; }
    public bool HasHistoryOfPelvicRadiation { get; set; }
    public bool HasHistoryOfAbdominalOrPelvicSurgery { get; set; }
    public bool HasOtherComorbidities { get; set; }
    public string Note { get; set; }

    // Emergency Details
    public bool? IntubationBeforeAdmission { get; set; }
    public bool? IntubationAtAdmission { get; set; }
    public bool? Resuscitation { get; set; }
    public int? VolumeReplacementMl { get; set; }
    public int? EKUnitsReplacement { get; set; }
    public int? FFPUnitsAcute { get; set; }
    public int? PlateletsUnitsAcute { get; set; }
    public bool? FASTUltrasound { get; set; }
    public bool? MSCT { get; set; }
    public bool? PelvicBeltHS { get; set; }
    public bool? EmergencyExternalFixation { get; set; }
    public bool? PelvicPacking { get; set; }
    public bool? Embolization { get; set; }

    public List<EncounterInjuryDto> Injuries { get; set; } = [];
}

public class EncounterInjuryDto
{
    public Guid Id { get; set; }
    public DateTime? InjuredAt { get; set; }
    public string Note { get; set; }
    public string EnergyLevel { get; set; }
    public string AccidentType { get; set; }

    // Acetabular
    public string LetournelLeft { get; set; }
    public string LetournelRight { get; set; }
    public string LetournelVariant { get; set; }
    public string PipkinLeft { get; set; }
    public string PipkinRight { get; set; }
    public string HipDislocationLeft { get; set; }
    public string HipDislocationRight { get; set; }
    public string AcetabulumComment { get; set; }

    // Neurological
    public string AISGrade { get; set; }
    public string NeurologicalStatus { get; set; }
    public bool? PerinealSensationPresent { get; set; }
    public bool? AnalSphincterIntact { get; set; }
    public string PeripheralNerveInjuryDescription { get; set; }

    // Pelvic
    public string PelvicInjuryMechanism { get; set; }
    public string TileLeft { get; set; }
    public string TileRight { get; set; }
    public string AOClassification { get; set; }
    public string YoungBurgess { get; set; }
    public bool SymphysisDislocation { get; set; }

    // Open Injury
    public string GustiloAnderson { get; set; }
    public bool? OpenPelvicFracture { get; set; }
    public string OpenWoundLocation { get; set; }
}