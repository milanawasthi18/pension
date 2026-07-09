using System;
using PensionVault.EmployerMember.Service.Domain.Enums;

namespace PensionVault.EmployerMember.Service.Application.Dtos;

public record CreateEmployerRequest(
    string CompanyName,
    string RegistrationNumber,
    string? Industry,
    RemittanceFrequency RemittanceFrequency,
    string? ContactDetails
);

public record UpdateEmployerRequest(
    string CompanyName,
    string? Industry,
    RemittanceFrequency RemittanceFrequency,
    string? ContactDetails,
    EmployerStatus Status
);

public record EmployerResponse(
    Guid EmployerId,
    string CompanyName,
    string RegistrationNumber,
    string? Industry,
    int EnrolledMemberCount,
    RemittanceFrequency RemittanceFrequency,
    string? ContactDetails,
    EmployerStatus Status
);

public record CreateSchemeRequest(
    string SchemeName,
    SchemeType SchemeType,
    decimal EmployeeContributionRate,
    decimal EmployerContributionRate,
    decimal InterestRatePA,
    string? VestingSchedule
);

public record UpdateSchemeRequest(
    string SchemeName,
    decimal EmployeeContributionRate,
    decimal EmployerContributionRate,
    decimal InterestRatePA,
    string? VestingSchedule,
    SchemeStatus Status
);

public record SchemeResponse(
    Guid SchemeId,
    string SchemeName,
    SchemeType SchemeType,
    decimal EmployeeContributionRate,
    decimal EmployerContributionRate,
    decimal InterestRatePA,
    string? VestingSchedule,
    SchemeStatus Status
);

public record CreateMemberRequest(
    Guid UserId,
    string MembershipNumber,
    string Name,
    DateTime DateOfBirth,
    string? Gender,
    string? NationalIdRef,
    Guid EmployerId,
    DateTime JoiningDate,
    DateTime? DateOfRetirement,
    string? NomineeDetails
);

public record UpdateMemberRequest(
    string Name,
    string? Gender,
    string? NationalIdRef,
    DateTime? DateOfRetirement,
    string? NomineeDetails,
    MemberStatus Status
);

public record SelfEnrollMemberRequest(
    string NationalIdRef,
    DateTime DateOfBirth,
    string? Gender,
    Guid EmployerId,
    string? NomineeDetails
);

public record ApproveMemberRequest(
    string MembershipNumber,
    Guid EmployerId
);

public record MemberResponse(
    Guid MemberId,
    string MembershipNumber,
    string Name,
    DateTime DateOfBirth,
    string? Gender,
    string? NationalIdRef,
    Guid EmployerId,
    string EmployerName,
    DateTime JoiningDate,
    DateTime? DateOfRetirement,
    string? NomineeDetails,
    MemberStatus Status,
    string? ProfileImageUrl
);

public record FundAccountResponse(
    Guid AccountId,
    Guid MemberId,
    Guid SchemeId,
    string SchemeName,
    DateTime AccountOpenDate,
    decimal EmployeeContributionBalance,
    decimal EmployerContributionBalance,
    decimal InterestAccrued,
    decimal TotalBalance,
    decimal VestingPercent,
    string Status
);

public record UpdateBalanceRequest(
    decimal Delta,
    decimal EmployeeDelta,
    decimal EmployerDelta,
    decimal InterestDelta = 0
);
