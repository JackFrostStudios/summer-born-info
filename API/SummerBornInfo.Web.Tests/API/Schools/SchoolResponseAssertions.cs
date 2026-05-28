namespace SummerBornInfo.Web.Tests.API.Schools;

internal static class SchoolResponseAssertions
{
    public static void AssertMatches(School expectedSchool, SchoolResponse actualSchool)
    {
        Assert.Equal(expectedSchool.Id, actualSchool.Id);
        Assert.Equal(expectedSchool.URN, actualSchool.URN);
        Assert.Equal(expectedSchool.UKPRN, actualSchool.UKPRN);
        Assert.Equal(expectedSchool.EstablishmentNumber, actualSchool.EstablishmentNumber);
        Assert.Equal(expectedSchool.Name, actualSchool.Name);
        Assert.Equal(expectedSchool.OpenDate, actualSchool.OpenDate);
        Assert.Equal(expectedSchool.CloseDate, actualSchool.CloseDate);

        Assert.Equal(expectedSchool.Address.Street, actualSchool.Address.Street);
        Assert.Equal(expectedSchool.Address.Locality, actualSchool.Address.Locality);
        Assert.Equal(expectedSchool.Address.AddressThree, actualSchool.Address.AddressThree);
        Assert.Equal(expectedSchool.Address.Town, actualSchool.Address.Town);
        Assert.Equal(expectedSchool.Address.County, actualSchool.Address.County);
        Assert.Equal(expectedSchool.Address.PostCode, actualSchool.Address.PostCode);

        Assert.Equal(expectedSchool.PhaseOfEducation.Id, actualSchool.PhaseOfEducation.Id);
        Assert.Equal(expectedSchool.PhaseOfEducation.Code, actualSchool.PhaseOfEducation.Code);
        Assert.Equal(expectedSchool.PhaseOfEducation.Name, actualSchool.PhaseOfEducation.Name);

        Assert.Equal(expectedSchool.LocalAuthority.Id, actualSchool.LocalAuthority.Id);
        Assert.Equal(expectedSchool.LocalAuthority.Code, actualSchool.LocalAuthority.Code);
        Assert.Equal(expectedSchool.LocalAuthority.Name, actualSchool.LocalAuthority.Name);

        Assert.Equal(expectedSchool.EstablishmentType.Code, actualSchool.EstablishmentType.Code);
        Assert.Equal(expectedSchool.EstablishmentType.Name, actualSchool.EstablishmentType.Name);
        Assert.Equal(expectedSchool.EstablishmentGroup.Code, actualSchool.EstablishmentGroup.Code);
        Assert.Equal(expectedSchool.EstablishmentGroup.Name, actualSchool.EstablishmentGroup.Name);
        Assert.Equal(expectedSchool.EstablishmentStatus.Code, actualSchool.EstablishmentStatus.Code);
        Assert.Equal(expectedSchool.EstablishmentStatus.Name, actualSchool.EstablishmentStatus.Name);
    }
}
